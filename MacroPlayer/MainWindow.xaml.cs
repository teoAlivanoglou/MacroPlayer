using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Xceed.Wpf.Toolkit;

namespace MacroPlayer
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private AreaHelper _areaHelper = new();
        private ViewModel _viewModel = ViewModel.Instance;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = _viewModel;
        }

        private void ButtonOpenOverlayClick(object sender, RoutedEventArgs e)
        {
            _viewModel.OverlayOpen = !_viewModel.OverlayOpen;

            if (_viewModel.OverlayOpen)
                _areaHelper.Show();
            else
                _areaHelper.Hide();
        }

        private void MainWindow_OnClosed(object? sender, EventArgs e)
        {
            _areaHelper.Close();
        }


        private void ButtonAddRectClick(object sender, RoutedEventArgs e)
        {
            var newClipArea = new ClipArea(Random.Shared.NextDouble() * 300, Random.Shared.NextDouble() * 150, 100, 100,
                "New Rect Area");

            var nextColor = new byte[4];
            Random.Shared.NextBytes(nextColor);

            newClipArea.FillColor = Color.FromArgb(nextColor[0], nextColor[1], nextColor[2], nextColor[3]); 
            _viewModel.clipAreas.Add(newClipArea);
        }
    }
}