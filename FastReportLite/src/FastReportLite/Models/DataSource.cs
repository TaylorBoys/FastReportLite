using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FastReportLite.Models
{
    public enum DataSourceType
    {
        SqlServer,
        SQLite,
        CSV,
        JSON,
        Object
    }

    public partial class DataSourceDefinition : ObservableObject
    {
        [ObservableProperty]
        private string _id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private DataSourceType _dataSourceType;

        [ObservableProperty]
        private string _connectionString = "";

        [ObservableProperty]
        private string _query = "";

        [ObservableProperty]
        private string _tableName = "";

        [ObservableProperty]
        private string _filePath = "";

        [ObservableProperty]
        private bool _isConnected = false;

        [ObservableProperty]
        private ObservableCollection<FieldDefinition> _fields = new();

        public string DisplayName => $"{Name} ({DataSourceType})";

        public DataSourceDefinition()
        {
            Name = $"DataSource_{Guid.NewGuid().ToString()[..8]}";
        }
    }

    public class FieldDefinition
    {
        public string Name { get; set; } = "";
        public string DataType { get; set; } = "string";
        public string? Description { get; set; }
        public bool IsKey { get; set; } = false;
    }
}
