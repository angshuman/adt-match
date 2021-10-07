using System;
using System.Threading.Tasks;

namespace adt_match
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var dataProvider = new DataProvider(new Uri("<ADT-INSTANCE_URL>"));

            // Ingest data
            await dataProvider.DeleteEntities();
            await dataProvider.IngestData();

            // COUNT
            await dataProvider.QueryAsync("SELECT COUNT() FROM DIGITALTWINS");

            // Fixed hop queries
            await dataProvider.QueryAsync("SELECT a.$dtId AS a_id, b.$dtId AS b_id FROM DIGITALTWINS MATCH (a)-[]->(b) WHERE a.$dtId = '0'");

            // Variable hop queries
            await dataProvider.QueryAsync("SELECT b.$dtId, b.level FROM DIGITALTWINS MATCH (a)-[*3]->(b) WHERE a.$dtId = '0'");
            await dataProvider.QueryAsync("SELECT b.$dtId, b.level FROM DIGITALTWINS MATCH (a)-[*1..3]->(b) WHERE a.$dtId = '0'");
            await dataProvider.QueryAsync("SELECT b.$dtId, b.level FROM DIGITALTWINS MATCH (a)-[*..2]->(b) WHERE a.$dtId = '0'");
            await dataProvider.QueryAsync("SELECT b.$dtId, b.level FROM DIGITALTWINS MATCH (a)-[*..2]->(b) WHERE a.$dtId = '0'");

            // Chained MATCH queries
            await dataProvider.QueryAsync("SELECT c.$dtId, c.level FROM DIGITALTWINS MATCH (a)-[*..2]->(b)-[*..2]->(c) WHERE a.$dtId = '0'");
            await dataProvider.QueryAsync("SELECT c.$dtId, c.level FROM DIGITALTWINS MATCH (a)-[]->()-[]->()-[]->()-[]->(c) WHERE a.$dtId = '0'");

            // Parent queries
            await dataProvider.QueryAsync("SELECT c.$dtId, c.level FROM DIGITALTWINS MATCH (a)<-[*..4]-(c) WHERE a.$dtId = '50'");
            await dataProvider.QueryAsync("SELECT c.$dtId, c.level FROM DIGITALTWINS MATCH (a)<-[:contains*..4]-(c) WHERE a.$dtId = '50'");

            // Undirected queries
            await dataProvider.QueryAsync("SELECT c.$dtId, c.level FROM DIGITALTWINS MATCH (a)-[:contains*..2]-(c) WHERE a.$dtId = '50'");

            // Queries on relationship properties
            await dataProvider.QueryAsync("SELECT c.$dtId, c.level, r.Length FROM DIGITALTWINS MATCH (a)-[r:contains]-(c) WHERE a.$dtId = '50' AND r.length > 0");
            await dataProvider.QueryAsync("SELECT c.$dtId, c.level, r1.Length AS r1_length, r2.Length AS r2_length FROM DIGITALTWINS MATCH (a)-[r1]-()-[r2]-(c) WHERE a.$dtId = '50' AND r1.length > 0 AND r2.length > 0");
        }
    }
}
