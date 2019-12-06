using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AngleSharp;

namespace EQKillboard.DiscordParser.Scrapers {
    public class CharBrowserScraper {
        private const string _charBrowserUrl = "https://riseofzek.com/charbrowser/index.php?page=search&name=";
        private const string _npcSearchUrl = "https://riseofzek.com/alla/?a=advanced_npcs&isearch=Search&iname=";
        private IConfiguration _config;
        private string _characterName;
        private string _classLevelResult;
        private bool _isNpc;
        private bool _success;

        public CharBrowserScraper(string characterName) {
            _characterName = characterName;
            _config = Configuration.Default.WithDefaultLoader();
        }

        public async Task Fetch() 
        {
            var address = _charBrowserUrl + _characterName;
            var document = await BrowsingContext.New(_config).OpenAsync(address);
            var cellSelector = "table.StatTable tbody";
            var tableBody = document.QuerySelector(cellSelector);
            
            string classLevel = string.Empty;
            if (tableBody != null)
            {
                // Loop over each row in the table looking at the first column to find an exact match on character name
                foreach (var tableRow in tableBody.Children.Where(x => x.LocalName == "tr"))
                {
                    var firstColumn = tableRow.Children.FirstOrDefault(x => x.LocalName == "td");
                    if (firstColumn != null && firstColumn.TextContent.Equals(_characterName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        var guildColumn = tableRow.Children.Where(x => x.LocalName == "td").Skip(1).FirstOrDefault();
                        var levelColumn = tableRow.Children.Where(x => x.LocalName == "td").Skip(2).FirstOrDefault();
                        var classColumn = tableRow.Children.Where(x => x.LocalName == "td").Skip(3).FirstOrDefault();

                        if (guildColumn != null && !string.IsNullOrEmpty(guildColumn.TextContent))
                        {
                            Guild = guildColumn.TextContent.Replace("<", "").Replace(">", "");
                        }
                        if (levelColumn != null && !string.IsNullOrEmpty(levelColumn.TextContent))
                        {
                            Level = Convert.ToInt32(levelColumn.TextContent);
                        }
                        if (classColumn != null && !string.IsNullOrEmpty(classColumn.TextContent))
                        {
                            Class = classColumn.TextContent;
                        }
                    }
                }
            }

            address = _npcSearchUrl + _characterName;
            document = await BrowsingContext.New(_config).OpenAsync(address);
            cellSelector = "div.page-content-ajax li a";
            var cellsAll = document.QuerySelectorAll(cellSelector);
            _isNpc = cellsAll.Any(x => x.TextContent.Equals(_characterName, StringComparison.InvariantCultureIgnoreCase));

            _success = true;
        }

        public string Class { get; private set; }
        public int? Level { get; private set; }
        public string Guild { get; private set; }

        public bool IsNpc
        {
            get
            {
                return _isNpc;
            }
        }

        public bool Success
        {
            get
            {
                return _success;
            }
        }
    }
}