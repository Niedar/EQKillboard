using System;
using System.Linq;
using System.Threading.Tasks;
using Discord;
using Discord.WebSocket;
using System.IO;
using Microsoft.Extensions.Configuration;
using EQKillboard.DiscordParser.Db;
using Dapper;
using EQKillboard.DiscordParser.Entities;
using System.Data;
using System.Collections.Generic;
using EQKillboard.DiscordParser.Models;
using EQKillboard.DiscordParser.Parsers;
using EQKillboard.DiscordParser.Scrapers;
using System.Globalization;
using Npgsql;
using System.Transactions;

namespace EQKillboard.DiscordParser
{
    public class Program
    {
        private IConfiguration configuration;
        private DiscordSocketClient client;
        private DiscordSocketConfig discordConfig;
        private string DbConnectionString;
        private DbService dbService;
        private IMessage lastRetrievedDiscordMsg;
        public static void Main(string[] args) {
            new Program().MainAsync().GetAwaiter().GetResult();
        }
 
        public async Task MainAsync()
        {

            // add json config for DBsettings, token, etc.
            var builder = new ConfigurationBuilder()
                            .SetBasePath(Directory.GetCurrentDirectory())
                            .AddJsonFile("appsettings.json"); 

            configuration = builder.Build(); 

            // call connection string from config
            DbConnectionString = configuration["ConnectionStrings:DefaultConnection"];
            dbService = new DbService();

            // create Config for Discord
            discordConfig = new DiscordSocketConfig {
                MessageCacheSize = 100
            };

            client = new DiscordSocketClient(discordConfig);
            client.Log += Log;
 
            string token = configuration["Tokens:userToken"];
            await client.LoginAsync(TokenType.User, token);
            await client.StartAsync();
            client.Ready += clientReadyHandler;
        
            client.MessageReceived += MessageReceived;

            // Block this task until the program is closed.
            await Task.Delay(-1);
        }
 
        private Task MessageReceived(SocketMessage message)
        {
            if (message.Channel.Name.IndexOf("yellowtext", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                Task.Run(() => ProcessMessage(message));
            }
            return Task.CompletedTask;
        }
 
        private Task Log(LogMessage msg)
        {
            Console.WriteLine(msg.ToString());
            return Task.CompletedTask;
        }

        private Task clientReadyHandler() {
            Task.Run(() => GetHistory());
            return Task.CompletedTask;
        }

        private async Task GetHistory()
        {
            var serverTime = new DateTimeOffset(DateTime.Now);

            var killmailGuild = client.Guilds.FirstOrDefault(x => x.Name == "Rise of Zek");
            var killmailChannel = killmailGuild.TextChannels.FirstOrDefault(x => x.Name.IndexOf("death_recap", StringComparison.OrdinalIgnoreCase) >= 0);

            // Retrieve first message
            if(killmailChannel != null) {
                var limit = 1;
                var messageCollToReceive = await killmailChannel.GetMessagesAsync(limit).Flatten();
                var messageToReceive = messageCollToReceive.ElementAt(0);
                lastRetrievedDiscordMsg = messageToReceive;
                await ProcessMessage(messageToReceive); // GetMessagesAsync ignores the message in the FROM parameter, so process this message now
            }

            int historyNumberOfDays;
            int.TryParse(configuration["Settings:HistoryLengthInDays"], out historyNumberOfDays);
            var historyLengthSetting = new TimeSpan(historyNumberOfDays, 0, 0, 0); // constructor with parameters: days, hours, minutes, seconds
            var messageLimit = 100;

            while(serverTime - lastRetrievedDiscordMsg.CreatedAt < historyLengthSetting)
            {
                    var messages = await killmailChannel.GetMessagesAsync(lastRetrievedDiscordMsg.Id, Direction.Before, messageLimit).Flatten();
                    
                    if (messages.Count() == 0)
                        break;

                    foreach (var message in messages) {
                        await ProcessMessage(message);
                        await Task.Delay(200);
                        lastRetrievedDiscordMsg = message;
                    }
            }
        }

        private async Task ProcessMessage(IMessage message)
        {
            try
            {
                ParsedKillMail parsedKillmail = null;
                
                parsedKillmail = DeathRecapParser.ParseKillmail(message.Content);
                if (parsedKillmail != null)
                {
                    
                    var existingRawKillMail = await dbService.GetRawKillMailAsync(message.Id);
                    if (existingRawKillMail != null)
                    {
                        return;
                    }

                    var insertedKillmail = await dbService.InsertRawAndParsedKillMailAsync(message, parsedKillmail);

                    // Get level and class for each char
                    var victimScraper = new CharBrowserScraper(parsedKillmail.VictimName);
                    var attackerScraper = new CharBrowserScraper(parsedKillmail.AttackerName);
                    await victimScraper.Fetch();
                    await attackerScraper.Fetch();

                    using(var connection = DatabaseConnection.CreateConnection(DbConnectionString)) {
                        if (victimScraper.Success)
                        {
                            var victim = new CharacterModel{
                                name = parsedKillmail.VictimName,
                                isAttacker = false,
                                className = victimScraper.Class,
                                level = victimScraper.Level
                            };
                            await InsertClassLevel(connection, victim, insertedKillmail, message);
                        }
                        if (attackerScraper.Success)
                        {
                            var attacker = new CharacterModel{
                                name = parsedKillmail.AttackerName,
                                isAttacker = true,
                                className = attackerScraper.Class,
                                level = attackerScraper.Level
                            };
                            await InsertClassLevel(connection, attacker, insertedKillmail, message);  
                        } 
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
 
        private async Task InsertClassLevel(IDbConnection connection, CharacterModel character, Killmail insertedKillmail, IMessage message) {            
            // Initialize variables for time testing as a base for relevance of data
            var historyLengthUpdateSetting = new TimeSpan(0, 12, 0, 0);
            var serverTime = new DateTimeOffset(DateTime.Now);
            var shouldUpdateKillmail = serverTime - message.CreatedAt < historyLengthUpdateSetting;
            
            await dbService.InsertOrUpdateClassAndLevel(character, insertedKillmail, shouldUpdateKillmail);

        }
    }
}