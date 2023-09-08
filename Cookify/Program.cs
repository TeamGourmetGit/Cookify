using Business.Repositories;
using Business.Repositories.Default;
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


// Add services to the container.
builder.Services.AddRazorPages();
builder.Services.AddServerSideBlazor();
builder.Services.AddHttpClient();

builder.Services.AddScoped<IRepository<Recipe>>(sp =>
{
    return new RecipeRepository(cosmosConnectionString, databaseName, containerName);
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

app.MapRecipeEndpoints();
app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

app.MapBlazorHub();
app.MapFallbackToPage("/_Host");

var seed = new Seed(cosmosConnectionString, databaseName, containerName);
await seed.InitializeAsync();

app.Run();
