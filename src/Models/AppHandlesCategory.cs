using Flow.Launcher.Plugin.AppsSnapshoter.HandlesViewers;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Models;

public record AppHandlesCategory (string CategoryName, string[] Apps, IHandlesExplorer HandlesExplorer);