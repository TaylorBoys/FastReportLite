using System.Collections.ObjectModel;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FastReportLite.Models;
using FastReportLite.Services;
using Microsoft.Win32;

namespace FastReportLite.ViewModels
{
    public partial class MainViewModel : ObservableObject
    {
        private readonly ReportSerializer _serializer = new();
        private readonly DataService _dataService = new();
        private readonly ExportService _exportService = new();

        [ObservableProperty]
        private Report? _currentReport;

        [ObservableProperty]
        private Band? _selectedBand;

        [ObservableProperty]
        private ReportComponent? _selectedComponent;

        [ObservableProperty]
        private string _currentFilePath = "";

        [ObservableProperty]
        private bool _hasUnsavedChanges = false;

        [ObservableProperty]
        private double _zoomLevel = 1.0;

        [ObservableProperty]
        private ObservableCollection<DataSourceDefinition> _dataSources = new();

        [ObservableProperty]
        private DataSourceDefinition? _selectedDataSource;

        [ObservableProperty]
        private bool _isPreviewMode = false;

        [ObservableProperty]
        private ObservableCollection<object> _previewData = new();

        public MainViewModel()
        {
            NewReport();
        }

        [RelayCommand]
        private void NewReport()
        {
            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show("当前报表有未保存的更改，是否保存?", "确认", 
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Cancel)
                    return;
                if (result == MessageBoxResult.Yes)
                    SaveReport();
            }

            CurrentReport = new Report();
            CurrentFilePath = "";
            HasUnsavedChanges = false;
            IsPreviewMode = false;
            DataSources = CurrentReport.DataSources;
            SelectedBand = CurrentReport.Bands.FirstOrDefault();
            SelectedComponent = null;
        }

