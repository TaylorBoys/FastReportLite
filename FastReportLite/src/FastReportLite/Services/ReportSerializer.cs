using System.IO;
using System.Xml.Linq;
using FastReportLite.Models;
using Newtonsoft.Json;

namespace FastReportLite.Services
{
    public class ReportSerializer
    {
        public void SaveToFile(Report report, string filePath)
        {
            var reportData = SerializeReport(report);
            var json = JsonConvert.SerializeObject(reportData, Formatting.Indented);
            File.WriteAllText(filePath, json);
        }

        public Report? LoadFromFile(string filePath)
        {
            if (!File.Exists(filePath))
                return null;

            var json = File.ReadAllText(filePath);
            var reportData = JsonConvert.DeserializeObject<ReportData>(json);
            return DeserializeReport(reportData);
        }

        private ReportData SerializeReport(Report report)
        {
            var data = new ReportData
            {
                Name = report.Name,
                Description = report.Description,
                PageSettings = report.PageSettings,
                Bands = new List<BandData>(),
                DataSources = new List<DataSourceData>()
            };

            foreach (var band in report.Bands)
            {
                var bandData = new BandData
                {
                    Id = band.Id,
                    BandType = band.BandType,
                    Name = band.Name,
                    Height = band.Height,
                    Visible = band.Visible,
                    DataSourceName = band.DataSourceName,
                    FilterExpression = band.FilterExpression,
                    SortExpression = band.SortExpression,
                    Components = new List<ComponentData>()
                };

                foreach (var component in band.Components)
                {
                    bandData.Components.Add(new ComponentData
                    {
                        Id = component.Id,
                        Name = component.Name,
                        ComponentType = component.ComponentType,
                        Left = component.Left,
                        Top = component.Top,
                        Width = component.Width,
                        Height = component.Height,
                        Visible = component.Visible,
                        DataBinding = component.DataBinding,
                        Text = component.Text,
                        FontFamily = component.FontFamily,
                        FontSize = component.FontSize,
                        FontBold = component.FontBold,
                        FontItalic = component.FontItalic,
                        Foreground = component.Foreground,
                        Background = component.Background,
                        BorderColor = component.BorderColor,
                        BorderWidth = component.BorderWidth,
                        BorderStyle = component.BorderStyle,
                        HorizontalAlignment = component.HorizontalAlignment,
                        VerticalAlignment = component.VerticalAlignment,
                        ImagePath = component.ImagePath,
                        Tooltip = component.Tooltip,
                        UseScript = component.UseScript,
                        Script = component.Script,
                        ConditionalFormatting = component.ConditionalFormatting,
                        OnBeforePrintScript = component.OnBeforePrintScript,
                        OnAfterPrintScript = component.OnAfterPrintScript,
                        OnGetValueScript = component.OnGetValueScript,
                        OnFormattingScript = component.OnFormattingScript
                    });
                }

                data.Bands.Add(bandData);
            }

            foreach (var ds in report.DataSources)
            {
                data.DataSources.Add(new DataSourceData
                {
                    Id = ds.Id,
                    Name = ds.Name,
                    DataSourceType = ds.DataSourceType,
                    ConnectionString = ds.ConnectionString,
                    Query = ds.Query,
                    TableName = ds.TableName,
                    FilePath = ds.FilePath
                });
            }

            return data;
        }

