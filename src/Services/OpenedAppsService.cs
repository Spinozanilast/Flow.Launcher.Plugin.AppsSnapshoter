using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.AppsSnapshoter.ConfigurationSettings;
using Flow.Launcher.Plugin.AppsSnapshoter.HandlesViewers;
using Flow.Launcher.Plugin.AppsSnapshoter.Models;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Services;

public class OpenedAppsService
{
    private const int DefaultModelsCapacity = 5;

    private Settings _pluginSettings;

    #region Limitations

    private readonly string[] _excludedDirectories =
    {
        "WindowsApps",
        "SystemApps",
        "system32"
    };

    private readonly string[] _includedProcesses = { "Video.UI", "explorer" };

    #endregion

    private readonly IconService _iconService;

    private readonly string _pluginDirectory;
    private readonly string _iconsDirectory;


    private readonly HandlesViewer _handlesViewer;
    private readonly PluginInitContext _context;

    public static async ValueTask<OpenedAppsService> CreateAsync(string pluginDirectory, PluginInitContext context,
        Settings settings)
    {
        var service = new OpenedAppsService(pluginDirectory, context, settings);
        await service.WriteOpenedAppsModels();
        return service;
    }

    private OpenedAppsService(string pluginDirectory, PluginInitContext context, Settings settings)
    {
        _context = context;
        _pluginSettings = settings;
        _pluginDirectory = pluginDirectory ?? Directory.GetCurrentDirectory();
        _iconService = new IconService(_pluginDirectory);
        _handlesViewer = new HandlesViewer(_pluginDirectory);
    }

    public List<AppModel> AppModels { get; } = new(DefaultModelsCapacity);

    public void WriteAppsIconsToModels()
    {
        foreach (var model in AppModels)
        {
            if (!string.IsNullOrEmpty(model.IconPath)) continue;
            _iconService.SetIconForApp(model);
        }
    }

    private async Task WriteOpenedAppsModels()
    {
        foreach (var process in GetValidProcesses())
        {
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

    private IEnumerable<Process> GetValidProcesses()
    {
        return Process.GetProcesses().Where(IsValidProcess);
    }

    private async Task AddHandlesExplorerPaths(Process process, IHandlesExplorer handlesExplorer, string moduleName,
        string windowText = "")
    {
        var paths = await _handlesViewer.GetOpenedPaths(process.Id, handlesExplorer, windowText);
        foreach (var path in paths)
        {
            if (path.Length > 0)
            {
                var model = new AppModel
                {
                    AppModuleName = moduleName[..^4],
                    ExecutionFilePath = path
                };

                _iconService.SetIconForApp(model, process.MainModule?.FileName);
                AppModels.Add(model);
            }
        }
    }

    private void AddDefaultPath(Process process, string moduleName)
    {
        var executionFilePath = process.MainModule?.FileName;

        if (executionFilePath is null) return;

        foreach (var pathSwap in _pluginSettings.PathsToSwapOnAdd)
        {
            if (pathSwap.OriginalPath.Equals(executionFilePath, StringComparison.OrdinalIgnoreCase))
            {
                executionFilePath = pathSwap.ReplacementPath;
                break;
            }
        }

        var appModuleName = moduleName[..^4];
        var newModel = new AppModel
        {
            AppModuleName = appModuleName,
            ExecutionFilePath = executionFilePath
        };

        if (!_pluginSettings.BlockedApps.Contains(appModuleName) && (_pluginSettings.AllowAppDuplicatesExist ||
                                                                     AppModels.All(m =>
                                                                         m.ExecutionFilePath !=
                                                                         newModel.ExecutionFilePath)))
        {
            AppModels.Add(newModel);
        }
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
        return _excludedDirectories.Concat(_pluginSettings.DirectoriesToExclude)
            .Any(dir => filename.Contains(dir, StringComparison.OrdinalIgnoreCase));
    }

    private bool IsInIncludedApps(string processName)
    {
        return _includedProcesses.Any(app => processName.Equals(app, StringComparison.OrdinalIgnoreCase));
    }
}