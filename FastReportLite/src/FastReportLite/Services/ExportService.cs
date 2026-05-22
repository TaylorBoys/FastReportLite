#nullable disable

using System.IO;
using System.Text;
using System.Linq;
using System.Collections.Generic;
using FastReportLite.Models;
using FastReportLite.Scripting;
using PdfSharp.Drawing;
using PdfSharp.Pdf;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;

namespace FastReportLite.Services
{
    public class ExportService
    {
        private AdvancedScriptEngine _scriptEngine;

        public ExportService()
        {
            _scriptEngine = new AdvancedScriptEngine();
        }

        public async Task ExportToPdfAsync(Report report, IList<object> data, string filePath)
        {
            await Task.Run(() =>
            {
                var document = new PdfDocument();
                document.Info.Title = report.Name;
                document.Info.Author = "FastReportLite";

                var pageWidth = report.PageSettings.Landscape ? report.PageSettings.Height : report.PageSettings.Width;
                var pageHeight = report.PageSettings.Landscape ? report.PageSettings.Width : report.PageSettings.Height;

                var page = document.AddPage();
                page.Width = XUnit.FromMillimeter(pageWidth);
                page.Height = XUnit.FromMillimeter(pageHeight);

                var gfx = XGraphics.FromPdfPage(page);
                double yPosition = 20;

                foreach (var band in report.Bands)
                {
                    if (!band.Visible)
                        continue;

                    var fontSize = band.BandType == BandType.ReportTitle ? 18.0 : 
                                   band.BandType == BandType.PageHeader ? 12.0 : 10.0;
                    var font = new XFont("Segoe UI", fontSize);

                    foreach (var component in band.Components)
                    {
                        if (!component.Visible)
                            continue;

                        ExecuteBeforePrintEvent(report, component, null, 0);
                        
                        var x = component.Left + report.PageSettings.LeftMargin;
                        var y = yPosition + component.Top;

                        switch (component.ComponentType)
                        {
                            case ComponentType.TextBox:
                                DrawPdfTextBox(gfx, component, x, y, font, null, null, 0);
                                break;
                            case ComponentType.Line:
                                DrawPdfLine(gfx, component, x, y);
                                break;
                        }
                        
                        ExecuteAfterPrintEvent(report, component, null, 0);
                    }

                    if (band.BandType == BandType.Data && data.Count > 0)
                    {
                        var firstDataRow = data[0] as Dictionary<string, object>;
                        if (firstDataRow != null)
                        {
                            yPosition += band.Height + 10;
                            
                            int rowIndex = 0;
                            foreach (var row in data)
                            {
                                if (row is Dictionary<string, object> dataRow)
                                {
                                    foreach (var component in band.Components)
                                    {
                                        if (!component.Visible || component.ComponentType != ComponentType.TextBox)
                                            continue;

                                        ExecuteBeforePrintEvent(report, component, dataRow, rowIndex);
                                        
                                        var x = component.Left + report.PageSettings.LeftMargin;
                                        var y = yPosition + component.Top;

                                        DrawPdfTextBox(gfx, component, x, y, font, dataRow, report, rowIndex);
                                        
                                        ExecuteAfterPrintEvent(report, component, dataRow, rowIndex);
                                    }

                                    yPosition += 20;
                                    rowIndex++;
                                    
                                    if (rowIndex >= 30)
                                        break;
                                }
                            }
                        }
                    }

                    yPosition += band.Height + 5;
                }

                document.Save(filePath);
            });
        }

        private void DrawPdfTextBox(XGraphics gfx, ReportComponent component, double x, double y, XFont font, 
            Dictionary<string, object> dataRow, Report report, int rowIndex)
        {
            string text = GetComponentValue(component, dataRow, report, rowIndex);
            
            string foregroundColor = component.Foreground;
            if (!string.IsNullOrEmpty(component.OnFormattingScript) && report != null)
            {
                foregroundColor = ExecuteFormattingEvent(report, component, dataRow, rowIndex);
            }

            var brush = new XSolidBrush(XColor.FromArgb(GetColorFromHex(foregroundColor)));
            gfx.DrawString(text, font, brush, new XPoint(x, y + component.FontSize));
        }

        private string GetComponentValue(ReportComponent component, Dictionary<string, object> dataRow, Report report, int rowIndex)
        {
            if (!string.IsNullOrEmpty(component.OnGetValueScript) && report != null)
            {
                var result = ExecuteGetValueEvent(report, component, dataRow, rowIndex);
                if (result != null)
                    return result.ToString() ?? "";
            }

            if (component.UseScript && !string.IsNullOrEmpty(component.Script))
            {
                var globals = new ReportScriptGlobals
                {
                    Report = report,
                    Data = dataRow ?? new Dictionary<string, object>()
                };
                var result = _scriptEngine.ExecuteScript(component.Script, globals);
                return result?.ToString() ?? "";
            }
            else if (!string.IsNullOrEmpty(component.DataBinding) && dataRow != null)
            {
                var fieldName = component.DataBinding.Replace("{", "").Replace("}", "");
                if (dataRow.ContainsKey(fieldName))
                {
                    return dataRow[fieldName]?.ToString() ?? "";
                }
            }

            return component.Text;
        }

