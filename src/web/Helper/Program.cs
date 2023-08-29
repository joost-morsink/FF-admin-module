// See https://aka.ms/new-console-template for more information

using System.Text.Json;
using Microsoft.Data.SqlClient;

using var sqlconn =
    new SqlConnection(
        $"Server=tcp:g4g.database.windows.net,1433;Initial Catalog=EventStore;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;Authentication=\"Active Directory Default\"");
    
await sqlconn.OpenAsync();

var events = from f in Directory.EnumerateFiles("/Users/joost/Projects/ff-admin-module/events", "*.json",
        SearchOption.AllDirectories)
    let alljson = File.ReadAllText(f)
    from json in alljson.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries)
    let dict = JsonSerializer.Deserialize<Dictionary<string,object>>(json)!
    let res = new {json, timestamp = dict["timestamp"]?.ToString() ??""}
    //orderby res.timestamp
    select res.json;

Console.WriteLine(string.Join(Environment.NewLine,events));

