using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Shapes;
using Color = System.Windows.Media.Color;
using Size = System.Windows.Size;

namespace MacroPlayer;

public class ResizeAdorner : Adorner
{
    private static double _thumbSize = 30;
    private Vector _thumbOffset = new(-_thumbSize / 2, -_thumbSize / 2);
    private readonly VisualCollection _visualCollection;
    private readonly Dictionary<CompassOrientation, DirectionalThumb> _thumbs = new();

    private readonly OutlinedTextBlock _outlinedTextBlock;
    private readonly Border _outlinedTextBox;
    private GeometryDrawing _textBoxGeometry;

    private readonly Rectangle _border;
    
    const double margin = 8;

    private FrameworkElement Parent { get; init; }

    public ResizeAdorner(UIElement adornedElement) : base(adornedElement)
    {
        Parent = VisualTreeHelper.GetParent(adornedElement) as FrameworkElement ??
                 throw new InvalidOperationException();
        Debug.Assert(Parent is Canvas, nameof(Parent) + " is Canvas");


        _visualCollection = new VisualCollection(this);

        _border = new Rectangle
        {
            Stroke = new SolidColorBrush(Color.FromArgb(150, 18, 18, 18)),
            StrokeThickness = 2,
            StrokeDashArray = { 3, 3 },
            RadiusX = 4,
            RadiusY = 4,
        };

        _visualCollection.Add(_border);

        foreach (var orientation in Enum.GetValues<CompassOrientation>())
        {
            _thumbs.Add(orientation,
                new DirectionalThumb
                    { Orientation = orientation, Width = 30, Height = 30, Cursor = GetCursor(orientation) });
            _visualCollection.Add(_thumbs[orientation]);
            _thumbs[orientation].DragDelta += OnDragDelta;
        }

        _textBoxGeometry = new GeometryDrawing(new SolidColorBrush(Colors.White),
            new Pen(new SolidColorBrush(Colors.Black), 1), null);

        _outlinedTextBlock = new OutlinedTextBlock
        {
            Fill = new SolidColorBrush(Colors.White),
            Stroke = new SolidColorBrush(Colors.Black),
            StrokeThickness = 2,
            FontWeight = FontWeights.Bold,
            FontSize = 14
        };


        _outlinedTextBlock.SetBinding(OutlinedTextBlock.TextProperty, "Name");
        _outlinedTextBlock.DataContext = ((ContentPresenter)adornedElement).DataContext;

        // _visualCollection.Add(_outlinedTextBlock);


        var dropShadowEffect = new DropShadowEffect
        {
            BlurRadius = 5,
            Color = Colors.Black,
            ShadowDepth = 0
        };

        _outlinedTextBox = new Border();
        var actualTextBox = new TextBox();
        actualTextBox.Effect = dropShadowEffect;
        actualTextBox.SetBinding(TextBox.TextProperty, "Name");
        actualTextBox.DataContext = ((ContentPresenter)adornedElement).DataContext;
        actualTextBox.FontSize = 14;
        actualTextBox.FontWeight = FontWeights.Bold;
        actualTextBox.HorizontalAlignment = HorizontalAlignment.Left;
        actualTextBox.VerticalAlignment = VerticalAlignment.Top;
        actualTextBox.Background = new SolidColorBrush(Colors.Transparent);
        actualTextBox.Foreground = new SolidColorBrush(Colors.White);
        actualTextBox.BorderThickness = new Thickness(0);
        actualTextBox.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
        actualTextBox.KeyDown += ActualTextBoxOnKeyDown;
        actualTextBox.GotFocus += ActualTextBoxOnGotFocus;
        // actualTextBox.Text = ((ClipArea)((ContentPresenter)adornedElement).DataContext).Name;

        _outlinedTextBox.Child = actualTextBox;
        _outlinedTextBox.Effect = dropShadowEffect;

        _visualCollection.Add(_outlinedTextBox);


        adornedElement.MouseLeftButtonDown += MouseDown;
        adornedElement.MouseLeftButtonUp += MouseUp;
        adornedElement.MouseMove += MouseMove;
    }

