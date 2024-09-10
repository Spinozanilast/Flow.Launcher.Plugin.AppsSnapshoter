using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps;

public class OpenedAppsService
{
    private const int DefaultModelsCapacity = 5;
    private const string DefaultIconsDirectoryName = "Icons";
    private const string DefaultIconsImagesExtension = ".png";

    private readonly string[] ExcludedFullPathDirectories =
    {
        "C:\\WINDOWS\\SystemApps",
        "C:\\WINDOWS\\system32",
    };

    private readonly string _pluginDirectory;
    private readonly string _iconsDirectory;

    public OpenedAppsService(string pluginDirectory)
    {
        _pluginDirectory = pluginDirectory ?? Directory.GetCurrentDirectory();
        _iconsDirectory = Path.Combine(_pluginDirectory, DefaultIconsDirectoryName);

        if (!Directory.Exists(_iconsDirectory))
        {
            Directory.CreateDirectory(_iconsDirectory);
        }

        WriteOpenedAppsModels();
    }

    public List<AppModel> AppModels { get; } = new List<AppModel>(DefaultModelsCapacity);

    public void WriteAppsIconsToModels()
    {
        foreach (var model in AppModels)
        {
            var iconFilePath = Path.Combine(_iconsDirectory, model.AppModuleName) + DefaultIconsImagesExtension;
            if (File.Exists(iconFilePath))
            {
                model.IconPath = iconFilePath;
                continue;
            }

            var icon = Icon.ExtractAssociatedIcon(model.ExecutionFilePath);
            var bitmapIcon = icon.ToBitmap();
            bitmapIcon.Save(iconFilePath, ImageFormat.Png);
        }
    }

    private void WriteOpenedAppsModels()
    {
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            var windowTitle = process.MainWindowTitle;
            if (windowTitle.Length > 0 && process.MainModule is not null &&
                !IsInExcludedDirectory(process.MainModule.FileName))
            {
                var mainModule = process.MainModule;
                AppModels.Add(new AppModel
                {
                    AppModuleName = mainModule.ModuleName[..^4],
                    ExecutionFilePath = mainModule.FileName
                });
            }
        }
    }

    private bool IsInExcludedDirectory(string filename)
    {
        return ExcludedFullPathDirectories.Any(filename.StartsWith);
    }
}