using System;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace JsonPatch
{
    public class NoPatch : Attribute
    {
    }

    public class Building
    {
        [NoPatch]
        public Guid Id { get; set; }
        public string Material { get; set; }
        public int Floors { get; set; }
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            var building = new Building
            {
                Id = Guid.NewGuid(),
                Material = "Wood",
                Floors = 3
            };

            OutputJson(building, nameof(building), "Original");

            var patch = new JsonPatchDocument();
            patch.Replace("/material", "Steel");
            patch.Replace("/id", Guid.NewGuid());

            patch.RemoveNoPatchProperties<Building>();

            patch.ApplyTo(building);

            OutputJson(building, nameof(building), "Patched");

            Console.ReadKey();
        }

        public static JsonPatchDocument RemoveNoPatchProperties<T>(this JsonPatchDocument patch)
        {
            var noPatchProperties = typeof(T).GetProperties()
                .Where(x => Attribute.IsDefined(x, typeof(NoPatch)))
                .Select(x => x.Name.ToLower()).ToList();

            patch.Operations.RemoveAll(p => noPatchProperties.Contains(p.path.TrimStart(' ', '/').ToLower()));

            return patch;
        }

        public static void OutputJson(object data, string objectName, string version)
        {
            Console.WriteLine($"{objectName} ({version}):");
            Console.WriteLine(JToken.Parse(JsonConvert.SerializeObject(data)).ToString(Formatting.Indented));
            Console.WriteLine("------------------------------------------------------");
        }

    }

}
