using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Azure.Cosmos;


namespace episode3
{
    class Program
    {

        static string uri = "";
        static string key = "";
        static string databaseId = "database1";
        static string containerId = "families";

        static Container container;
        static CosmosClient client;


        static async Task Main()
        {

            client = new CosmosClient(uri, key);
            Database db = await client.CreateDatabaseIfNotExistsAsync(databaseId);
            container = await db.CreateContainerIfNotExistsAsync(containerId, "/lastName", 400);


            Family family = await Create();
            List<Family> familyList = await Query(family.LastName, family.Id);
            family = await Get(family.LastName, family.Id);
            family.Age = 40;
            await Update(family);
            await FamilyTransaction();
            await Delete(family.LastName, family.Id);

        }

        public static async Task<Family> Create()
        {
            Family family = new Family()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Mark",
                LastName = "Brown",
                Age = 30
            };

            Response<Family> response = await container.CreateItemAsync<Family>(
                family,
                new PartitionKey(family.LastName));

            Print(response.Resource);

            Console.WriteLine(response.RequestCharge);

            return family;
        }

        public static async Task<List<Family>> Query(string lastName, string id)
        {
            List<Family> families = new();

            string sql = "select * from c where c.id = @id";

            FeedIterator<Family> resultSet = container.GetItemQueryIterator<Family>(
                new QueryDefinition(sql)
                .WithParameter("@id", id),
                requestOptions: new QueryRequestOptions()
                {
                    PartitionKey = new PartitionKey(lastName)
                });

            while (resultSet.HasMoreResults)
            {
                FeedResponse<Family> response = await resultSet.ReadNextAsync();

                foreach(Family family in response)
                {
                    families.Add(family);
                }


                Console.WriteLine(response.RequestCharge);
            }

            return families;

        }

        public static async Task<Family> Get(string lastName, string id)
        {
            Response<Family> response = await container.ReadItemAsync<Family>(
                id: id,
                partitionKey: new PartitionKey(lastName));

            Console.WriteLine(response.RequestCharge);

            return response.Resource;            
        }

        public static async Task Update(Family family)
        {

            await container.ReplaceItemAsync<Family>(
                family,
                id: family.Id,
                partitionKey: new PartitionKey(family.LastName)
                );
        }

        public static async Task Delete(string lastName, string id)
        {
            await container.DeleteItemAsync<Family>(id, new PartitionKey(lastName));
        }

        public static async Task FamilyTransaction()
        {
            Family family1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "David",
                LastName = "Brown",
                Age = 25
            };

            Family family2 = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Mary",
                LastName = "Brown",
                Age = 27
            };

            TransactionalBatchResponse batch = await container.CreateTransactionalBatch(
                new PartitionKey("Brown"))
                .CreateItem<Family>(family1)
                .CreateItem<Family>(family2)
                .ExecuteAsync();

            Console.WriteLine(batch.RequestCharge);

        }

        public static async Task<string> InsertItemAsync(Family family)
        {
            Response<Family> response = await container.CreateItemAsync<Family>(family, new PartitionKey(family.LastName));

            return response.Headers.Session;
        
        }

        public static async Task MultiInsert()
        {
            List<Family> families = new List<Family>();

            Family family1 = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "David",
                LastName = "Brown",
                Age = 25
            };

            Family family2 = new()
            {
                Id = Guid.NewGuid().ToString(),
                FirstName = "Mary",
                LastName = "Brown",
                Age = 27
            };

            families.Add(family1);
            families.Add(family2);

            var tasks = new List<Task<string>>();


            foreach(Family family in families)
            {
                tasks.Add(InsertItemAsync(family));
            }

            //List<Task<string>> responses = await Task.WhenAll(tasks).Result;

        }

        public static void Print(object obj)
        {
            Console.WriteLine($"{JObject.FromObject(obj)}\n");
        }
    }


    class Family
    {
        [JsonProperty(PropertyName = "id")]
        public string Id { get; set; }

        [JsonProperty(PropertyName = "firstName")]
        public string FirstName { get; set; }

        [JsonProperty(PropertyName = "lastName")]
        public string LastName { get; set; }

        [JsonProperty(PropertyName = "age")]
        public int Age { get; set; }
    }
}
