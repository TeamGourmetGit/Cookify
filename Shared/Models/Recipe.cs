using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.Models
{
    public class Recipe
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "partitionKey")]
        public string PartitionKey { get; set; }

        [JsonProperty(PropertyName = "ingredients")]
        public List<string> Ingredients { get; set; }

        [JsonProperty(PropertyName = "name")]
        public string Name { get; set; }

    }
}
