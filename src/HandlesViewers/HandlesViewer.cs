using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public class HandlesViewer
{
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

    // public static List<string> GetOpenedExplorerPaths()
    // {
    //     var openedExplorerPaths = new List<string>();
    //     var shellType = Type.GetTypeFromProgID("Shell.Application");
    //     dynamic shell = Activator.CreateInstance(shellType);
    //
    //     try
    //     {
    //         var windows = shell.Windows();
    //         for (int i = 0; i < windows.Count; i++)
    //         {
    //             dynamic ie = windows.Item(i);
    //             if (ie == null) continue;
    //
    //             if (ie.FullName == ExplorerPath)
    //             {
    //                 openedExplorerPaths.Add(ie.LocationURL);
    //             }
    //         }
    //
    //         return openedExplorerPaths;
    //     }
    //     finally
    //     {
    //         Marshal.FinalReleaseComObject(shell);
    //     }
    // }

    public async Task<string[]> GetOpenedPaths(int processId, IHandleExplorer handleExplorer)
    {
        var handles = await FindConcreteHandlesByLineEndAsync(processId);
        return handleExplorer.GetPathsByHandles(handles);
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
}