        private Report? DeserializeReport(ReportData? data)
        {
            if (data == null)
                return null;

            var report = new Report
            {
                Name = data.Name,
                Description = data.Description ?? "",
                PageSettings = data.PageSettings ?? new PageSettings()
            };

            report.Bands.Clear();

            foreach (var bandData in data.Bands ?? new List<BandData>())
            {
                var band = new Band
                {
                    Id = bandData.Id,
                    BandType = bandData.BandType,
                    Name = bandData.Name,
                    Height = bandData.Height,
                    Visible = bandData.Visible,
                    DataSourceName = bandData.DataSourceName ?? "",
                    FilterExpression = bandData.FilterExpression ?? "",
                    SortExpression = bandData.SortExpression ?? ""
                };

                foreach (var compData in bandData.Components ?? new List<ComponentData>())
                {
                    var component = new ReportComponent
                    {
                        Id = compData.Id,
                        Name = compData.Name,
                        ComponentType = compData.ComponentType,
                        Left = compData.Left,
                        Top = compData.Top,
                        Width = compData.Width,
                        Height = compData.Height,
                        Visible = compData.Visible,
                        DataBinding = compData.DataBinding ?? "",
                        Text = compData.Text ?? "",
                        FontFamily = compData.FontFamily ?? "Segoe UI",
                        FontSize = compData.FontSize,
                        FontBold = compData.FontBold,
                        FontItalic = compData.FontItalic,
                        Foreground = compData.Foreground ?? "#333333",
                        Background = compData.Background ?? "#FFFFFF",
                        BorderColor = compData.BorderColor ?? "#000000",
                        BorderWidth = compData.BorderWidth,
                        BorderStyle = compData.BorderStyle ?? "Solid",
                        HorizontalAlignment = compData.HorizontalAlignment,
                        VerticalAlignment = compData.VerticalAlignment,
                        ImagePath = compData.ImagePath,
                        Tooltip = compData.Tooltip,
                        UseScript = compData.UseScript,
                        Script = compData.Script ?? "",
                        ConditionalFormatting = compData.ConditionalFormatting ?? "",
                        OnBeforePrintScript = compData.OnBeforePrintScript ?? "",
                        OnAfterPrintScript = compData.OnAfterPrintScript ?? "",
                        OnGetValueScript = compData.OnGetValueScript ?? "",
                        OnFormattingScript = compData.OnFormattingScript ?? ""
                    };

                    band.Components.Add(component);
                }

                report.Bands.Add(band);
            }

            report.DataSources.Clear();

            foreach (var dsData in data.DataSources ?? new List<DataSourceData>())
            {
                var ds = new DataSourceDefinition
                {
                    Id = dsData.Id,
                    Name = dsData.Name,
                    DataSourceType = dsData.DataSourceType,
                    ConnectionString = dsData.ConnectionString ?? "",
                    Query = dsData.Query ?? "",
                    TableName = dsData.TableName ?? "",
                    FilePath = dsData.FilePath ?? ""
                };

                report.DataSources.Add(ds);
            }

            return report;
        }
    }

    public class ReportData
    {
        public string Name { get; set; } = "";
        public string? Description { get; set; }
        public PageSettings? PageSettings { get; set; }
        public List<BandData> Bands { get; set; } = new();
        public List<DataSourceData> DataSources { get; set; } = new();
    }

    public class BandData
    {
        public string Id { get; set; } = "";
        public BandType BandType { get; set; }
        public string Name { get; set; } = "";
        public double Height { get; set; }
        public bool Visible { get; set; } = true;
        public string? DataSourceName { get; set; }
        public string? FilterExpression { get; set; }
        public string? SortExpression { get; set; }
        public List<ComponentData> Components { get; set; } = new();
    }

    public class ComponentData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public ComponentType ComponentType { get; set; }
        public double Left { get; set; }
        public double Top { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public bool Visible { get; set; } = true;
        public string? DataBinding { get; set; }
        public string? Text { get; set; }
        public string? FontFamily { get; set; }
        public double FontSize { get; set; } = 12;
        public bool FontBold { get; set; }
        public bool FontItalic { get; set; }
        public string? Foreground { get; set; }
        public string? Background { get; set; }
        public string? BorderColor { get; set; }
        public double BorderWidth { get; set; } = 1;
        public string? BorderStyle { get; set; }
        public System.Windows.HorizontalAlignment HorizontalAlignment { get; set; }
        public System.Windows.VerticalAlignment VerticalAlignment { get; set; }
        public string? ImagePath { get; set; }
        public string? Tooltip { get; set; }
        public bool UseScript { get; set; }
        public string? Script { get; set; }
        public string? ConditionalFormatting { get; set; }
        public string? OnBeforePrintScript { get; set; }
        public string? OnAfterPrintScript { get; set; }
        public string? OnGetValueScript { get; set; }
        public string? OnFormattingScript { get; set; }
    }

    public class DataSourceData
    {
        public string Id { get; set; } = "";
        public string Name { get; set; } = "";
        public DataSourceType DataSourceType { get; set; }
        public string? ConnectionString { get; set; }
        public string? Query { get; set; }
        public string? TableName { get; set; }
        public string? FilePath { get; set; }
    }
}
