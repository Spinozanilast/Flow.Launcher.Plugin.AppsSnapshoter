using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.SnapshotApps.Models;
using static Flow.Launcher.Plugin.SnapshotApps.HandlesViewers.FileNameExtractFromHandles;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public class HandlesViewer
{
    public static readonly List<AppHandlesCategory> AppHandlesCategories = new()
    {
        new(
            CategoryName: "Explorer",
            Apps: new[] { "explorer" },
            HandlesExplorer: new ExplorerHandlesExplorer()
        ),
        new(
            CategoryName: "VideoPlayers",
            Apps: new[]
            {
                "Video.UI", "vlc", "wmplayer", "mpc-hc",
                "potplayer", "kmplayer", "bsplayer"
            },
            HandlesExplorer: new VideoHandlesExplorer()
        )
    };

    private static readonly string HandleCliFileName = "handle.exe";
    private readonly ProcessStartInfo _handlesStartInfo;

    public HandlesViewer(string pluginDirectory)
    {
        var handlesCliFilename = Path.Combine(pluginDirectory, HandleCliFileName);

        if (!File.Exists(handlesCliFilename))
        {
            throw new Exception(handlesCliFilename + " doesnt found");
        }

        _handlesStartInfo = new ProcessStartInfo
        {
            FileName = handlesCliFilename,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    public async Task<List<string>> GetOpenedPaths(int processId, IHandlesExplorer handlesExplorer, string windowText)
    {
        if (handlesExplorer.GetType() == typeof(ExplorerHandlesExplorer) && string.IsNullOrEmpty(windowText))
            return handlesExplorer.GetPathsByHandles(
                pathsFromHandles: null,
                windowText);

        if (handlesExplorer.GetType() == typeof(VideoHandlesExplorer))
        {
            var videoHandles = await FindConcreteHandlesByLineEndAsync(processId);
            return handlesExplorer.GetPathsByHandles(videoHandles, windowText);
        }

        var appHandles = await FindConcreteHandlesByLineEndAsync(processId, windowText);

        return handlesExplorer.GetPathsByHandles(
            appHandles,
            windowText);
    }

    public bool TryGetHandlesExplorer(string moduleName, out IHandlesExplorer handlesExplorer)
    {
        handlesExplorer = null;

        foreach (var appHandlesCategory in AppHandlesCategories)
        {
            if (appHandlesCategory.Apps.Any(appExecutable =>
                    moduleName.Contains(appExecutable, StringComparison.OrdinalIgnoreCase)))
            {
                handlesExplorer = appHandlesCategory.HandlesExplorer;
                return true;
            }
        }

        return false;
    }

    private async Task<HashSet<string>> FindConcreteHandlesByLineEndAsync(int processId, string windowText = "")
    {
        ChangeHandlesArgs($"-p {processId} {windowText} -nobanner");
        var process = Process.Start(_handlesStartInfo);

        ArgumentNullException.ThrowIfNull(process, nameof(process));
        
        var paths = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        while (!process.StandardOutput.EndOfStream)
        {
            var line = await process.StandardOutput.ReadLineAsync();
            
            if (!string.IsNullOrEmpty(line) && TryExtractFilenameFromHandleOutput(line, out var path)) 
            {
                paths.Add(path);
            }
        }
        process.Close();
        return paths;
    }

    private void ChangeHandlesArgs(string args) => _handlesStartInfo.Arguments = args;
}