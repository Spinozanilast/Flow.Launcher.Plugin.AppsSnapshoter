using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public class ExplorerHandlesExplorer : IHandlesExplorer
{
    public List<string> GetPathsByHandles(HashSet<string> handles, Func<string, string> filenameExtractor)
    {
        var openedExplorerPaths = new List<string>();
        foreach (var handle in handles)
        {
            if (handle.Contains("explorer.exe"))
            {
                var path = filenameExtractor(handle);
                openedExplorerPaths.Add(path);
            }
        }

        return openedExplorerPaths;
    }
}