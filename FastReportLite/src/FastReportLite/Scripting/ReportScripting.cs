#nullable disable

using System;
using System.Collections.Generic;
using FastReportLite.Models;

namespace FastReportLite.Scripting
{
    public class ReportScriptContext
    {
        public Report Report { get; set; }
        public Dictionary<string, object> DataRow { get; set; }
        public int RowIndex { get; set; }
        public ReportComponent CurrentComponent { get; set; }
        
        public object this[string fieldName]
        {
            get
            {
                if (DataRow != null && DataRow.TryGetValue(fieldName, out var value))
                    return value;
                return null;
            }
            set
            {
                if (DataRow != null)
                    DataRow[fieldName] = value;
            }
        }
    }

    public class ReportScripting
    {
        public ReportScriptContext Context { get; private set; }
        
        public ReportScripting(Report report, Dictionary<string, object> dataRow, int rowIndex, ReportComponent component)
        {
            Context = new ReportScriptContext
            {
                Report = report,
                DataRow = dataRow,
                RowIndex = rowIndex,
                CurrentComponent = component
            };
        }

        public void SetComponentText(string text)
        {
            if (Context.CurrentComponent != null)
            {
                Context.CurrentComponent.Text = text;
            }
        }

        public void SetComponentVisible(bool visible)
        {
            if (Context.CurrentComponent != null)
            {
                Context.CurrentComponent.Visible = visible;
            }
        }

        public void SetComponentBackColor(string color)
        {
            if (Context.CurrentComponent != null)
            {
                Context.CurrentComponent.Background = color;
            }
        }

        public void SetComponentFontColor(string color)
        {
            if (Context.CurrentComponent != null)
            {
                Context.CurrentComponent.Foreground = color;
            }
        }

        public object GetField(string fieldName)
        {
            return Context[fieldName];
        }

        public void SetField(string fieldName, object value)
        {
            Context[fieldName] = value;
        }

        public string FormatNumber(object value, string format)
        {
            if (value == null) return string.Empty;
            try
            {
                return Convert.ToDecimal(value).ToString(format);
            }
            catch
            {
                return value.ToString();
            }
        }

        public string FormatDate(object value, string format)
        {
            if (value == null) return string.Empty;
            try
            {
                return Convert.ToDateTime(value).ToString(format);
            }
            catch
            {
                return value.ToString();
            }
        }

        public bool IsEvenRow()
        {
            return Context.RowIndex % 2 == 0;
        }

        public bool IsOddRow()
        {
            return Context.RowIndex % 2 != 0;
        }

        public ReportComponent FindComponent(string name)
        {
            if (Context.Report == null) return null;
            
            foreach (var band in Context.Report.Bands)
            {
                foreach (var comp in band.Components)
                {
                    if (comp.Name == name)
                        return comp;
                }
            }
            return null;
        }
    }
}
