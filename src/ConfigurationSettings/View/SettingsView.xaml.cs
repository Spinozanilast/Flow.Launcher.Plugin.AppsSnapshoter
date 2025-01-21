using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Input;
using Flow.Launcher.Plugin.AppsSnapshoter.ConfigurationSettings.Commands;
using JetBrains.Annotations;
using OpenFileDialog = Microsoft.Win32.OpenFileDialog;

namespace Flow.Launcher.Plugin.AppsSnapshoter.ConfigurationSettings.View;

public partial class SettingsView
{
    private readonly PluginInitContext _context;
    private readonly Settings _settings;

    public ICommand AddDirectoryCommand { get; }
    public ICommand RemoveDirectoryCommand { get; }
    public ICommand AddPathSwapCommand { get; }
    public ICommand RemovePathCommand { get; }

    public SettingsView(PluginInitContext context, Settings settings)
    {
        _context = context;
        DataContext = _settings = settings;

        InitializeComponent();

        AddDirectoryCommand = new RelayCommand(AddDirectory);
        RemoveDirectoryCommand = new RelayCommand<string>(RemoveDirectory);
        AddPathSwapCommand = new RelayCommand(AddPathSwap);
        RemovePathCommand = new RelayCommand<string>(RemovePathSwap);
    }

    private static bool TryGetNotEmptyPathFromFileDialog(string title, out string result)
    {
        result = null;

        var dialog = new OpenFileDialog
        {
            ValidateNames = false,
            CheckFileExists = false,
            CheckPathExists = true,
            FileName = "Folder Selection.",
            Title = title
        };

        if (dialog.ShowDialog() == false || dialog.FileName.Length == 0) return false;

        result = dialog.FileName;
        return true;
    }

    private static bool TryGetNotEmptyPathFromFolderDialog(string description, out string result)
    {
        result = null;

        var dialog = new FolderBrowserDialog
        {
            Description = description,
            UseDescriptionForTitle = true,
        };

        if (dialog.ShowDialog() == DialogResult.OK || dialog.SelectedPath.Length == 0) return false;

        result = dialog.SelectedPath;
        return true;
    }

    private void Save(object sender = null, RoutedEventArgs e = null) =>
        _context.API.SaveSettingJsonStorage<Settings>();

    private void AddDirectory()
    {
        if (!TryGetNotEmptyPathFromFolderDialog("Select a folder to exclude", out var selectedPath)) return;

        if (!_settings.DirectoriesToExclude.Contains(selectedPath))
        {
            _settings.DirectoriesToExclude.Add(selectedPath);
            Save();
        }
        else
        {
            throw new Exception("Directory is already excluded");
        }
    }

    private void RemoveDirectory(string directory)
    {
        if (_settings.DirectoriesToExclude.Remove(directory))
        {
            Save();
        }
    }

    private void AddPathSwap()
    {
        if (!TryGetNotEmptyPathFromFileDialog("Select the old path", out var oldPath) ||
            !TryGetNotEmptyPathFromFileDialog("Select the new path", out var newPath)) return;


        if (_settings.PathsToSwapOnAdd.All(entry => entry.OriginalPath != oldPath))
        {
            _settings.PathsToSwapOnAdd.Add(new PathSwapEntry { OriginalPath = oldPath, ReplacementPath = newPath });
            Save();
        }
        else
        {
            throw new Exception("Some error occured while adding paths");
        }
    }

    private void RemovePathSwap(string oldPath)
    {
        var entryToRemove = _settings.PathsToSwapOnAdd.FirstOrDefault(entry => entry.OriginalPath == oldPath);
        if (entryToRemove is not null)
        {
            _settings.PathsToSwapOnAdd.Remove(entryToRemove);
            Save();
        }
    }
}