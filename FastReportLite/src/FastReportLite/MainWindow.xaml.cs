using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using FastReportLite.Models;
using FastReportLite.ViewModels;

namespace FastReportLite
{
    public partial class MainWindow : Window
    {
        private MainViewModel ViewModel => (MainViewModel)DataContext;

        private bool _isDragging = false;
        private Point _dragStartPoint;
        private ReportComponent? _draggedComponent;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void Exit_Click(object sender, RoutedEventArgs e)
        {
            if (ViewModel.HasUnsavedChanges)
            {
                var result = MessageBox.Show("当前报表有未保存的更改，是否保存?", "确认",
                    MessageBoxButton.YesNoCancel, MessageBoxImage.Question);

                if (result == MessageBoxResult.Cancel)
                    return;
                if (result == MessageBoxResult.Yes)
                    ViewModel.SaveReportCommand.Execute(null);
            }

            Application.Current.Shutdown();
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "FastReportLite v1.0\n\n" +
                "轻量级WPF报表设计工具\n\n" +
                "功能特点:\n" +
                "- 可视化报表设计\n" +
                "- 多组件支持\n" +
                "- 数据源连接\n" +
                "- 多种导出格式\n\n" +
                "© 2024 FastReportLite",
                "关于 FastReportLite",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Component_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element && element.DataContext is ReportComponent component)
            {
                ViewModel.SelectedComponent = component;
                _isDragging = true;
                _dragStartPoint = e.GetPosition(DesignerCanvas);
                _draggedComponent = component;
                element.CaptureMouse();
                e.Handled = true;
            }
        }

        private void Component_MouseMove(object sender, MouseEventArgs e)
        {
            if (_isDragging && _draggedComponent != null && e.LeftButton == MouseButtonState.Pressed)
            {
                var currentPosition = e.GetPosition(DesignerCanvas);
                var deltaX = currentPosition.X - _dragStartPoint.X;
                var deltaY = currentPosition.Y - _dragStartPoint.Y;

                _draggedComponent.Left = Math.Max(0, _draggedComponent.Left + deltaX);
                _draggedComponent.Top = Math.Max(0, _draggedComponent.Top + deltaY);

                _dragStartPoint = currentPosition;
                ViewModel.MarkAsChanged();
                e.Handled = true;
            }
        }

        private void Component_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is FrameworkElement element)
            {
                _isDragging = false;
                _draggedComponent = null;
                element.ReleaseMouseCapture();
                e.Handled = true;
            }
        }
    }
}