    private string LastName = String.Empty;

    private void ActualTextBoxOnGotFocus(object sender, RoutedEventArgs e)
    {
        LastName = ((TextBox)sender).Text;
    }

    private void ActualTextBoxOnKeyDown(object sender, KeyEventArgs e)
    {
        switch (e.Key)
        {
            case Key.Escape:
                ((TextBox)sender).Text = LastName;
                LastName = string.Empty;
                goto case Key.Enter;
            case Key.Tab:
            case Key.Enter:
                e.Handled = true;
                ((TextBox)sender).MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                break;
        }
    }


    private bool _isRectDragInProg;
    private Point clickOffset;

    void MouseDown(object sender, MouseButtonEventArgs e)
    {
        _isRectDragInProg = true;
        clickOffset = e.GetPosition(AdornedElement);

        ((FrameworkElement)AdornedElement).Cursor = Cursors.SizeAll;
        AdornedElement.CaptureMouse();
    }

    void MouseUp(object sender, MouseButtonEventArgs e)
    {
        _isRectDragInProg = false;
        ((FrameworkElement)AdornedElement).Cursor = null;
        AdornedElement.ReleaseMouseCapture();
    }

    void MouseMove(object sender, MouseEventArgs e)
    {
        if (!_isRectDragInProg) return;

        if (AdornedElement is ContentPresenter presenter)
        {
            var clipArea = (ClipArea)presenter.Content;
            var mousePos = e.GetPosition(Parent);

            Move(clipArea, Parent, mousePos, clickOffset);
        }
    }

    void Move(ClipArea clipArea, FrameworkElement parent, Point position, Point clickOffset)
    {

        var newRect = clipArea.RectArea;

        newRect.X = Math.Clamp(position.X - clickOffset.X, margin, parent.ActualWidth - newRect.Width - margin);
        newRect.Y = Math.Clamp(position.Y - clickOffset.Y, margin, parent.ActualHeight - newRect.Height - margin);

        clipArea.RectArea = newRect;
    }


