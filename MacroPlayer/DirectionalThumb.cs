using System.Windows;
using System.Windows.Controls.Primitives;

namespace MacroPlayer;

public class DirectionalThumb : Thumb
{
    public CompassOrientation Orientation
    {
        get
        {
            return (CompassOrientation)GetValue(OrientationProperty);
        }
        set
        {
            SetValue(OrientationProperty, value);
        }
    }
    
    public static readonly DependencyProperty
        OrientationProperty = DependencyProperty.Register("Orientation",
            typeof(string), typeof(DirectionalThumb), new PropertyMetadata(""));
}