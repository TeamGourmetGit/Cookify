using Microsoft.Azure.Cosmos;
using Shared.Models;

namespace Business.Repositories.Default
{
    public class RecipeRepository : IRepository<Recipe>
    {
        private readonly Container _container;

        public RecipeRepository(string cosmosConnectionString, string databaseName, string containerName)
        {
            var client = new CosmosClient(cosmosConnectionString);
            var database = client.GetDatabase(databaseName);
            _container = database.GetContainer(containerName);
        }

        public async Task<Recipe> AddAsync(Recipe entity)
        {
            var response = await _container.CreateItemAsync(entity);
            return response.Resource;
        }

        public async Task<Recipe> DeleteAsync(object id)
        {
            var response = await _container.DeleteItemAsync<Recipe>(id.ToString(), new PartitionKey(id.ToString()));
            return response.Resource;
        }

        public async Task<IEnumerable<Recipe>> GetAllAsync(string? sort, string? filter, int? page, int? pageSize)
        {
            var query = new QueryDefinition("SELECT * FROM c");

            if (!string.IsNullOrEmpty(sort))
            {
                // Implement sorting logic here
                // For example: query.OrderBy("c.PropertyName");
            }

            if (!string.IsNullOrEmpty(filter))
            {
                // Implement filtering logic here
                // For example: query.Where("c.PropertyName = @filterParam", new SqlParameter("@filterParam", filter));
            }

            var resultSetIterator = _container.GetItemQueryIterator<Recipe>(query);

            var results = new List<Recipe>();
            while (resultSetIterator.HasMoreResults)
            {
                var response = await resultSetIterator.ReadNextAsync();
                results.AddRange(response.ToList());
            }

            if (page.HasValue && pageSize.HasValue)
            {
                // Implement paging logic here
                // For example: results = results.Skip((page - 1) * pageSize).Take(pageSize).ToList();
            }

            return results;
        }

        public async Task<Recipe> GetByIdAsync(object id)
        {  
            var response = await _container.ReadItemAsync<Recipe>(id.ToString(), new PartitionKey(id.ToString()));
            return response.Resource;
        }

        public async Task<Recipe> UpdateAsync(Recipe entity)
        {
            var response = await _container.UpsertItemAsync(entity);
            return response.Resource;
        }
    }
}
