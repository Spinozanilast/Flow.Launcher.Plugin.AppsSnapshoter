using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public class ExplorerHandlesExplorer : IHandlesExplorer
{

    
    public List<string> GetPathsByHandles(HashSet<string> handles, Func<string, string> filenameExtractor,
        string windowText = "")
    {
        if (string.IsNullOrEmpty(windowText))
        {
            return MarshalingExplorerOpenedPaths();
        }

        var openedExplorerPaths = new List<string>();
        foreach (var handle in handles)
        {
            var isItCurrentlyOpenedPath = handle.EndsWith(windowText);

            if (isItCurrentlyOpenedPath)
            {
                var path = filenameExtractor(handle);
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

                if (ie.FullName == "explorer.exe")
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