        private void ExecuteBeforePrintEvent(Report report, ReportComponent component, Dictionary<string, object> dataRow, int rowIndex)
        {
            if (string.IsNullOrEmpty(component.OnBeforePrintScript))
                return;

            var globals = new ReportScriptGlobals
            {
                Report = report,
                Data = dataRow ?? new Dictionary<string, object>()
            };

            _scriptEngine.ExecuteScript(component.OnBeforePrintScript, globals);
        }

        private void ExecuteAfterPrintEvent(Report report, ReportComponent component, Dictionary<string, object> dataRow, int rowIndex)
        {
            if (string.IsNullOrEmpty(component.OnAfterPrintScript))
                return;

            var globals = new ReportScriptGlobals
            {
                Report = report,
                Data = dataRow ?? new Dictionary<string, object>()
            };

            _scriptEngine.ExecuteScript(component.OnAfterPrintScript, globals);
        }

        private object ExecuteGetValueEvent(Report report, ReportComponent component, Dictionary<string, object> dataRow, int rowIndex)
        {
            if (string.IsNullOrEmpty(component.OnGetValueScript))
                return null;

            var globals = new ReportScriptGlobals
            {
                Report = report,
                Data = dataRow ?? new Dictionary<string, object>()
            };

            return _scriptEngine.ExecuteScript(component.OnGetValueScript, globals);
        }

        private string ExecuteFormattingEvent(Report report, ReportComponent component, Dictionary<string, object> dataRow, int rowIndex)
        {
            if (string.IsNullOrEmpty(component.OnFormattingScript))
                return component.Foreground;

            var globals = new ReportScriptGlobals
            {
                Report = report,
                Data = dataRow ?? new Dictionary<string, object>()
            };

            var result = _scriptEngine.ExecuteScript(component.OnFormattingScript, globals);
            return result?.ToString() ?? component.Foreground;
        }

        private void DrawPdfLine(XGraphics gfx, ReportComponent component, double x, double y)
        {
            var pen = new XPen(XColor.FromArgb(GetColorFromHex(component.BorderColor)), component.BorderWidth);
            gfx.DrawLine(pen, x, y, x + component.Width, y);
        }

        public async Task ExportToExcelAsync(Report report, IList<object> data, string filePath)
        {
            await Task.Run(() =>
            {
                using var spreadsheetDocument = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook);
                
                var workbookPart = spreadsheetDocument.AddWorkbookPart();
                workbookPart.Workbook = new Workbook();

                var worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                worksheetPart.Worksheet = new Worksheet();

                var sheetData = worksheetPart.Worksheet.AppendChild(new SheetData());
                uint rowIndex = 1;

                foreach (var band in report.Bands)
                {
                    if (!band.Visible)
                        continue;

                    foreach (var component in band.Components)
                    {
                        if (!component.Visible || component.ComponentType != ComponentType.TextBox)
                            continue;

                        ExecuteBeforePrintEvent(report, component, null, 0);
                        
                        var row = new Row { RowIndex = rowIndex };
                        var cell = new Cell
                        {
                            CellReference = $"A{rowIndex}",
                            CellValue = new CellValue(GetComponentValue(component, null, report, 0)),
                            DataType = CellValues.String
                        };
                        row.Append(cell);
                        sheetData.Append(row);
                        rowIndex++;
                        
                        ExecuteAfterPrintEvent(report, component, null, 0);
                    }

                    if (band.BandType == BandType.Data)
                    {
                        int dataRowIndex = 0;
                        foreach (var rowData in data)
                        {
                            if (rowData is Dictionary<string, object> dataRow)
                            {
                                var row = new Row { RowIndex = rowIndex };
                                int colIndex = 0;

                                foreach (var component in band.Components)
                                {
                                    if (!component.Visible || component.ComponentType != ComponentType.TextBox)
                                        continue;

                                    ExecuteBeforePrintEvent(report, component, dataRow, dataRowIndex);
                                    
                                    var cell = new Cell
                                    {
                                        CellReference = $"{GetColumnLetter(colIndex)}{rowIndex}",
                                        CellValue = new CellValue(GetComponentValue(component, dataRow, report, dataRowIndex)),
                                        DataType = CellValues.String
                                    };
                                    row.Append(cell);
                                    colIndex++;
                                    
                                    ExecuteAfterPrintEvent(report, component, dataRow, dataRowIndex);
                                }

                                sheetData.Append(row);
                                rowIndex++;
                                dataRowIndex++;

                                if (rowIndex >= 1000)
                                    break;
                            }
                        }
                    }

                    rowIndex++;
                }

                workbookPart.Workbook.Save();
            });
        }

