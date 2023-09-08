using Business.Repositories;
using Cookify.Helpers;
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
            return app;
        }

        public static async Task<IResult> GetAllRecipes(string? sort, string? filter, int? page = 1, int? pageSize = 100)
        {
            var models = await _recipeRepository.GetAllAsync(sort, filter, page, pageSize);
            return Results.Ok(models);
        }
    }
}
