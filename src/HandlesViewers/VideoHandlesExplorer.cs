using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public class VideoHandlesExplorer : IHandlesExplorer
{
    private readonly string[] _videoExtensions = new[]
    {
        ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mpg", ".mpeg", ".m4v"
    };

    public List<string> GetPathsByHandles(HashSet<string> handles, Func<string, string> filenameExtractor)
    {
        if (handles.Count == 1 && handles.First() == "No matching handles found.")
        {
            throw new ArgumentException("There are no any process handles.", nameof(handles));
        }

        var paths = new List<string>();

        foreach (var handle in handles)
        {
            if (IsVideoFile(handle))
            {
                var path = filenameExtractor(handle);
                paths.Add(path);
            }
        }

        return paths;
    }

    private bool IsVideoFile(string handleInfo) =>
        _videoExtensions.Any(extension => handleInfo.EndsWith(extension));
}