using System.Collections.Generic;
using ProtoBuf;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Models;

[ProtoContract]
public class Snapshot
{
    [ProtoMember(1)] public string SnapshotName { get; set; }
    [ProtoMember(2)] public List<AppModel> AppModelsIncluded { get; set; }
    [ProtoMember(3)] public string IcoPath { get; set; } = string.Empty;
}