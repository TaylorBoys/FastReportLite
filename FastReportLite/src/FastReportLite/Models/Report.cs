using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace FastReportLite.Models
{
    public partial class Report : ObservableObject
    {
        [ObservableProperty]
        private string _name = "未命名报表";

        [ObservableProperty]
        private string _description = "";

        [ObservableProperty]
        private PageSettings _pageSettings = new();

        [ObservableProperty]
        private ObservableCollection<Band> _bands = new();

        [ObservableProperty]
        private ObservableCollection<DataSourceDefinition> _dataSources = new();

        public Report()
        {
            Bands.Add(new Band { BandType = BandType.ReportTitle, Height = 60 });
            Bands.Add(new Band { BandType = BandType.PageHeader, Height = 40 });
            Bands.Add(new Band { BandType = BandType.Data, Height = 100, DataSourceName = "" });
            Bands.Add(new Band { BandType = BandType.PageFooter, Height = 40 });
        }

        public void AddBand(BandType bandType)
        {
            var band = new Band { BandType = bandType, Height = GetDefaultHeight(bandType) };
            var insertIndex = GetInsertIndex(bandType);
            Bands.Insert(insertIndex, band);
        }

        private int GetInsertIndex(BandType bandType)
        {
            return bandType switch
            {
                BandType.ReportTitle => 0,
                BandType.PageHeader => 1,
                BandType.GroupHeader => FindGroupHeaderIndex(),
                BandType.Data => FindDataIndex(),
                BandType.GroupFooter => FindGroupFooterIndex(),
                BandType.PageFooter => Bands.Count - 1,
                BandType.ReportSummary => Bands.Count,
                _ => Bands.Count
            };
        }

        private int FindGroupHeaderIndex()
        {
            for (int i = 0; i < Bands.Count; i++)
            {
                if (Bands[i].BandType == BandType.Data)
                    return i;
            }
            return Bands.Count;
        }

        private int FindDataIndex()
        {
            for (int i = 0; i < Bands.Count; i++)
            {
                if (Bands[i].BandType == BandType.PageFooter)
                    return i;
            }
            return Bands.Count;
        }

        private int FindGroupFooterIndex()
        {
            for (int i = 0; i < Bands.Count; i++)
            {
                if (Bands[i].BandType == BandType.PageFooter)
                    return i;
            }
            return Bands.Count;
        }

        private double GetDefaultHeight(BandType bandType)
        {
            return bandType switch
            {
                BandType.ReportTitle => 60,
                BandType.PageHeader => 40,
                BandType.Data => 100,
                BandType.PageFooter => 40,
                BandType.ReportSummary => 60,
                _ => 50
            };
        }
    }

    public class PageSettings
    {
        public string PaperSize { get; set; } = "A4";
        public double Width { get; set; } = 210; // mm
        public double Height { get; set; } = 297; // mm
        public double LeftMargin { get; set; } = 20; // mm
        public double RightMargin { get; set; } = 20; // mm
        public double TopMargin { get; set; } = 20; // mm
        public double BottomMargin { get; set; } = 20; // mm
        public bool Landscape { get; set; } = false;
    }
}
