using Microsoft.Azure.Services.AppAuthentication;
using MySql.Data.MySqlClient;
using System.Net;
using System.Text.Json;

namespace BlazorApp1
{
    public class ConnectionStringBuilder : IConnectionStringBuilder
    {

        private readonly IConnectionStringBuilder _connection;
        private readonly IConfiguration _config;

        public ConnectionStringBuilder(IConfiguration config)
        {


            //_connection = connection ?? throw new ArgumentNullException(nameof(connection));
            _config = config ?? throw new ArgumentNullException("config");

        }

        public async Task<string> GetConnectionString(bool isProduction)
        {

            var connectionString = "";

            if (!isProduction)
            {

                connectionString = _config.GetValue<string>("SqlConnectionString");
            }
            else
            {

                /*Getting the Token*/
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














        // Obtain connection string information from the portal
        //
        private static string Host = "mysql-rg-auz-2-auzw1-dev-eastus2-001.mysql.database.azure.com";
        private static string User = "web-app-mysql";
        private static string Database = "people";
        private static string ClientId = "be5cf291-d1af-4578-91eb-f5102001637e";

        public async Task<string> GetConnectionString2(bool isProduction)
        {
            //
            // Get an access token for MySQL.
            //
            System.Diagnostics.Debug.WriteLine("Getting access token from Azure Instance Metadata service...");

            var connectionString2 = "";

            // Azure AD resource ID for Azure Database for MySQL is https://ossrdbms-aad.database.windows.net/
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://169.254.169.254/metadata/identity/oauth2/token?api-version=2018-02-01&resource=https://ossrdbms-aad.database.windows.net&client_id=" + ClientId);
            request.Headers["Metadata"] = "true";
            request.Method = "GET";
            string accessToken = null;

            try
            {
                // Call managed identities for Azure resources endpoint.
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();

                // Pipe response Stream to a StreamReader and extract access token.
                StreamReader streamResponse = new StreamReader(response.GetResponseStream());
                string stringResponse = streamResponse.ReadToEnd();
                var list = JsonSerializer.Deserialize<Dictionary<string, string>>(stringResponse);
                accessToken = list["access_token"];
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("{0} \n\n{1}", e.Message, e.InnerException != null ? e.InnerException.Message : "Acquire token failed");
                System.Environment.Exit(1);
            }

            //
            // Open a connection to the MySQL server using the access token.
            //
            var builder = new MySqlConnectionStringBuilder
            {
                Server = Host,
                Database = Database,
                UserID = User,
                Password = accessToken,
                SslMode = MySqlSslMode.Required,
            };

            connectionString2 = builder.ConnectionString;

            using (var conn = new MySqlConnection(builder.ConnectionString))
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
                            System.Diagnostics.Debug.WriteLine("\nConnected!\n\nMySQL version: {0}", reader.GetString(0));
                        }
                    }
                }
            }

            return connectionString2;
        }





    }
}
