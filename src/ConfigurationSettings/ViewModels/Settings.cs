using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;

namespace Flow.Launcher.Plugin.AppsSnapshoter.ConfigurationSettings;

public class Settings : INotifyPropertyChanged
{
    private bool _allowAppDuplicatesExist = true;

    public bool AllowAppDuplicatesExist
    {
        get => _allowAppDuplicatesExist;
        set
        {
            if (_allowAppDuplicatesExist != value)
            {
                _allowAppDuplicatesExist = value;
                OnPropertyChanged(nameof(AllowAppDuplicatesExist));
            }
        }
    }

    public ObservableCollection<string> DirectoriesToExclude { get; set; } = new();

    public ObservableCollection<PathSwapEntry> PathsToSwapOnAdd { get; set; } = new();

    private HashSet<string> _blockedApps { get; set; } = new();
    
    public HashSet<string> BlockedApps
    {
        get => _blockedApps;
        set
        {
            if (_blockedApps != value)
            {
                _blockedApps = value;
                OnPropertyChanged(nameof(BlockedApps));
            }
        }
    }
    
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}