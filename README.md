
# DbExchange

Simple console app to exchange data between different database


## Tech Stack

.NET 5

DB:
- MSSQL
- POSTGRESQL

  
## Documentation

### Db connection
Configure connection strings in appsettings.json
```json
"ConnectionStringList": [
    {
      "Name": "Connection1",
      "ConnectionString": "Data Source=.;Initial Catalog=ExampleDb1;Integrated Security=True;",
      "DbType": "MSSQL"
    },
    {
      "Name": "Connection2",
      "ConnectionString": "Data Source=.;Initial Catalog=ExampleDb2;Integrated Security=True;",
      "DbType": "MSSQL"
    }
  ],
```

Use DbType enum to switch between db

```csharp
public enum DbType
{
    MSSQL,
    POSTGRESQL
}
```

### Query settings
Write config.json file with your use case queries

Config.json file example:
```json
{
  "Steps": [
    {
      "FromDataImage": "2021-01-12T00:42:00.46",
      "DataImageUpdateColumn": "ChangedAt",
      "StepName": "Align item",
      "Fetch": {
        "ConnectionName": "Connection1",
        "Query": [
          "SELECT * FROM [dbo].[Items] WHERE ChangedAt >= @FromDataImage"
        ]
      },
      "Check": {
        "ConnectionName": "Connection2",
        "Query": [
          "SELECT count(*) FROM [dbo].[Items] WHERE Id = @Id"
        ]
      },
      "Update": [
        {
          "ConnectionName": "Connection2",
          "Query": [
            "UPDATE [dbo].[Items] SET Description = @Description WHERE Id = @Id"
          ]
        }
      ],
      "Create": [
        {
          "ConnectionName": "Connection2",
          "Query": [
            "INSERT INTO [dbo].[Items](Id, Description) VALUES(@Id, @Description)"
          ]
        }
      ]
    }
  ],
  "FetchDataDeltaSeconds": -180.0
}
```

Sections:
- Fetch: query to fetch data in read db
- Check: query to check if data exists in write db
- Update: query to update row (if exists)
- Create: query to insert row (if not exists)

If you need variables in script, you need to append "_\_" to variable name (example: @itemId__)
