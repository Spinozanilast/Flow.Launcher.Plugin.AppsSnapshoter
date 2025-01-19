using System.IO;
using Flow.Launcher.Plugin.AppsSnapshoter.Models;
using JetBrains.Annotations;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Services;

public class FileDialogService
{
    private readonly IconService _iconService;

    public FileDialogService(IconService iconService)
    {
        _iconService = iconService;
    }

    [CanBeNull]
    public AppModel WriteAppModelFromFileDialog(string title)
    {
        var openFileDialog = new OpenFileDialog
        {
            Filter = "Executable files (*.exe)|*.exe",
            Title = title,
        };

        if (openFileDialog.ShowDialog() != true)
        {
            return null;
        }

        var newExecutablePath = openFileDialog.FileName;
        var appModel = new AppModel
        {
            AppModuleName = Path.GetFileNameWithoutExtension(newExecutablePath),
            ExecutionFilePath = newExecutablePath,
        };
        _iconService.SetIconForApp(appModel, newExecutablePath);

        return appModel;
    }
}