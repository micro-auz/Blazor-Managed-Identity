using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BlazorApp1;
using BlazorApp1.Data;
using DataLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.Azure.Services.AppAuthentication;
using MySql.Data.MySqlClient;
using MySqlX.XDevAPI;
using System.Configuration;
using System.Net.NetworkInformation;
using static Org.BouncyCastle.Math.EC.ECCurve;
using ConfigurationManager = System.Configuration.ConfigurationManager;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<IDataAccessMySQL, DataAccessMySQL>();
builder.Services.AddSingleton<IConnectionStringBuilder, ConnectionStringBuilder>();
builder.Configuration.GetSection("ConnectionStrings");




/*Getting the Token
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

System.Diagnostics.Debug.WriteLine("The MySQL connectionstring1 is: " + mySqlConnectionStringBuilder.ConnectionString);
*/



if (builder.Environment.IsDevelopment())
{
    System.Diagnostics.Debug.WriteLine("This is development!");

    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}


var app = builder.Build();

// Configure the HTTP request pipeline. 
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();

app.UseStaticFiles();

app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

app.Run();
