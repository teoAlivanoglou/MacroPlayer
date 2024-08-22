using System.Windows;
using System.Windows.Controls.Primitives;

namespace MacroPlayer;

public class DirectionalThumb : Thumb
{
    public CompassOrientation Orientation
    {
        get => (CompassOrientation)GetValue(OrientationProperty);
        init => SetValue(OrientationProperty, value);
    }
    
    public static readonly DependencyProperty
        OrientationProperty = DependencyProperty.Register(nameof(Orientation),
            typeof(CompassOrientation), typeof(DirectionalThumb), new PropertyMetadata(CompassOrientation.North));
}