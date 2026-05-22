using System.Collections.ObjectModel;
using System.Data;
using System.IO;
using FastReportLite.Models;
using Microsoft.Data.Sqlite;
using Newtonsoft.Json;

namespace FastReportLite.Services
{
    public class DataService
    {
        public bool TestConnection(DataSourceDefinition dataSource)
        {
            try
            {
                switch (dataSource.DataSourceType)
                {
                    case DataSourceType.SQLite:
                        return TestSqliteConnection(dataSource);
                    case DataSourceType.CSV:
                    case DataSourceType.JSON:
                        return !string.IsNullOrEmpty(dataSource.FilePath) && File.Exists(dataSource.FilePath);
                    default:
                        return false;
                }
            }
            catch
            {
                return false;
            }
        }

        private bool TestSqliteConnection(DataSourceDefinition dataSource)
        {
            if (string.IsNullOrEmpty(dataSource.ConnectionString))
                return false;

            using var connection = new SqliteConnection(dataSource.ConnectionString);
            connection.Open();
            return connection.State == ConnectionState.Open;
        }

        public ObservableCollection<FieldDefinition> GetFields(DataSourceDefinition dataSource)
        {
            var fields = new ObservableCollection<FieldDefinition>();

            switch (dataSource.DataSourceType)
            {
                case DataSourceType.SQLite:
                    return GetSqliteFields(dataSource);
                case DataSourceType.JSON:
                    return GetJsonFields(dataSource);
                default:
                    return fields;
            }
        }

        private ObservableCollection<FieldDefinition> GetSqliteFields(DataSourceDefinition dataSource)
        {
            var fields = new ObservableCollection<FieldDefinition>();

            try
            {
                using var connection = new SqliteConnection(dataSource.ConnectionString);
                connection.Open();

                var tableName = dataSource.TableName;
                if (string.IsNullOrEmpty(tableName) && !string.IsNullOrEmpty(dataSource.Query))
                {
                    var match = System.Text.RegularExpressions.Regex.Match(dataSource.Query, 
                        @"FROM\s+(\w+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                    if (match.Success)
                        tableName = match.Groups[1].Value;
                }

                if (!string.IsNullOrEmpty(tableName))
                {
                    var command = new SqliteCommand($"PRAGMA table_info({tableName})", connection);
                    using var reader = command.ExecuteReader();

                    while (reader.Read())
                    {
                        fields.Add(new FieldDefinition
                        {
                            Name = reader["name"].ToString() ?? "",
                            DataType = reader["type"].ToString() ?? "string"
                        });
                    }
                }
            }
            catch
            {
            }

            return fields;
        }

        private ObservableCollection<FieldDefinition> GetJsonFields(DataSourceDefinition dataSource)
        {
            var fields = new ObservableCollection<FieldDefinition>();

            try
            {
                if (File.Exists(dataSource.FilePath))
                {
                    var json = File.ReadAllText(dataSource.FilePath);
                    var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);

                    if (data != null && data.Count > 0)
                    {
                        foreach (var key in data[0].Keys)
                        {
                            fields.Add(new FieldDefinition
                            {
                                Name = key,
                                DataType = data[0][key]?.GetType().Name ?? "string"
                            });
                        }
                    }
                }
            }
            catch
            {
            }

            return fields;
        }

        public List<Dictionary<string, object>> ExecuteQuery(DataSourceDefinition dataSource)
        {
            switch (dataSource.DataSourceType)
            {
                case DataSourceType.SQLite:
                    return ExecuteSqliteQuery(dataSource);
                case DataSourceType.JSON:
                    return LoadJsonData(dataSource);
                case DataSourceType.CSV:
                    return LoadCsvData(dataSource);
                default:
                    return new List<Dictionary<string, object>>();
            }
        }

        private List<Dictionary<string, object>> ExecuteSqliteQuery(DataSourceDefinition dataSource)
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                using var connection = new SqliteConnection(dataSource.ConnectionString);
                connection.Open();

                var command = new SqliteCommand(dataSource.Query, connection);
                using var reader = command.ExecuteReader();

                while (reader.Read())
                {
                    var row = new Dictionary<string, object>();
                    for (int i = 0; i < reader.FieldCount; i++)
                    {
                        row[reader.GetName(i)] = reader.IsDBNull(i) ? DBNull.Value : reader.GetValue(i);
                    }
                    results.Add(row);
                }
            }
            catch
            {
            }

            return results;
        }

        private List<Dictionary<string, object>> LoadJsonData(DataSourceDefinition dataSource)
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                if (File.Exists(dataSource.FilePath))
                {
                    var json = File.ReadAllText(dataSource.FilePath);
                    var data = JsonConvert.DeserializeObject<List<Dictionary<string, object>>>(json);
                    if (data != null)
                        results = data;
                }
            }
            catch
            {
            }

            return results;
        }

        private List<Dictionary<string, object>> LoadCsvData(DataSourceDefinition dataSource)
        {
            var results = new List<Dictionary<string, object>>();

            try
            {
                if (File.Exists(dataSource.FilePath))
                {
                    var lines = File.ReadAllLines(dataSource.FilePath);
                    if (lines.Length > 0)
                    {
                        var headers = lines[0].Split(',');
                        
                        for (int i = 1; i < lines.Length; i++)
                        {
                            var values = lines[i].Split(',');
                            var row = new Dictionary<string, object>();
                            
                            for (int j = 0; j < headers.Length && j < values.Length; j++)
                            {
                                row[headers[j].Trim()] = values[j].Trim();
                            }
                            
                            results.Add(row);
                        }
                    }
                }
            }
            catch
            {
            }

            return results;
        }

        public static List<Dictionary<string, object>> LoadSampleData()
        {
            var data = new List<Dictionary<string, object>>();
            var random = new Random();

            for (int i = 1; i <= 20; i++)
            {
                data.Add(new Dictionary<string, object>
                {
                    ["ID"] = i,
                    ["OrderNumber"] = $"ORD-{2024}{i:D4}",
                    ["CustomerName"] = $"客户 {i}",
                    ["ProductName"] = $"产品 {random.Next(1, 10)}",
                    ["Quantity"] = random.Next(1, 100),
                    ["UnitPrice"] = Math.Round(random.NextDouble() * 1000, 2),
                    ["TotalAmount"] = Math.Round(random.NextDouble() * 10000, 2),
                    ["OrderDate"] = DateTime.Now.AddDays(-random.Next(1, 365)),
                    ["Status"] = random.Next(1, 4) switch
                    {
                        1 => "待处理",
                        2 => "已完成",
                        _ => "已取消"
                    }
                });
            }

            return data;
        }
    }
}
