using System;
using System.Collections.Generic;
using System.Text;
using System.Data.SqlClient;
using System.IO;

namespace ChatServer
{
    
    class DatabaseProvider
    {
        private static string GetConnnectionString()
        {
            var builder = new SqlConnectionStringBuilder
            {
                //1. указать источник
                DataSource = "(localdb)\\MSSQLLocalDB",
                //2. указать местораспалажения файла
                AttachDBFilename = Path.GetFullPath("Database.mdf"),
                IntegratedSecurity = true
            };
            return builder.ConnectionString;
        }
        public static bool IsValidUserInfo(UserInfo info)
        {
            string connecttionString = GetConnnectionString();
            // Установление соединения с БД
            try
            {

                using (SqlConnection connection = new SqlConnection(connecttionString))
                {
                    connection.Open();
                    string query = "SELECT Apartment_ID,Password FROM [Apartment_Security] WHERE Apartment_ID=@un AND Password=@up";
                    // Создание SQL команды
                    using (SqlCommand command = new SqlCommand(query, connection))
                    {
                        command.Parameters.AddWithValue("@un", info.username);
                        command.Parameters.AddWithValue("@up", info.password);
                        // Получение результатов этой команды
                        using (SqlDataReader reader = command.ExecuteReader())
                        {
                            return reader.HasRows;
                        }
                    }
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
