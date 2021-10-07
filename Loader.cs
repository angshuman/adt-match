using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Azure.DigitalTwins.Core;
using Azure.Identity;

namespace adt_match
{
    public class DataProvider
    {
        private readonly int Levels = 10;
        private readonly int Factor = 2;
        private readonly DigitalTwinsClient client;
        private readonly string filePath = "./data/model.json";
        private readonly JsonSerializerOptions options = new JsonSerializerOptions { WriteIndented = true };

        public DataProvider(Uri host)
        {
            client = new DigitalTwinsClient(host, new AzureCliCredential());
        }

        public async Task IngestData()
        {
            var model = ReadModel();
            await UploadModels(new object[] { model });
            await CreateEntities();
        }

        public async Task UploadModels(object[] models)
        {
            try
            {
                await client.CreateModelsAsync(models.Select(x => JsonSerializer.Serialize(x)));
                Console.WriteLine($"Created Model");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in Model Upload: {ex.GetType()} \n {ex.Message}");
            }
        }

        public async Task QueryAsync(string query)
        {
            Console.WriteLine($"QUERY: {query}");
            var watch = Stopwatch.StartNew();
            var rsp = client.QueryAsync<Dictionary<string, object>>(query);
            await foreach (var item in rsp)
            {
                Console.WriteLine(JsonSerializer.Serialize(item, options));
                Console.WriteLine();
            }
            watch.Stop();
            Console.WriteLine($"COMPLETED IN: {watch.ElapsedMilliseconds} ms");
            Console.WriteLine();
        }

        private async Task CreateEntities()
        {
            var dataGenerator = new DataGenerator(Levels, Factor, "contains");
            foreach (var item in dataGenerator.Generate())
            {
                try
                {
                    if (item is Node node)
                    {
                        var twin = new BasicDigitalTwin();
                        twin.Id = node.Id;
                        twin.Metadata.ModelId = "dtmi:syncservicetests:anothertype;2";
                        twin.Contents.Add("level", node.Properties["level"]);
                        twin.Contents.Add("temperature", node.Properties["level"]);

                        await client.CreateOrReplaceDigitalTwinAsync(twin.Id, twin);
                        Console.WriteLine($"Created twin with id: {twin.Id}");
                    }
                    else if (item is Edge edge)
                    {
                        var relationship = new BasicRelationship
                        {
                            Id = edge.Id,
                            SourceId = edge.FromId,
                            TargetId = edge.ToId,
                            Name = edge.Label
                        };

                        relationship.Properties.Add("length", edge.Properties["length"]);
                        await client.CreateOrReplaceRelationshipAsync(relationship.SourceId, relationship.Id, relationship);
                        Console.WriteLine($"Created relationship with id: {relationship.Id} From: {relationship.SourceId} To: {relationship.TargetId}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error: {ex.Message}");
                }
            }
        }

        public async Task DeleteEntities()
        {
            var rsp = client.QueryAsync<Dictionary<string, string>>("SELECT T.$dtId FROM DIGITALTWINS T");
            await foreach (var item in rsp)
            {
                var twinId = item.Values.First();
                var relationships = client.GetRelationshipsAsync<Dictionary<string, object>>(twinId);


                await foreach (Dictionary<string, object> rel in relationships)
                {

                    var relationshipId = rel["$relationshipId"].ToString();
                    Console.WriteLine($"Deleting relationship with id: {relationshipId} of twin: {twinId}");

                    await client.DeleteRelationshipAsync(twinId, relationshipId);
                }

                var incomingRelationships = client.GetIncomingRelationshipsAsync(twinId);

                await foreach (var incoming in incomingRelationships)
                {

                    Console.WriteLine($"Deleting relationship with id: {incoming.RelationshipId} of twin: {incoming.SourceId}");

                    await client.DeleteRelationshipAsync(incoming.SourceId, incoming.RelationshipId);
                }

                Console.WriteLine($"Deleting twin with id: {twinId}");
                await client.DeleteDigitalTwinAsync(twinId);
            }
        }

        private object ReadModel()
        {
            var json = File.ReadAllText(filePath);
            return JsonSerializer.Deserialize<object>(json);
        }
    }
}