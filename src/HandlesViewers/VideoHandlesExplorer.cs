using System;
using System.Collections.Generic;
using System.Linq;

namespace Flow.Launcher.Plugin.AppsSnapshoter.HandlesViewers;

public class VideoHandlesExplorer : IHandlesExplorer
{
    private readonly string[] _videoExtensions = new[]
    {
        ".mp4", ".mkv", ".avi", ".mov", ".wmv", ".flv", ".webm", ".mpg", ".mpeg", ".m4v"
    };

    public List<string> GetPathsByHandles(HashSet<string> pathsFromHandles,
        string windowText = "")
    {
        if (pathsFromHandles.Count == 1 && pathsFromHandles.First() == "No matching handles found.")
        {
            throw new ArgumentException("There are no any process handles.", nameof(pathsFromHandles));
        }

        var paths = new List<string>();

        foreach (var path in pathsFromHandles)
        {
            if (IsVideoFile(path))
            {
                paths.Add(path);
            }
        }

        return paths;
    }

    private bool IsVideoFile(string handleInfo) =>
        _videoExtensions.Any(extension => handleInfo.EndsWith(extension));
}