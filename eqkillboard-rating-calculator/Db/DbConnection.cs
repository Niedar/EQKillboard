using System;
using System.Data;
using System.Data.SqlClient;
using System.Reflection;
using Dapper;
using Npgsql;

namespace eqkillboard_rating_calculator.Db {

    public class DatabaseConnection {
        public static IDbConnection CreateConnection(string connString)
            {
                var connection = new NpgsqlConnection(connString);
                // SqlMapper.AddTypeHandler(new DateTimeTypeHandler());

                var mapper = new Func<Type, string, PropertyInfo>((type, columnName) => 
                            {
                            if (columnName == "hour")
                                return type.GetProperty("Timeslice");
                            else
                                return type.GetProperty(columnName);
                            });

                var KillmailModelMap = new CustomPropertyTypeMap(
                    typeof(KillmailModel),
                    (type, columnName) => mapper(type, columnName)
                    );

                SqlMapper.SetTypeMap(typeof(KillmailModel), KillmailModelMap);

                return connection;
            }
    }
}