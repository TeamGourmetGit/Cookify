using Business.Repositories;
using Cookify.Helpers;
using Microsoft.Azure.Cosmos.Serialization.HybridRow;
using Shared.Models;

namespace Cookify.Endpoints
{
    public static class RecipeEndpoints
    {
        private static readonly IRepository<Recipe> _recipeRepository;

        static RecipeEndpoints()
        {
            _recipeRepository = ServiceLocator.GetService<IRepository<Recipe>>();
        }

        public static WebApplication MapRecipeEndpoints(this WebApplication app)
        {
            app.MapGet("/api/recipes", GetAllRecipes).WithTags("Recipes");
            app.MapDelete("/api/recipes/{id}", DeleteRecipe).WithTags("Recipes");
            app.MapGet("/api/recipes/{id}", GetRecipeByIdAsync).WithTags("Recipes");
            app.MapPost("api/recipes", PostRecipe).WithTags("Recipes");
            return app;
        }

        public static async Task<IResult> GetAllRecipes(string? sort, string? filter, int? page = 1, int? pageSize = 100)
        {
            var models = await _recipeRepository.GetAllAsync(sort, filter, page, pageSize);
            return Results.Ok(models);
        }

        public static async Task<IResult> DeleteRecipe(string id, string partitionKey)
        {
            var models = await _recipeRepository.DeleteAsync(id,partitionKey);
            return Results.Ok(models);
        }

        public static async Task<IResult> GetRecipeByIdAsync(string id, string partitionKey)
        {
            var models = await _recipeRepository.GetByIdAsync(id, partitionKey);
            return Results.Ok(models);
        }

        public static async Task<IResult> PostRecipe(Recipe recipe)
        {
            var models = await _recipeRepository.AddAsync(recipe);

            if(models != null)
            {
                return Results.Created($"api/recipes/{models.Id}", models);
            }
            else
            {
                return Results.BadRequest("Failed to create the recipe");
            }
        }
    }
}
