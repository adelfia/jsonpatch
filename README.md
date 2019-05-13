# Safely use HTTP PATCH with the help of a custom attribute
## Introduction
A HTTP PATCH request lets you update selected properties of an object. But you often need to prevent some of these properties from being edited. Typically, properties like Id, CreatedDate and CreatedBy should not be updateable. 

In .NET Core, you would use the [JsonPatchDocument](https://docs.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.jsonpatch.jsonpatchdocument?view=aspnetcore-2.2) class to hold your patch information, then apply this patch to the object you are updating.

This demo shows how to use a custom attribute to mark properties as non-patchable.

## How-to
To see the example code in action, clone and run the contents of this repository. Or try for yourself:

1. Open the package manager console and install Microsoft.AspNetCore.JsonPatch.

```
PM> Install-Package Microsoft.AspNetCore.JsonPatch
```

2. Create an empty attribute class, which will be used later to mark individual properties of any class.

```cs
public class NoPatch : Attribute
{
}
```

3. Decorate any properties you want to be non-patchable using your newly created NoPatch attribute.

```cs
public class Building
{
    [NoPatch]
    public Guid Id { get; set; }
    public string Material { get; set; }
    public int Floors { get; set; }
}
```

4. Create an extension method for JsonPatchDocument, which will clean out any forbidden properties:

```cs
public static JsonPatchDocument RemoveNoPatchProperties<T>(this JsonPatchDocument patch)
{
    var noPatchProperties = typeof(T).GetProperties()
        .Where(x => Attribute.IsDefined(x, typeof(NoPatch)))
        .Select(x => x.Name.ToLower()).ToList();

    patch.Operations.RemoveAll(p => noPatchProperties.Contains(p.path.TrimStart(' ', '/').ToLower()));

    return patch;
}
```

4. Get your original object, create the patch, remove the NoPatch properties and then apply the patch:

```cs
// The original object
var building = new Building
{
    Id = Guid.NewGuid(),
    Material = "Wood",
    Floors = 3
};

// Create a patch to replace the values of Material and Id (note the Id carries the [NoPatch] attribute)
var patch = new JsonPatchDocument();
patch.Replace("/material", "Steel");
patch.Replace("/id", Guid.NewGuid());

// Remove [NoPatch] properties
patch.RemoveNoPatchProperties<Building>();

// Apply the patch
patch.ApplyTo(building);
```

## Full demo-code
For your reference, here's the full source of this quick-and-dirty demo:

```cs
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
```
