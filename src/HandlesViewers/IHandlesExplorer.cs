using System;
using System.Collections.Generic;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

/// <summary>
/// Interface for exploring handles and extracting paths.
/// </summary>
public interface IHandlesExplorer
{
    /// <summary>
    /// Gets the paths associated with the provided handles.
    /// </summary>
    /// <param name="handles">A set of handles to explore.</param>
    /// <param name="filenameExtractor">A function to extract filenames from handles.</param>
    /// <returns>A list of paths associated with the provided handles.</returns>
    List<string> GetPathsByHandles(HashSet<string> handles, Func<string, string> filenameExtractor);
}