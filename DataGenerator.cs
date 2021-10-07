using System;
using System.Collections.Generic;
using System.Linq;

namespace adt_match
{
    public class DataGenerator
    {
        private readonly int level;
        private readonly int factor;
        private readonly string realationshipName;
        private readonly Random rand;
        private int nodeCount = 1;
        private int edgeCount = 0;

        public DataGenerator(int level, int factor, string relationshipName)
        {
            this.level = level;
            this.factor = factor;
            this.realationshipName = relationshipName;
            rand = new Random();
        }

        public IEnumerable<object> Generate()
        {
            var root = new List<Node> { GetNode("0", "twin", 0) };
            yield return root.First();
            var rest = GenerateLevel(root, 1);
            foreach (var item in rest)
            {
                yield return item;
            }
        }

        private IEnumerable<object> GenerateLevel(List<Node> previousNodes, int currentLevel)
        {
            if (currentLevel == level)
            {
                yield break;
            }

            var levelNodes = new List<Node>();

            foreach (var previousNode in previousNodes)
            {
                for (var count = 0; count < factor; count++)
                {
                    var newNode = GetNode(id: nodeCount++.ToString(), label: "twin", level: currentLevel);
                    levelNodes.Add(newNode);
                    yield return newNode;
                    var newEdge = new Edge
                    {
                        Id = edgeCount++.ToString(),
                        Label = this.realationshipName,
                        FromId = previousNode.Id,
                        ToId = newNode.Id,
                    };
                    newEdge.Properties.Add("length", rand.Next(0, 10));
                    yield return newEdge;
                }
            }

            foreach (var item in GenerateLevel(levelNodes, currentLevel + 1))
            {
                yield return item;
            }
        }

        private Node GetNode(string id, string label, int level)
        {
            var newNode = new Node
            {
                Id = id,
                Label = label,
            };
            newNode.PartitionId = newNode.Id;
            newNode.Properties.Add("temperature", rand.Next(50, 100));
            newNode.Properties.Add("humidity", rand.Next(50, 100));
            newNode.Properties.Add("pressure", rand.Next(50, 100));
            newNode.Properties.Add("level", level);
            return newNode;
        }

        private IEnumerable<Edge> GetEdge(string id, string label, string from, string to)
        {
            yield return new Edge
            {
                Id = id,
                Label = label,
                EdgeType = "Outgoing",
                FromId = from,
                ToId = to,
                Properties = new Dictionary<string, object> { { "length", rand.Next(10) } },
            };

            yield return new Edge
            {
                Id = $"re:{id}",
                Label = label,
                EdgeType = "Reverse",
                FromId = to,
                ToId = from,
                Properties = new Dictionary<string, object> { { "length", rand.Next(10) } },
            };
        }
    }

    public class Node
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public string PartitionId { get; set; }

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }

    public class Edge
    {
        public string Id { get; set; }

        public string Label { get; set; }

        public string EdgeType { get; set; }

        public string FromId { get; set; }

        public string ToId { get; set; }

        public Dictionary<string, object> Properties { get; set; } = new Dictionary<string, object>();
    }
}
