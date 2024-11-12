using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.AppsSnapshoter.HandlesViewers;
using Flow.Launcher.Plugin.AppsSnapshoter.Models;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Services;

public class OpenedAppsService
{
    private const int DefaultModelsCapacity = 5;
    private const string DefaultIconsDirectoryName = "Icons";
    private const string DefaultAppIconFileName = "application-default.png";
    private const string DefaultIconsImagesExtension = ".png";

    #region limitations

    private readonly string[] _excludedDirectories =
    {
        "WindowsApps",
        "SystemApps",
        "system32"
    };

    private readonly string[] _includedProcesses = { "Video.UI", "explorer" };

    #endregion

    private readonly string _pluginDirectory;
    private readonly string _iconsDirectory;

    private readonly HandlesViewer _handlesViewer;

    private PluginInitContext _context;

    public static async Task<OpenedAppsService> CreateAsync(string pluginDirectory, PluginInitContext context)
    {
        var service = new OpenedAppsService(pluginDirectory, context);
        await service.WriteOpenedAppsModels();
        return service;
    }

    private OpenedAppsService(string pluginDirectory, PluginInitContext context)
    {
        _context = context;
        _pluginDirectory = pluginDirectory ?? Directory.GetCurrentDirectory();
        _iconsDirectory = Path.Combine(_pluginDirectory, DefaultIconsDirectoryName);
        Path.Combine(_pluginDirectory, DefaultAppIconFileName);
        if (!Directory.Exists(_iconsDirectory))
        {
            Directory.CreateDirectory(_iconsDirectory);
        }

        _handlesViewer = new HandlesViewer(_pluginDirectory);
    }

    public List<AppModel> AppModels { get; } = new(DefaultModelsCapacity);

    public void WriteAppsIconsToModels()
    {
        foreach (var model in AppModels)
        {
            if (!string.IsNullOrEmpty(model.IconPath)) continue;
            SetIconToAppModel(model);
        }
    }

    private void SetIconToAppModel(AppModel model, string executionFilePath = null)
    {
        var iconFilePath = Path.Combine(_iconsDirectory, model.AppModuleName) + DefaultIconsImagesExtension;

        if (File.Exists(iconFilePath))
        {
            model.IconPath = iconFilePath;
            return;
        }

        var icon = Icon.ExtractAssociatedIcon(executionFilePath ?? model.ExecutionFilePath);
        var bitmapIcon = icon?.ToBitmap();
        bitmapIcon?.Save(iconFilePath, ImageFormat.Png);
        model.IconPath = iconFilePath;
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

    private async Task AddHandlesExplorerPaths(Process process, IHandlesExplorer handlesExplorer, string moduleName,
        string windowText = "")
    {
        var paths = await _handlesViewer.GetOpenedPaths(process.Id, handlesExplorer, windowText);
        foreach (var path in paths)
        {
            if (path.Length > 0)
            {
                var model = new AppModel()
                {
                    AppModuleName = moduleName[..^4],
                    ExecutionFilePath = path
                };

                SetIconToAppModel(model, process.MainModule?.FileName);
                AppModels.Add(model);
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

    private bool IsValidProcess(Process process)
    {
        try
        {
            if (IsInIncludedApps(process.ProcessName))
            {
                return true;
            }

            var mainModule = process.MainModule;
            return mainModule is not null && process.MainWindowTitle.Length > 0 &&
                   !IsInExcludedDirectory(mainModule.FileName);
        }
        catch (Win32Exception)
        {
            return false;
        }
    }

    private bool IsInExcludedDirectory(string filename)
    {
        return _excludedDirectories.Any(dir => filename.Contains(dir));
    }

    private bool IsInIncludedApps(string processName)
    {
        return _includedProcesses.Any(app => processName.Equals(app, StringComparison.OrdinalIgnoreCase));
    }
}