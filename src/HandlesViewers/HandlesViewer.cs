using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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
        var appHandles = await FindConcreteHandlesByLineEndAsync(processId);
        return handlesExplorer.GetPathsByHandles(
            appHandles,
            filenameExtractor: ExtractFilenameFromHandleOutput,
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
        var handlesLines = new HashSet<string>();
        while (!process.StandardOutput.EndOfStream)
        {
            handlesLines.Add(await process.StandardOutput.ReadLineAsync());
        }

        process.Close();

        return handlesLines;
    }

    private void ChangeHandlesArgs(string args) => _handlesStartInfo.Arguments = args;

    private string ExtractFilenameFromHandleOutput(string handleOutput)
    {
        var filenameIndex = handleOutput.IndexOf(":\\", StringComparison.Ordinal) - 1;
        return handleOutput.Substring(filenameIndex);
    }
}