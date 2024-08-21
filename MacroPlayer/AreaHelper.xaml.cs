using System;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace MacroPlayer;

public partial class AreaHelper : Window
{
    public AreaHelper()
    {
        InitializeComponent();
    }

    private bool _mouseDown = false;

    private void UIElement_OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Pressed)
        {
            _mouseDown = true;
        }
    }

    private void UIElement_OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (e.LeftButton == MouseButtonState.Released)
        {
            _mouseDown = false;
        }
    }


    private void UIElement_OnMouseMove(object sender, MouseEventArgs e)
    {
        if (_mouseDown)
        {
        }
    }

    private void Thumb_OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        Resize(CompassOrientation.North, e.VerticalChange);
    }


    

    private CompassOrientation[] marginRotations =
        { CompassOrientation.SouthWest, CompassOrientation.West, CompassOrientation.NorthWest, CompassOrientation.North, CompassOrientation.NorthEast };

    void Resize(CompassOrientation compassOrientation, double delta)
    {
        var newWidth = compassOrientation switch
        {
            CompassOrientation.NorthWest or CompassOrientation.West or CompassOrientation.SouthWest => MyRectangle.Width - delta,
            CompassOrientation.NorthEast or CompassOrientation.East or CompassOrientation.SouthEast => MyRectangle.Width + delta,
            _ => MyRectangle.Width
        };

        var newHeight = compassOrientation switch
        {
            CompassOrientation.NorthWest or CompassOrientation.North or CompassOrientation.NorthEast => MyRectangle.Height - delta,
            CompassOrientation.SouthWest or CompassOrientation.South or CompassOrientation.SouthEast => MyRectangle.Height + delta,
            _ => MyRectangle.Height
        };

        var newX = compassOrientation switch
        {
            CompassOrientation.SouthWest or CompassOrientation.West or CompassOrientation.NorthWest => MyRectangle.Margin.Left + delta,
            _ => MyRectangle.Margin.Left
        };

        var newY = compassOrientation switch
        {
            CompassOrientation.NorthWest or CompassOrientation.North or CompassOrientation.NorthEast => MyRectangle.Margin.Top + delta,
            _ => MyRectangle.Margin.Top
        };
        
        
        if (newWidth < 20 || newHeight < 25) return;

        var rect = new Rect(
            newX,
            newY,
            newWidth,
            newHeight);

        Resize(compassOrientation, rect);
    }


    void Resize(CompassOrientation compassOrientation, Rect newSize)
    {
        // var sign = 1;
        // if (orientation is Orientation.SouthWest
        // or Orientation.West
        // or Orientation.NorthWest
        // or Orientation.North
        // or Orientation.NorthEast)
        // {
        var newMargin = MyRectangle.Margin;
        newMargin.Left = newSize.Left;
        newMargin.Top = newSize.Top;
        MyRectangle.Margin = newMargin;
        // sign = -1;
        // }

        MyRectangle.Width = newSize.Width;
        MyRectangle.Height = newSize.Height;
    }
}