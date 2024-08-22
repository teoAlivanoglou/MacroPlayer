using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;

namespace MacroPlayer;

public class ClipArea : INotifyPropertyChanged
{
    private Rect _rectArea;
    private string _name = string.Empty;
    private Color _fillColor = Color.FromArgb( 150, 80, 56, 122);

    public Rect RectArea
    {
        get => _rectArea;
        set => SetField(ref _rectArea, value);
    }

    public string Name
    {
        get => _name;
        set => SetField(ref _name, value);
    }

    public Color FillColor
    {
        get => _fillColor;
        set => SetField(ref _fillColor, value);
    }


    public ClipArea()
    {
        RectArea = new Rect(0, 0, 0, 0);
    }

    public ClipArea(Rect rectArea, string name)
    {
        _rectArea = rectArea;
        _name = name;
    }

    public ClipArea(double x, double y, double width, double height, string name)
    {
        _rectArea = new Rect(x, y, width, height);
        _name = name;
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}