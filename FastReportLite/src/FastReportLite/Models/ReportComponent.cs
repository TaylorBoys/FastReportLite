using System.Windows;
using System.Windows.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FastReportLite.Models
{
    public enum ComponentType
    {
        TextBox,
        Picture,
        Table,
        Chart,
        Line,
        Barcode
    }

    public partial class ReportComponent : ObservableObject
    {
        [ObservableProperty]
        private string _id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private ComponentType _componentType;

        [ObservableProperty]
        private double _left = 0;

        [ObservableProperty]
        private double _top = 0;

        [ObservableProperty]
        private double _width = 100;

        [ObservableProperty]
        private double _height = 30;

        [ObservableProperty]
        private bool _visible = true;

        [ObservableProperty]
        private string _dataBinding = "";

        [ObservableProperty]
        private string _text = "";

        [ObservableProperty]
        private string _fontFamily = "Segoe UI";

        [ObservableProperty]
        private double _fontSize = 12;

        [ObservableProperty]
        private bool _fontBold = false;

        [ObservableProperty]
        private bool _fontItalic = false;

        [ObservableProperty]
        private string _foreground = "#333333";

        [ObservableProperty]
        private string _background = "#FFFFFF";

        [ObservableProperty]
        private string _borderColor = "#000000";

        [ObservableProperty]
        private double _borderWidth = 1;

        [ObservableProperty]
        private string _borderStyle = "Solid";

        [ObservableProperty]
        private HorizontalAlignment _horizontalAlignment = HorizontalAlignment.Left;

        [ObservableProperty]
        private VerticalAlignment _verticalAlignment = VerticalAlignment.Center;

        [ObservableProperty]
        private string? _imagePath;

        [ObservableProperty]
        private string? _tooltip;

        [ObservableProperty]
        private bool _useScript = false;

        [ObservableProperty]
        private string _script = "";

        [ObservableProperty]
        private string _conditionalFormatting = "";

        // 事件脚本
        [ObservableProperty]
        private string _onBeforePrintScript = "";

        [ObservableProperty]
        private string _onAfterPrintScript = "";

        [ObservableProperty]
        private string _onGetValueScript = "";

        [ObservableProperty]
        private string _onFormattingScript = "";

        public string DisplayName => $"{ComponentType}_{Name}";

        public FontWeight FontWeight => FontBold ? FontWeights.Bold : FontWeights.Normal;
        public FontStyle FontStyle => FontItalic ? System.Windows.FontStyles.Italic : System.Windows.FontStyles.Normal;

        partial void OnFontBoldChanged(bool value)
        {
            OnPropertyChanged(nameof(FontWeight));
        }

        partial void OnFontItalicChanged(bool value)
        {
            OnPropertyChanged(nameof(FontStyle));
        }

        public ReportComponent()
        {
            Name = $"{ComponentType}_{Guid.NewGuid().ToString()[..8]}";
        }

        public ReportComponent(ComponentType type) : this()
        {
            ComponentType = type;
        }
    }

    public class TableComponent : ReportComponent
    {
        public TableComponent() : base(ComponentType.Table)
        {
            Columns = new();
            Rows = new();
        }

        public List<TableColumn> Columns { get; set; } = new();
        public List<TableRow> Rows { get; set; } = new();
        public int RowCount { get; set; } = 5;
    }

    public class TableColumn
    {
        public string Header { get; set; } = "";
        public string DataField { get; set; } = "";
        public double Width { get; set; } = 100;
    }

    public class TableRow
    {
        public List<string> Cells { get; set; } = new();
    }
}