        [RelayCommand]
        private void OpenReport()
        {
            if (HasUnsavedChanges)
            {
                var result = MessageBox.Show("当前报表有未保存的更改，是否保存?", "确认", 
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);
                
                if (result == MessageBoxResult.Cancel)
                    return;
                if (result == MessageBoxResult.Yes)
                    SaveReport();
            }

            var dialog = new OpenFileDialog
            {
                Filter = "报表文件 (*.frl)|*.frl|所有文件 (*.*)|*.*",
                DefaultExt = ".frl"
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    CurrentReport = _serializer.LoadFromFile(dialog.FileName);
                    CurrentFilePath = dialog.FileName;
                    HasUnsavedChanges = false;
                    IsPreviewMode = false;
                    DataSources = CurrentReport.DataSources;
                    SelectedBand = CurrentReport.Bands.FirstOrDefault();
                    SelectedComponent = null;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"打开报表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void SaveReport()
        {
            if (string.IsNullOrEmpty(CurrentFilePath))
            {
                SaveReportAs();
                return;
            }

            try
            {
                if (CurrentReport != null)
                {
                    _serializer.SaveToFile(CurrentReport, CurrentFilePath);
                    HasUnsavedChanges = false;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"保存报表失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        [RelayCommand]
        private void SaveReportAs()
        {
            var dialog = new SaveFileDialog
            {
                Filter = "报表文件 (*.frl)|*.frl|所有文件 (*.*)|*.*",
                DefaultExt = ".frl",
                FileName = CurrentReport?.Name ?? "Report"
            };

            if (dialog.ShowDialog() == true)
            {
                CurrentFilePath = dialog.FileName;
                SaveReport();
            }
        }

        [RelayCommand]
        private void AddBand(BandType bandType)
        {
            CurrentReport?.AddBand(bandType);
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void AddComponent(ComponentType componentType)
        {
            if (SelectedBand == null)
            {
                MessageBox.Show("请先选择一个波段!", "提示", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            var component = new ReportComponent(componentType)
            {
                Left = 10,
                Top = 10,
                Width = GetDefaultWidth(componentType),
                Height = GetDefaultHeight(componentType)
            };

            SelectedBand.Components.Add(component);
            SelectedComponent = component;
            HasUnsavedChanges = true;
        }

        private double GetDefaultWidth(ComponentType type)
        {
            return type switch
            {
                ComponentType.TextBox => 150,
                ComponentType.Picture => 200,
                ComponentType.Table => 300,
                ComponentType.Chart => 300,
                ComponentType.Line => 200,
                ComponentType.Barcode => 150,
                _ => 100
            };
        }

        private double GetDefaultHeight(ComponentType type)
        {
            return type switch
            {
                ComponentType.TextBox => 30,
                ComponentType.Picture => 150,
                ComponentType.Table => 100,
                ComponentType.Chart => 200,
                ComponentType.Line => 2,
                ComponentType.Barcode => 50,
                _ => 30
            };
        }

        [RelayCommand]
        private void DeleteSelectedComponent()
        {
            if (SelectedBand != null && SelectedComponent != null)
            {
                SelectedBand.Components.Remove(SelectedComponent);
                SelectedComponent = null;
                HasUnsavedChanges = true;
            }
        }

        [RelayCommand]
        private void Preview()
        {
            if (CurrentReport == null)
                return;

            IsPreviewMode = true;
            LoadPreviewData();
        }

        private void LoadPreviewData()
        {
            PreviewData.Clear();
            
            foreach (var ds in DataSources)
            {
                if (!string.IsNullOrEmpty(ds.Query) && ds.IsConnected)
                {
                    var data = _dataService.ExecuteQuery(ds);
                    foreach (var row in data)
                    {
                        PreviewData.Add(row);
                    }
                }
            }

            if (PreviewData.Count == 0)
            {
                for (int i = 0; i < 10; i++)
                {
                    PreviewData.Add(new Dictionary<string, object>
                    {
                        ["ID"] = i + 1,
                        ["Name"] = $"项目 {i + 1}",
                        ["Value"] = 100 * (i + 1),
                        ["Date"] = DateTime.Now.AddDays(-i)
                    });
                }
            }
        }

        [RelayCommand]
        private void ClosePreview()
        {
            IsPreviewMode = false;
        }

        [RelayCommand]
        private async Task ExportToPdf()
        {
            if (CurrentReport == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "PDF文件 (*.pdf)|*.pdf",
                DefaultExt = ".pdf",
                FileName = CurrentReport.Name
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _exportService.ExportToPdfAsync(CurrentReport, PreviewData, dialog.FileName);
                    MessageBox.Show("导出成功!", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task ExportToExcel()
        {
            if (CurrentReport == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "Excel文件 (*.xlsx)|*.xlsx",
                DefaultExt = ".xlsx",
                FileName = CurrentReport.Name
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _exportService.ExportToExcelAsync(CurrentReport, PreviewData, dialog.FileName);
                    MessageBox.Show("导出成功!", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private async Task ExportToHtml()
        {
            if (CurrentReport == null)
                return;

            var dialog = new SaveFileDialog
            {
                Filter = "HTML文件 (*.html)|*.html",
                DefaultExt = ".html",
                FileName = CurrentReport.Name
            };

            if (dialog.ShowDialog() == true)
            {
                try
                {
                    await _exportService.ExportToHtmlAsync(CurrentReport, PreviewData, dialog.FileName);
                    MessageBox.Show("导出成功!", "完成", MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"导出失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void ZoomIn()
        {
            if (ZoomLevel < 3.0)
                ZoomLevel += 0.1;
        }

        [RelayCommand]
        private void ZoomOut()
        {
            if (ZoomLevel > 0.3)
                ZoomLevel -= 0.1;
        }

        [RelayCommand]
        private void ResetZoom()
        {
            ZoomLevel = 1.0;
        }

        [RelayCommand]
        private void AddDataSource()
        {
            var dataSource = new DataSourceDefinition
            {
                Name = $"DataSource{DataSources.Count + 1}"
            };
            DataSources.Add(dataSource);
            CurrentReport?.DataSources.Add(dataSource);
            SelectedDataSource = dataSource;
            HasUnsavedChanges = true;
        }

        [RelayCommand]
        private void RemoveDataSource()
        {
            if (SelectedDataSource != null)
            {
                DataSources.Remove(SelectedDataSource);
                CurrentReport?.DataSources.Remove(SelectedDataSource);
                SelectedDataSource = null;
                HasUnsavedChanges = true;
            }
        }

        [RelayCommand]
        private void TestDataSourceConnection()
        {
            if (SelectedDataSource != null)
            {
                try
                {
                    SelectedDataSource.IsConnected = _dataService.TestConnection(SelectedDataSource);
                    if (SelectedDataSource.IsConnected)
                    {
                        MessageBox.Show("连接成功!", "成功", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        MessageBox.Show("连接失败!", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"连接失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        [RelayCommand]
        private void RefreshDataSourceFields()
        {
            if (SelectedDataSource != null && SelectedDataSource.IsConnected)
            {
                try
                {
                    var fields = _dataService.GetFields(SelectedDataSource);
                    SelectedDataSource.Fields.Clear();
                    foreach (var field in fields)
                    {
                        SelectedDataSource.Fields.Add(field);
                    }
                    HasUnsavedChanges = true;
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"获取字段失败: {ex.Message}", "错误", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }

        public void MarkAsChanged()
        {
            HasUnsavedChanges = true;
        }
    }
}
