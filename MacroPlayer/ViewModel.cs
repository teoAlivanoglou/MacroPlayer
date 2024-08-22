using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace MacroPlayer;

public class ViewModel : INotifyPropertyChanged
{
    private bool _overlayOpen;
    private ClipArea _selectedItem;
    private static ViewModel? _instance;
    public ObservableCollection<ClipArea> clipAreas { get; private set; } = new();

    public ClipArea SelectedItem
    {
        get => _selectedItem;
        set => SetField(ref _selectedItem, value);
    }

    public bool OverlayOpen
    {
        get => _overlayOpen;
        set => SetField(ref _overlayOpen, value);
    }

    public static ViewModel Instance => _instance ??= new ViewModel();

    public ViewModel()
    {
        clipAreas.Add(new ClipArea(125, 43, 400, 200, "My Clip Area"));
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