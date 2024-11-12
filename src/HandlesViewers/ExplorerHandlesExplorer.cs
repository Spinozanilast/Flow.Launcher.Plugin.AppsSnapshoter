using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Flow.Launcher.Plugin.AppsSnapshoter.HandlesViewers;

public class ExplorerHandlesExplorer : IHandlesExplorer
{
    private const string ExplorerFullFileName = "C:\\WINDOWS\\Explorer.EXE";

    public List<string> GetPathsByHandles(HashSet<string> pathsFromHandles,
        string windowText = "")
    {
        if (string.IsNullOrEmpty(windowText))
        {
            return MarshalingExplorerOpenedPaths();
        }

        var strokes = windowText.Split(' ');
        if (strokes.Length > 1)
        {
            windowText = strokes[0];
        }

        var openedExplorerPaths = new List<string>();
        foreach (var path in pathsFromHandles)
        {
            var isItCurrentlyOpenedPath = path.EndsWith(windowText, StringComparison.OrdinalIgnoreCase);

            if (isItCurrentlyOpenedPath)
            {
                openedExplorerPaths.Add(path);
            }
        }

        return openedExplorerPaths;
    }

    private List<string> MarshalingExplorerOpenedPaths()
    {
        var openedExplorerPaths = new List<string>();
        var shellType = Type.GetTypeFromProgID("Shell.Application");
        ArgumentNullException.ThrowIfNull(shellType);

        dynamic shell = Activator.CreateInstance(shellType);
        ArgumentNullException.ThrowIfNull(shell);

        try
        {
            var windows = shell.Windows();
            for (int i = 0; i < windows.Count; i++)
            {
                dynamic ie = windows.Item(i);
                if (ie == null) continue;

                if (ie.FullName.Equals(ExplorerFullFileName, StringComparison.OrdinalIgnoreCase))
                {
                    openedExplorerPaths.Add(ie.LocationURL);
                }
            }

            return openedExplorerPaths;
        }
        finally
        {
            Marshal.FinalReleaseComObject(shell);
        }
    }
}