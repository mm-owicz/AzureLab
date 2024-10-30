using System;
using System.Threading.Tasks;
using Microsoft.Azure.Cosmos;

namespace Lab1CosmosApp
{
    class Program
    {
        private static readonly string EndpointUri = "https://magdacosmosdbaccount.documents.azure.com:443/";
        private static readonly string PrimaryKey;
        private static CosmosClient cosmosClient;
        private static Database database;
        private static Container container;

        private const string DatabaseId = "ShopDB";
        private const string ContainerId = "Products";
        private const string partitionKey = "/productCategory";

        static async Task Main(string[] args)
        {
            await InitTask();

            // operacje crud
            await CreateProduct("9", "clothes", "shoes", 220.00);
            await ReadProduct("9", "clothes");
            await UpdateProduct("9", "clothes", "blue shoes", 200.00);
            await DeleteProduct("9", "clothes");

        }

        private static async Task InitTask()
        {
            cosmosClient = new CosmosClient(EndpointUri, PrimaryKey);
            database = await cosmosClient.CreateDatabaseIfNotExistsAsync(DatabaseId);
            container = await database.CreateContainerIfNotExistsAsync(ContainerId, partitionKey);
            Console.WriteLine("Connected to Cosmos DB.");
        }

        private static async Task CreateProduct(string id, string category, string name, double price)
        {
            var product = new Product
            {
                id = id,
                productCategory = category,
                name = name,
                price = price
            };

            try
            {
                await container.CreateItemAsync(product, new PartitionKey(category));
                Console.WriteLine($"Created '{name}' product.");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task ReadProduct(string id, string category)
        {
            try
            {
                ItemResponse<Product> response = await container.ReadItemAsync<Product>(id, new PartitionKey(category));
                Console.WriteLine($"Reading product. Name: {response.Resource.name}. Price: {response.Resource.price} zł.");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task UpdateProduct(string id, string category, string newName, double newPrice)
        {
            try
            {
                ItemResponse<Product> response = await container.ReadItemAsync<Product>(id, new PartitionKey(category));
                Product product = response.Resource;
                string oldName = product.name;

                product.name = newName;
                product.price = newPrice;

                await container.ReplaceItemAsync(product, id, new PartitionKey(category));
                Console.WriteLine($"Update product '{oldName}'. New product data: name '{newName}', price {newPrice} zł.");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }

        private static async Task DeleteProduct(string id, string category)
        {
            try
            {
                await container.DeleteItemAsync<Product>(id, new PartitionKey(category));
                Console.WriteLine("Deleted product item.");
            }
            catch (CosmosException ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
            }
        }
    }

    public class Product
    {
        public string id { get; set; }
        public string productCategory { get; set; }
        public string name { get; set; }
        public double price { get; set; }
    }
}
