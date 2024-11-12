using System.Collections.Generic;

namespace Flow.Launcher.Plugin.AppsSnapshoter.HandlesViewers;

/// <summary>
/// Interface for exploring handles and extracting paths.
/// </summary>
public interface IHandlesExplorer
{
    /// <summary>
    /// Gets the paths associated with the provided handles.
    /// </summary>
    /// <param name="pathsFromHandles">A set of handles to explore.</param>
    /// <param name="windowText">The Main window title </param>
    /// <returns>A list of paths associated with the provided handles.</returns>
    List<string> GetPathsByHandles(HashSet<string> pathsFromHandles, string windowText = "");
}