using Microsoft.Azure.Services.AppAuthentication;
using MySql.Data.MySqlClient;


namespace BlazorApp1
{
    public class ConnectionStringBuilder : IConnectionStringBuilder
    {

        private readonly IConnectionStringBuilder _connection;
        private readonly IConfiguration _config;

        public ConnectionStringBuilder(IConfiguration config) { 
        

            //_connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _config = config ?? throw new ArgumentNullException("config");  

        }    

        public async Task<string> GetConnectionString(bool isProduction)
        {

            var connectionString = "";

            if (!isProduction) {

                connectionString = _config.GetValue<string>("SqlConnectionString");
            }
            else
            {

                /*Gettting the Token*/
                var sqlServerTokenProvider = new AzureServiceTokenProvider();
                var SqlAccessToken = await sqlServerTokenProvider.GetAccessTokenAsync("https://ossrdbms-aad.database.windows.net");


                //
                // Open a connection to the MySQL server using the access token.
                //
                var mySqlConnectionStringBuilder = new MySqlConnectionStringBuilder
                {
                    Server = "mysql-rg-auz-2-auzw1-dev-eastus2-001.mysql.database.azure.com",
                    Database = "people",
                    UserID = "v-auzclement_microsoft.com#EXT#@StudiosCore.onmicrosoft.com",
                    Password = SqlAccessToken,
                    SslMode = MySqlSslMode.Required,

                };

                var responseMessage = "";

                using (var conn = new MySqlConnection(mySqlConnectionStringBuilder.ConnectionString))
                {
                    System.Diagnostics.Debug.WriteLine("Opening connection using access token...");
                    await conn.OpenAsync();

                    using (var command = conn.CreateCommand())
                    {
                        command.CommandText = "SELECT VERSION()";

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            while (await reader.ReadAsync())
                            {
                                System.Diagnostics.Debug.WriteLine("\nConnected to MySQL using managed identity!\n\nMySQL version: " + reader.GetString(0));
                                responseMessage = reader.GetString(0);
                            }
                        }
                    }
                }

                System.Diagnostics.Debug.WriteLine("The versions is Azure Database for Mysql is:" + responseMessage);

                System.Diagnostics.Debug.WriteLine("The MySQL connectionstring is: " + mySqlConnectionStringBuilder.ConnectionString);

                connectionString = mySqlConnectionStringBuilder.ConnectionString;

            }

            return connectionString;
        }



    }
}
