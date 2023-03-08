using Azure.Core;
using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using BlazorApp1.Data;
using DataLibrary;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using System.Net.NetworkInformation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddSingleton<WeatherForecastService>();
builder.Services.AddSingleton<IDataAccessMySQL, DataAccessMySQL>();
builder.Configuration.GetSection("ConnectionStrings");


if (builder.Environment.IsProduction())
{
    System.Diagnostics.Debug.WriteLine("This is production!");

    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}

if (builder.Environment.IsDevelopment())
{
    System.Diagnostics.Debug.WriteLine("This is development!");

    builder.Configuration.AddAzureKeyVault(
        new Uri($"https://{builder.Configuration["KeyVaultName"]}.vault.azure.net/"),
        new DefaultAzureCredential());
}



//Get secrets from Azure Keyvault
//https://learn.microsoft.com/en-us/azure/key-vault/general/tutorial-net-create-vault-azure-web-app
SecretClientOptions options = new SecretClientOptions()
{
    Retry =
        {
            Delay= TimeSpan.FromSeconds(2),
            MaxDelay = TimeSpan.FromSeconds(16),
            MaxRetries = 5,
            Mode = RetryMode.Exponential
         }
};

var client = new SecretClient(new Uri("https://kv-rg-auz-2.vault.azure.net/"), new DefaultAzureCredential(), options);
KeyVaultSecret secret = await client.GetSecretAsync("AppSecret");
string secretValue = secret.Value;
System.Diagnostics.Debug.WriteLine("This is the secret:" + secretValue);

//var connectionString = System.Configuration.ConfigurationManager.AppSettings["SqlConnectionString"];
var connectionString = builder.Configuration.GetConnectionString("SqlConnectionString");
System.Diagnostics.Debug.WriteLine("This is the connectionString:" + connectionString);

var connectionString2 = builder.Configuration["SqlConnectionString"];
System.Diagnostics.Debug.WriteLine("This is the connectionString2:" + connectionString2);

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
