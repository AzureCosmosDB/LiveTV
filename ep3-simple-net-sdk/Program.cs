using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;


namespace episode3
{
    class Program
    {



        static async Task Main()
        {




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
            Family family;


            return family;
        }

        public static async Task<List<Family>> Query(string lastName, string id)
        {
            List<Family> family = new();


            return family;

        }

        public static async Task<Family> Get(string lastName, string id)
        {
            
        }

        public static async Task Update(Family family)
        {

        }

        public static async Task Delete(string lastName, string id)
        {

        }

        public static async Task FamilyTransaction()
        {

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