    private Cursor GetCursor(CompassOrientation orientation)
    {
        return orientation switch
        {
            CompassOrientation.North or CompassOrientation.South => Cursors.SizeNS,
            CompassOrientation.East or CompassOrientation.West => Cursors.SizeWE,
            CompassOrientation.NorthWest or CompassOrientation.SouthEast => Cursors.SizeNWSE,
            CompassOrientation.NorthEast or CompassOrientation.SouthWest => Cursors.SizeNESW,
            _ => throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null)
        };
    }

    protected override Visual GetVisualChild(int index)
    {
        return _visualCollection[index];
    }

    protected override int VisualChildrenCount => _visualCollection.Count;

    protected override Size ArrangeOverride(Size finalSize)
    {
        const double borderOffset = 1;

        _border.Arrange(new Rect(-borderOffset, -borderOffset, AdornedElement.DesiredSize.Width + 2 * borderOffset,
            AdornedElement.DesiredSize.Height + 2 * borderOffset));

        foreach (var thumb in _thumbs.Values)
        {
            thumb.Arrange(GetRect(thumb.Orientation, AdornedElement.DesiredSize));
        }

        // _outlinedTextBlock.Text = ((ClipArea)_outlinedTextBlock.DataContext).Name;
        // _outlinedTextBlock.Measure(finalSize);
        // _outlinedTextBlock.Arrange(new Rect(0, 34, 1000000, _outlinedTextBlock.DesiredSize.Height));

        _outlinedTextBox.Measure(finalSize);
        _outlinedTextBox.Arrange(new Rect(0, -24, finalSize.Width, _outlinedTextBox.DesiredSize.Height));

        return base.ArrangeOverride(finalSize);
    }

    private Rect GetRect(CompassOrientation orientation, Size parentSize)
    {
        Rect rect = new(0, 0, _thumbSize, _thumbSize);
        switch (orientation)
        {
            case CompassOrientation.North:
                rect.X = parentSize.Width / 2;
                rect.Y = 0;
                break;
            case CompassOrientation.NorthEast:
                rect.X = parentSize.Width;
                rect.Y = 0;
                break;
            case CompassOrientation.East:
                rect.X = parentSize.Width;
                rect.Y = parentSize.Height / 2;
                break;
            case CompassOrientation.SouthEast:
                rect.X = parentSize.Width;
                rect.Y = parentSize.Height;
                break;
            case CompassOrientation.South:
                rect.X = parentSize.Width / 2;
                rect.Y = parentSize.Height;
                break;
            case CompassOrientation.SouthWest:
                rect.X = 0;
                rect.Y = parentSize.Height;
                break;
            case CompassOrientation.West:
                rect.X = 0;
                rect.Y = parentSize.Height / 2;
                break;
            case CompassOrientation.NorthWest:
                rect.X = 0;
                rect.Y = 0;
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(orientation), orientation, null);
        }


        return Rect.Offset(rect, _thumbOffset);
    }


    private void OnDragDelta(object sender, DragDeltaEventArgs e)
    {
        var delta = new Vector(e.HorizontalChange, e.VerticalChange);
        Resize(((DirectionalThumb)sender).Orientation, delta);
    }

    private void Resize(CompassOrientation compassOrientation, Vector delta)
    {
        var clipArea = ((ContentPresenter)AdornedElement).Content as ClipArea;


        Debug.Assert(clipArea != null, nameof(clipArea) + " != null");

        var newWidth = compassOrientation switch
        {
            CompassOrientation.NorthWest or CompassOrientation.West or CompassOrientation.SouthWest =>
                clipArea.RectArea.Width - delta.X,
            CompassOrientation.NorthEast or CompassOrientation.East or CompassOrientation.SouthEast =>
                clipArea.RectArea.Width + delta.X,
            _ => clipArea.RectArea.Width
        };

        var newHeight = compassOrientation switch
        {
            CompassOrientation.NorthWest or CompassOrientation.North or CompassOrientation.NorthEast =>
                clipArea.RectArea.Height - delta.Y,
            CompassOrientation.SouthWest or CompassOrientation.South or CompassOrientation.SouthEast =>
                clipArea.RectArea.Height + delta.Y,
            _ => clipArea.RectArea.Height
        };


        newWidth = Math.Max(newWidth, 20);
        newHeight = Math.Max(newHeight, 20);


        var newX = clipArea.RectArea.Left;

        if (Math.Abs(newWidth - clipArea.RectArea.Width) > Tolerance)
        {
            newX = compassOrientation switch
            {
                CompassOrientation.SouthWest or CompassOrientation.West or CompassOrientation.NorthWest =>
                    clipArea.RectArea.Left + delta.X,
                _ => clipArea.RectArea.Left
            };
        }

        var newY = clipArea.RectArea.Top;

        if (Math.Abs(newHeight - clipArea.RectArea.Height) > Tolerance)
        {
            newY = compassOrientation switch
            {
                CompassOrientation.NorthWest or CompassOrientation.North or CompassOrientation.NorthEast =>
                    clipArea.RectArea.Top + delta.Y,
                _ => clipArea.RectArea.Top
            };
        }
        //
        // if (newWidth < 20)
        // {
        //     newX = clipArea.RectArea.Left;
        //     newWidth = 20;
        // }

        var newSize = new Rect(
            newX,
            newY,
            newWidth,
            newHeight);

        
        // TODO: Refactor this to prevent extreme and out of bounds sizing
        
        
        
        clipArea.RectArea = newSize;
    }

    private const double Tolerance = 0.0001;
}