using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace MacroPlayer;

public partial class AreaHelper : Window
{
    private ViewModel _viewModel = ViewModel.Instance; //new ViewModel();

    public AreaHelper()
    {
        InitializeComponent();

        DataContext = _viewModel;
        // Loaded += (sender, args) =>
        // {
        //     var myLayer = AdornerLayer.GetAdornerLayer(TheCanvas);
        //
        //     for (int i = 0; i < TheCanvas.Items.Count; i++)
        //     {
        //         Debug.Assert(myLayer != null, nameof(myLayer) + " != null");
        //         myLayer.Add(new ResizeAdorner(
        //             (UIElement)TheCanvas.ItemContainerGenerator.ContainerFromIndex(i)
        //         ));
        //     }
        // };
    }

    //
    // private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
    // {
    //     var delta = new Point(e.HorizontalChange, e.VerticalChange);
    //     Resize(((DirectionalThumb)sender).Orientation, delta);
    // }
    //
    //
    // private CompassOrientation[] marginRotations =
    // {
    //     CompassOrientation.SouthWest, CompassOrientation.West, CompassOrientation.NorthWest, CompassOrientation.North,
    //     CompassOrientation.NorthEast
    // };
    //
    // void Resize(CompassOrientation compassOrientation, Point delta)
    // {
    //     var newWidth = compassOrientation switch
    //     {
    //         CompassOrientation.NorthWest or CompassOrientation.West or CompassOrientation.SouthWest =>
    //             MyRectangle.Width - delta.X,
    //         CompassOrientation.NorthEast or CompassOrientation.East or CompassOrientation.SouthEast =>
    //             MyRectangle.Width + delta.X,
    //         _ => MyRectangle.Width
    //     };
    //
    //     var newHeight = compassOrientation switch
    //     {
    //         CompassOrientation.NorthWest or CompassOrientation.North or CompassOrientation.NorthEast => MyRectangle
    //             .Height - delta.Y,
    //         CompassOrientation.SouthWest or CompassOrientation.South or CompassOrientation.SouthEast => MyRectangle
    //             .Height + delta.Y,
    //         _ => MyRectangle.Height
    //     };
    //
    //     var newX = compassOrientation switch
    //     {
    //         CompassOrientation.SouthWest or CompassOrientation.West or CompassOrientation.NorthWest => MyRectangle
    //             .Margin.Left + delta.X,
    //         _ => MyRectangle.Margin.Left
    //     };
    //
    //     var newY = compassOrientation switch
    //     {
    //         CompassOrientation.NorthWest or CompassOrientation.North or CompassOrientation.NorthEast => MyRectangle
    //             .Margin.Top + delta.Y,
    //         _ => MyRectangle.Margin.Top
    //     };
    //
    //     newWidth = Math.Max(newWidth, 20);
    //     newHeight = Math.Max(newHeight, 20);
    //
    //     var rect = new Rect(
    //         newX,
    //         newY,
    //         newWidth,
    //         newHeight);
    //
    //     Resize(compassOrientation, rect);
    // }
    //
    //
    // void Resize(CompassOrientation compassOrientation, Rect newSize)
    // {
    //     var newMargin = MyRectangle.Margin;
    //     newMargin.Left = newSize.Left;
    //     newMargin.Top = newSize.Top;
    //     MyRectangle.Margin = newMargin;
    //
    //     MyRectangle.Width = newSize.Width;
    //     MyRectangle.Height = newSize.Height;
    // }
    private void RectangleLoaded(object sender, RoutedEventArgs e)
    {
        var myLayer = AdornerLayer.GetAdornerLayer(TheCanvas);

        Debug.Assert(myLayer != null, nameof(myLayer) + " != null");
        var rectangleContainer = VisualTreeHelper.GetParent((UIElement)sender);

        Debug.Assert(rectangleContainer != null, nameof(rectangleContainer) + " != null");
        myLayer.Add(new ResizeAdorner((UIElement)rectangleContainer));
    }
}