        public async Task ExportToHtmlAsync(Report report, IList<object> data, string filePath)
        {
            await Task.Run(() =>
            {
                var html = new StringBuilder();
                
                html.AppendLine("<!DOCTYPE html>");
                html.AppendLine("<html lang=\"zh-CN\">");
                html.AppendLine("<head>");
                html.AppendLine("    <meta charset=\"UTF-8\">");
                html.AppendLine($"    <title>{report.Name}</title>");
                html.AppendLine("    <style>");
                html.AppendLine("        body { font-family: 'Segoe UI', sans-serif; margin: 20px; }");
                html.AppendLine("        .report-title { font-size: 24px; font-weight: bold; text-align: center; margin-bottom: 20px; }");
                html.AppendLine("        .band { margin-bottom: 10px; }");
                html.AppendLine("        .textbox { padding: 5px; }");
                html.AppendLine("        table { border-collapse: collapse; width: 100%; margin-top: 10px; }");
                html.AppendLine("        th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }");
                html.AppendLine("        th { background-color: #2D5A8A; color: white; }");
                html.AppendLine("    </style>");
                html.AppendLine("</head>");
                html.AppendLine("<body>");

                foreach (var band in report.Bands)
                {
                    if (!band.Visible)
                        continue;

                    html.AppendLine($"    <div class=\"band\">");

                    foreach (var component in band.Components)
                    {
                        if (!component.Visible)
                            continue;

                        ExecuteBeforePrintEvent(report, component, null, 0);
                        
                        switch (component.ComponentType)
                        {
                            case ComponentType.TextBox:
                                var fontStyle = "";
                                if (component.FontBold) fontStyle += "font-weight: bold; ";
                                if (component.FontItalic) fontStyle += "font-style: italic; ";
                                
                                string text = GetComponentValue(component, null, report, 0);
                                
                                if (band.BandType == BandType.ReportTitle)
                                {
                                    html.AppendLine($"        <div class=\"report-title\" style=\"{fontStyle}\">{text}</div>");
                                }
                                else
                                {
                                    html.AppendLine($"        <div class=\"textbox\" style=\"{fontStyle}\">{text}</div>");
                                }
                                break;

                            case ComponentType.Line:
                                html.AppendLine($"        <hr style=\"border: {component.BorderWidth}px solid {component.BorderColor}\">");
                                break;
                        }
                        
                        ExecuteAfterPrintEvent(report, component, null, 0);
                    }

                    html.AppendLine("    </div>");
                }

                var dataBand = report.Bands.FirstOrDefault(b => b.BandType == BandType.Data);
                if (dataBand != null && data.Count > 0)
                {
                    html.AppendLine("    <table>");
                    
                    var componentHeaders = new List<(string, ReportComponent)>();
                    html.Append("        <tr>");
                    foreach (var component in dataBand.Components)
                    {
                        if (component.ComponentType == ComponentType.TextBox)
                        {
                            html.Append($"<th>{component.Text}</th>");
                            componentHeaders.Add((component.DataBinding?.Replace("{", "").Replace("}", "") ?? "", component));
                        }
                    }
                    html.AppendLine("</tr>");

                    int dataRowIndex = 0;
                    foreach (var row in data)
                    {
                        if (row is Dictionary<string, object> dataRow)
                        {
                            html.Append("        <tr>");
                            foreach (var (header, component) in componentHeaders)
                            {
                                ExecuteBeforePrintEvent(report, component, dataRow, dataRowIndex);
                                string text = GetComponentValue(component, dataRow, report, dataRowIndex);
                                html.Append($"<td>{text}</td>");
                                ExecuteAfterPrintEvent(report, component, dataRow, dataRowIndex);
                            }
                            html.AppendLine("</tr>");
                            dataRowIndex++;
                        }
                    }

                    html.AppendLine("    </table>");
                }

                html.AppendLine("</body>");
                html.AppendLine("</html>");

                File.WriteAllText(filePath, html.ToString(), Encoding.UTF8);
            });
        }

        private string GetColumnLetter(int columnIndex)
        {
            var letters = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            if (columnIndex < 26)
                return letters[columnIndex].ToString();
            return letters[columnIndex / 26 - 1] + letters[columnIndex % 26].ToString();
        }

        private int GetColorFromHex(string hex)
        {
            try
            {
                if (hex.StartsWith("#"))
                    hex = hex.Substring(1);
                
                if (hex.Length == 6)
                {
                    var r = Convert.ToInt32(hex.Substring(0, 2), 16);
                    var g = Convert.ToInt32(hex.Substring(2, 2), 16);
                    var b = Convert.ToInt32(hex.Substring(4, 2), 16);
                    return (255 << 24) | (r << 16) | (g << 8) | b;
                }
            }
            catch { }
            return (255 << 24) | (51 << 16) | (51 << 8) | 51;
        }
    }
}
