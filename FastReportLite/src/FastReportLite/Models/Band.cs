using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FastReportLite.Models
{
    public enum BandType
    {
        ReportTitle,
        PageHeader,
        GroupHeader,
        Data,
        GroupFooter,
        PageFooter,
        ReportSummary
    }

    public partial class Band : ObservableObject
    {
        [ObservableProperty]
        private string _id = Guid.NewGuid().ToString();

        [ObservableProperty]
        private BandType _bandType;

        [ObservableProperty]
        private string _name = "";

        [ObservableProperty]
        private double _height = 100;

        [ObservableProperty]
        private bool _visible = true;

        [ObservableProperty]
        private string _dataSourceName = "";

        [ObservableProperty]
        private string _filterExpression = "";

        [ObservableProperty]
        private string _sortExpression = "";

        [ObservableProperty]
        private ObservableCollection<ReportComponent> _components = new();

        public string DisplayName => BandType switch
        {
            BandType.ReportTitle => "报表标题",
            BandType.PageHeader => "页眉",
            BandType.GroupHeader => "分组头",
            BandType.Data => "数据区",
            BandType.GroupFooter => "分组尾",
            BandType.PageFooter => "页脚",
            BandType.ReportSummary => "报表汇总",
            _ => BandType.ToString()
        };

        partial void OnBandTypeChanged(BandType value)
        {
            Name = DisplayName;
        }

        public Band()
        {
            Name = DisplayName;
        }
    }
}
