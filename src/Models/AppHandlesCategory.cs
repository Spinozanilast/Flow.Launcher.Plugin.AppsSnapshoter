using Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

namespace Flow.Launcher.Plugin.SnapshotApps.Models;

public record AppHandlesCategory (string CategoryName, string[] Apps, IHandlesExplorer HandlesExplorer);