using System.ComponentModel;

namespace Flow.Launcher.Plugin.AppsSnapshoter.ConfigurationSettings;

public class PathSwapEntry : INotifyPropertyChanged
{
    private string _originalPath;

    public string OriginalPath
    {
        get => _originalPath;
        set
        {
            if (_originalPath == value) return;

            _originalPath = value;
            OnPropertyChanged(nameof(OriginalPath));
        }
    }

    private string _replacementPath;

    public string ReplacementPath
    {
        get => _replacementPath;
        set
        {
            if (_replacementPath == value) return;

            _replacementPath = value;
            OnPropertyChanged(nameof(ReplacementPath));
        }
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}