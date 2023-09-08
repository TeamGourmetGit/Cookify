using Microsoft.Azure.Cosmos;
using Newtonsoft.Json;
using Shared.Models;
using System.Net;

namespace Cookify.Data
{
    public class Seed
    {
        private readonly CosmosClient cosmosClient;
        private readonly Database database;
        private readonly Container container;

        public Seed(string connectionString, string database, string container)
        {
            this.cosmosClient = new CosmosClient(connectionString);
            this.database = this.cosmosClient.GetDatabase(database);
            this.container = this.database.GetContainer(container);
        }

        public async Task InitializeAsync()
        {
            await AddRecipesToContainerAsync();
        }


        private async Task AddRecipesToContainerAsync()
        {
            var recipes = new List<Recipe>
    {
        new Recipe
        {
            Id = "1",
            PartitionKey = "Italian",
            Ingredients = new List<string>
            {
                "200g spaghetti",
                "100g pancetta or guanciale",
                "2 large eggs",
                "50g Pecorino Romano cheese",
                "Salt and black pepper"
            },
            Name = "Spaghetti Carbonara"
        },
        new Recipe
        {
            Id = "2",
            PartitionKey = "Italian",
            Ingredients = new List<string>
            {
                "8 oz fettuccine",
                "2 boneless, skinless chicken breasts",
                "2 cups heavy cream",
                "1 cup grated Parmesan cheese",
                "2 cloves garlic, minced",
                "2 tablespoons butter",
                "Salt and pepper to taste"
            },
            Name = "Chicken Alfredo"
        },
        new Recipe
        {
            Id = "3",
            PartitionKey = "Asian",
            Ingredients = new List<string>
            {
                "2 cups mixed vegetables (e.g., bell peppers, broccoli, carrots)",
                "1 lb tofu or chicken, cubed",
                "2 cloves garlic, minced",
                "1/4 cup soy sauce",
                "2 tablespoons vegetable oil",
                "1 tablespoon honey or sugar"
            },
            Name = "Vegetable Stir-Fry"
        },
    };

            foreach (var recipe in recipes)
            {
                try
                {
                    ItemResponse<Recipe> recipeResponse = await this.container.ReadItemAsync<Recipe>(recipe.Id, new PartitionKey(recipe.PartitionKey));
                    Console.WriteLine("Item in database with id: {0} already exists\n", recipeResponse.Resource.Id);
                }
                catch (CosmosException ex) when (ex.StatusCode == HttpStatusCode.NotFound)
                {
                    ItemResponse<Recipe> recipeResponse = await this.container.CreateItemAsync<Recipe>(recipe, new PartitionKey(recipe.PartitionKey));
                    Console.WriteLine("Created item in database with id: {0}. Operation consumed {1} RUs.\n", recipeResponse.Resource.Id, recipeResponse.RequestCharge);
                }
            }
        }

    }
}
