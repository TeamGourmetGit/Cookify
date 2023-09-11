using Business.Repositories;
using Business.Repositories.Default;
using Cookify.Config;
using Cookify.Data;
using Cookify.Endpoints;
using Cookify.Helpers;
using Microsoft.AspNetCore.Hosting;
using Shared.Models;
using System.Configuration;

var builder = WebApplication.CreateBuilder(args);

string cosmosConnectionString = builder.Configuration.GetConnectionString("Database");
string databaseName = builder.Configuration.GetSection("CosmosDBConfig")["DatabaseName"];
string containerName = builder.Configuration.GetSection("CosmosDBConfig")["ContainerName"];
string logicAppEndpointUrl = builder.Configuration["LOGIC_APP_ENDPOINT_URL"];


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IRepository<Recipe>>(sp =>
{
    return new RecipeRepository(cosmosConnectionString, databaseName, containerName);
});

builder.Services.AddScoped(provider =>
{
    var configuration = provider.GetRequiredService<IConfiguration>();
    return new ConfigurationExtension
    {
        LogicAppEndpointUrl = logicAppEndpointUrl
    };
});

IServiceProvider serviceProvider = builder.Services.BuildServiceProvider();
ServiceLocator.Initialize(serviceProvider);


var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseSwagger();
app.UseSwaggerUI();

app.MapRecipeEndpoints();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var seed = new Seed(cosmosConnectionString, databaseName, containerName);
await seed.InitializeAsync();

app.Run();