using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps.Services;

public class OpenedAppsService
{
    private const int DefaultModelsCapacity = 5;
    private const string DefaultIconsDirectoryName = "Icons";
    private const string DefaultAppIconFileName = "application-default.png";
    private const string DefaultIconsImagesExtension = ".png";

    private readonly string[] _excludedFullPathDirectories =
    {
        "WindowsApps",
        "SystemApps",
    };
    
    private readonly string _pluginDirectory;
    private readonly string _iconsDirectory;

    private readonly HandlesViewer _handlesViewer;
    
    public static async Task<OpenedAppsService> CreateAsync(string pluginDirectory)
    {
        var service = new OpenedAppsService(pluginDirectory);
        await service.WriteOpenedAppsModels();
        return service;
    }

    private OpenedAppsService(string pluginDirectory)
    {
        _pluginDirectory = pluginDirectory ?? Directory.GetCurrentDirectory();
        _iconsDirectory = Path.Combine(_pluginDirectory, DefaultIconsDirectoryName);
        Path.Combine(_pluginDirectory, DefaultAppIconFileName);
        if (!Directory.Exists(_iconsDirectory))
        {
            Directory.CreateDirectory(_iconsDirectory);
        }
        
        _handlesViewer = new HandlesViewer(_pluginDirectory);
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
            var bitmapIcon = icon?.ToBitmap();
            bitmapIcon?.Save(iconFilePath, ImageFormat.Png);
        }
    }

    private async Task WriteOpenedAppsModels()
    {
        var processes = Process.GetProcesses();
        foreach (var process in processes)
        {
            if (!IsValidProcess(process))
                continue;

            var mainModule = process.MainModule;
            var moduleName = mainModule?.ModuleName;
            var windowText = process.MainWindowTitle;

            if (_handlesViewer.TryGetHandlesExplorer(moduleName, out IHandlesExplorer handlesExplorer))
            {
                await AddHandlesExplorerPaths(process, handlesExplorer, moduleName, windowText);
            }
            else
            {
                AddDefaultPath(process, moduleName);
            }
        }
    }

    private bool IsValidProcess(Process process)
    {
        return process.MainWindowTitle.Length > 0 && process.MainModule is not null &&
               !IsInExcludedDirectory(process.MainModule.FileName);
    }

    private async Task AddHandlesExplorerPaths(Process process, IHandlesExplorer handlesExplorer, string moduleName, string windowText = "")
    {
        var paths = await _handlesViewer.GetOpenedPaths(process.Id, handlesExplorer, windowText);
        foreach (var path in paths)
        {
            if (path.Length > 0)
            {
                AppModels.Add(new AppModel
                {
                    AppModuleName = moduleName[..^4],
                    ExecutionFilePath = path,
                });
            }
        }
    }

    private void AddDefaultPath(Process process, string moduleName)
    {
        AppModels.Add(new AppModel
        {
            AppModuleName = moduleName[..^4],
            ExecutionFilePath = process.MainModule?.FileName
        });
    }

    private bool IsInExcludedDirectory(string filename)
    {
        return _excludedFullPathDirectories.Any(dir => filename.Contains(dir));
    }
}