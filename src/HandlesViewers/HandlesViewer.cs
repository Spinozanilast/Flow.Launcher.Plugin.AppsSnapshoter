using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public class HandlesViewer
{
    public static readonly List<AppHandlesCategory> AppHandlesCategories = new()
    {
        new(
            CategoryName: "Explorer",
            Apps: new[] { "explorer.exe" },
            HandlesExplorer: new ExplorerHandlesExplorer()
        ),
        new(
            CategoryName: "VideoPlayers",
            Apps: new[]
            {
                "Video.UI.exe", "vlc.exe", "wmplayer.exe", "mpc-hc.exe",
                "potplayer.exe", "kmplayer.exe", "bsplayer.exe"
            },
            HandlesExplorer: new VideoHandlesExplorer()
        )
    };

    private static string HandlesCliFilename = "handle.exe";
    private readonly ProcessStartInfo _handlesStartInfo;

    public HandlesViewer()
    {
        _handlesStartInfo = new ProcessStartInfo
        {
            FileName = HandlesCliFilename,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    public async Task<List<string>> GetOpenedPaths(int processId, IHandlesExplorer handlesExplorer)
    {
        var handles = await FindConcreteHandlesByLineEndAsync(processId);
        return handlesExplorer.GetPathsByHandles(handles, ExtractFilenameFromHandleOutput);
    }

    public bool TryGetHandlesExplorer(string moduleName, out IHandlesExplorer handlesExplorer)
    {
        handlesExplorer = null;
        
        foreach (var appHandlesCategory in AppHandlesCategories)
        {
            if (appHandlesCategory.Apps.Any(appExecutable => moduleName == appExecutable))
            {
                handlesExplorer = appHandlesCategory.HandlesExplorer;
                return true;
            }
        }

        return false;
    }

    private async Task<HashSet<string>> FindConcreteHandlesByLineEndAsync(int processId)
    {
        ChangeHandlesArgs($"-p {processId}");
        var process = Process.Start(_handlesStartInfo);

        ArgumentNullException.ThrowIfNull(process, nameof(process));
        var handlesLines = new HashSet<string>();
        while (!process.StandardOutput.EndOfStream)
        {
            handlesLines.Add(await process.StandardOutput.ReadLineAsync());
        }

        return handlesLines;
    }

    private void ChangeHandlesArgs(string args) => _handlesStartInfo.Arguments = args;

    private string ExtractFilenameFromHandleOutput(string handleOutput)
    {
        var filenameIndex = handleOutput.IndexOf(":\\", StringComparison.Ordinal) - 1;
        return handleOutput.Substring(filenameIndex);
    }
}