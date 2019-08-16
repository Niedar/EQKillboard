using System.Data;
using System.Data.SqlClient;
using Npgsql;

namespace EQKillboard.DiscordParser.Db {

    public class DatabaseConnection {
        public static IDbConnection CreateConnection(string connString)
            {
                var connection = new NpgsqlConnection(connString);
                return connection;
            }
    }
}