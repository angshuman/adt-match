# Using Variable Hop Queries in Azure Digital Twins
>How to use the _MATCH_ clause

Azure Digital Twins has recently announced support for Variable Hop Queries. This is done through introducing a new MATCH keyword in the [Query Language](https://docs.microsoft.com/en-us/azure/digital-twins/concepts-query-language). This project explores various features of this type of queries through sample code. 

## Why
In a Digital Twins representation of a complex system, it is often interesting to query nodes at a variable distance from each other. The data modelling could be such that the nodes of interest can be connected with different number of hops. In addition to this a query may have to look at both directions to find the results. `MATCH` enables these business scenarios simpler to query.

## Setup

Please follow [instructions](https://docs.microsoft.com/en-us/azure/digital-twins/how-to-set-up-instance-portal) to set up a new digital twins instance.
In the code replace `<ADT-INSTANCE_URL>` with the instance of your digital twins. Make sure that the account has access roles (Azure Digital Twins Data) for accessing the data plane API's. 

## Ingestion

This sample creates a tree-like dataset that is used for performing the queries. The levels of the tree and the number of children are configurable in the code. For example: the following would create a tree that is 10 levels deep and each node has two children.

```c#
var dataGenerator = new DataGenerator(10, 2, "contains");
```

> Note: Nodes and relationships have some properties for which the values are randomly generated. Every node also has a level property Please take a look at the model for the details.

> Note: Once the data is ingested, feel free to comment out that section in Program.cs to run only queries. 

## Building and running

Install dotnet core 3.1. Run the program using `dotnet run`

## Queries

Variable hop queries are supported through the MATCH keyword. In the most basic form a twin with a single hop relationship can be queried in the following way. 

```sql
SELECT a.$dtId AS a_id, b.$dtId AS b_id FROM DIGITALTWINS MATCH (a)-[]->(b) WHERE a.$dtId = '0'
```

Note that here we are querying for a node with id '0' and all the nodes at a single hop distance. `()` represents a node and `[]` represents a single hop relationship. The number of hops can be increased in the query by adding a `*` clause in the relationship. For example - 

### Number of hops

|Query   |Returns nodes at hop distance    | Description |
|---|---|---|
|`(r)-[]->(c)`   |   1| `[]` implies one hop |
|`(r)-[*1..3]->(c)` | 2, 3 | `*` implies variable hop |
|`(r)-[*3]->(c)` | 3 | `*3` implies exactly 3 hops |
|`(r)-[*..5]->(c)` | 1,2,3,4,5 | `*..N` implies upto N hops |


```sql
SELECT b.$dtId, b.level FROM DIGITALTWINS MATCH (a)-[*3]->(b) WHERE a.$dtId = '0'"
```
This query would return all nodes `(b)`  that are exactly 3 hops away from node `(a)`.
<br>
```sql
SELECT b.$dtId, b.level FROM DIGITALTWINS MATCH (a)-[*1..3]->(b) WHERE a.$dtId = '0'
```
This query would return nodes `(b)` that are 2 or 3 hops away from node `(a)`
<br>
```sql
SELECT b.$dtId, b.level FROM DIGITALTWINS MATCH (a)-[*..2]->(b) WHERE a.$dtId = '0'"
```
In this case `(b)` would match all nodes that are upto 2 levels away from `(a)` 
<br>

### Query direction

Direction of a relationships can be changed in the same query. For example the following query - 
```sql
SELECT P FROM digitaltwins MATCH (A)<-[*..3]-(P)-[*..3]->(B) WHERE A.$dtId = '11' AND B.$dtId = '14'
```
changes direction in the same MATCH clause. Semantically this would find a common ancestor of ndoes `(A)` and `(B)` within 3 levels.

![subtrees](/assets/common.jpg)

> Note: MATCH supprots directed and undirected queries.

<br>


### Query on relationships
Relationship names can be provided  with the `:` notation. 
```sql
SELECT c.$dtId, c.level FROM DIGITALTWINS MATCH (a)<-[:contains*..4]-(c) WHERE a.$dtId = '50'
```
This query would only follow the relationship path where relationship name is `contains`. Note that this query specifies a raltionship in the right to left `()<-[]-()` direction. Queries can also be made without specifying a direction using `()-[]-()`.
<br>

Queries can also be performed on relationship properties. Aliasing a relationship only works with a single hop distance.
> Note: this query uses an anynymous node.
```sql
SELECT c.$dtId, c.level, r1.Length AS r1_length, r2.Length AS r2_length FROM DIGITALTWINS MATCH (a)-[r1]-()-[r2]-(c) WHERE a.$dtId = '50' AND r1.length > 0 AND r2.length > 0
```
This query filters on relationships with restrictions on the length properties.


### Cycle detection
In a graph with circular paths MATCH would implicitly traverse each edge only once.

### Complex MATCH operations with chains
Consider the following query

```sql
SELECT C FROM digitaltwins MATCH (A)-[:has|contains*1..3]->(B)-[:has|contains*2..4]->(C) WHERE A.$dtId = '0'
```

In this query `(B)` is a set of intermediate nodes that are 2 or 3 hops away from A. The second part of the match clause expands the intermediate nodes to 3 or 4 levels. As a result we get a group of sub-trees that look like below.

![subtrees](/assets/subtrees.jpg)

> Note: This visualization was generated using [ADT explorer](https://explorer.digitaltwins.azure.net/)























