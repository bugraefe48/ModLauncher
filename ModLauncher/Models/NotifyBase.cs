using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace ModLauncher;

// Bir özellik değişince arayüze "güncelle" diye haber veren temel sınıf
public abstract class NotifyBase : INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;

    protected void OnPropertyChanged([CallerMemberName] string? name = null) =>
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
}