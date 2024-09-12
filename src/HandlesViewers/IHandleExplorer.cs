using System.Collections.Generic;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public interface IHandleExplorer
{
    string[] GetPathsByHandles(List<string> handles);
}