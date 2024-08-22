using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
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
using Compunet.ScreenCapture;
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
        private ScreenCaptureService _screenCaptureService = new();

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

        private async void ButtonCaptureClick(object sender, RoutedEventArgs e)
        {
            _areaHelper.Hide();
            var screenshot = _screenCaptureService.CaptureScreen();
            _areaHelper.Show();

            var bmp = screenshot.GetBitmapSource();
            
            var croppedBitmaps = new CroppedBitmap[ViewModel.Instance.clipAreas.Count];

            for (int i = 0; i < croppedBitmaps.Length; i++)
            {
                var aa = ViewModel.Instance.clipAreas[i].RectArea;
                croppedBitmaps[i] = new CroppedBitmap(bmp, new Int32Rect((int)(aa.X - ResizeAdorner.margin), (int)(aa.Y - ResizeAdorner.margin), (int)aa.Width, (int)aa.Height));
            }

            for (var index = 0; index < croppedBitmaps.Length; index++)
            {
                await using var mStream = new FileStream($"{ViewModel.Instance.clipAreas[index].Name}.png", FileMode.Create);
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(croppedBitmaps[index]));
                encoder.Save(mStream);
            }

            // screenshot.WriteToFile("ScreenCaptureOutput.png");
        }


        /// <summary>
        /// Take screenshot of a Window.
        /// </summary>
        /// <remarks>
        /// - Usage example: screenshot icon in every window header.                
        /// - Keep well away from any Windows Forms based methods that involve screen pixels. You will run into scaling issues at different
        ///   monitor DPI values. Quote: "Keep in mind though that WPF units aren't pixels, they're device-independent @ 96DPI
        ///   "pixelish-units"; so really what you want, is the scale factor between 96DPI and the current screen DPI (so like 1.5 for
        ///   144DPI) - Paul Betts."
        /// </remarks>
        public async Task<bool> TryScreenshotToClipboardAsync(FrameworkElement frameworkElement)
        {
            // frameworkElement.ClipToBounds = true; // Can remove if everything still works when the screen is maximised.

            Rect relativeBounds = VisualTreeHelper.GetDescendantBounds(frameworkElement);
            double
                areaWidth = frameworkElement.RenderSize
                    .Width; // Cannot use relativeBounds.Width as this may be incorrect if a window is maximised.
            double areaHeight = frameworkElement.RenderSize.Height; // Cannot use relativeBounds.Height for same reason.
            double XLeft = relativeBounds.X;
            double XRight = XLeft + areaWidth;
            double YTop = relativeBounds.Y;
            double YBottom = YTop + areaHeight;
            var bitmap = new RenderTargetBitmap((int)Math.Round(XRight, MidpointRounding.AwayFromZero),
                (int)Math.Round(YBottom, MidpointRounding.AwayFromZero),
                96, 96, PixelFormats.Default);

            // Render framework element to a bitmap. This works better than any screen-pixel-scraping methods which will pick up unwanted
            // artifacts such as the taskbar or another window covering the current window.
            var dv = new DrawingVisual();
            using (DrawingContext ctx = dv.RenderOpen())
            {
                var vb = new VisualBrush(frameworkElement);
                ctx.DrawRectangle(vb, null, new Rect(new Point(XLeft, YTop), new Point(XRight, YBottom)));
            }

            bitmap.Render(dv);
            
            BitmapFrame frame = BitmapFrame.Create(bitmap);
            BitmapEncoder encoder = new BmpBitmapEncoder();
            encoder.Frames.Add(frame);

            using (var stream = File.Create("testImageScreenshot.png"))
            {
                encoder.Save(stream);
            }
            
            return await TryCopyBitmapToClipboard(bitmap);
        }

        private static async Task<bool> TryCopyBitmapToClipboard(BitmapSource bmpCopied)
        {
            var tries = 3;
            while (tries-- > 0)
            {
                try
                {
                    // This must be executed on the calling dispatcher.
                    Clipboard.SetImage(bmpCopied);
                    return true;
                }
                catch (COMException)
                {
                    // Windows clipboard is optimistic concurrency. On fail (as in use by another process), retry.
                    await Task.Delay(TimeSpan.FromMilliseconds(100));
                }
            }

            return false;
        }
    }
}