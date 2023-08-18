using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

public class SqlToJsonConverter
{
    public string ConvertToJSON(string sqlInput)
    {
        var regex = new Regex(@"substring\(@TXT, (\d+), (\d+)\) as \[([a-zA-Z0-9_]+)\]");
        var matches = regex.Matches(sqlInput);

        var columnRepresentations = new List<object>();

        foreach (Match match in matches)
        {
            int start = int.Parse(match.Groups[1].Value);
            int length = int.Parse(match.Groups[2].Value);
            int end = start + length - 1;
            string columnName = match.Groups[3].Value;

            var columnRepresentation = new
            {
                FileColumnName = columnName,
                TableColumnName = columnName,
                Type = "string",
                Start = start.ToString(),
                End = end.ToString(),
                Mandatory = "False",
                Default = "null",
                Format = "",
                Migrate = new
                {
                    DoMigrate = "True",
                    DestinationColumnName = columnName
                },
                Virtuality = new
                {
                    isVirtual = "False",
                    ColumnValue = ""
                }
            };

            columnRepresentations.Add(columnRepresentation);
        }

        var options = new JsonSerializerOptions
        {
            WriteIndented = true
        };

        return JsonSerializer.Serialize(columnRepresentations, options);
    }
}

public class Program
{
    public static void Main()
    {
        var converter = new SqlToJsonConverter();
        string sqlInput = @"SELECT 
            substring(@TXT, 3, 1) as [StatusCode], 
            substring(@TXT, 4, 2) as [VersionControlNumber], 
            substring(@TXT, 6, 4) as [TransactionOriginatingSource], 
            substring(@TXT, 10, 12) as [Account], 
            substring(@TXT, 22, 1) as [SettlementCode],
            substring(@TXT, 23, 4) as [ParticipantNumber]";

        string jsonOutput = converter.ConvertToJSON(sqlInput);
        Console.WriteLine(jsonOutput);
    }
}
