using System.Collections.Generic;
using ProtoBuf;

namespace Flow.Launcher.Plugin.FolderMark.Models;

[ProtoContract]
public class Snapshot
{
    [ProtoMember(1)] public string SnapshotName { get; set; }
    [ProtoMember(2)] public List<AppModel> AppModelsIncluded { get; set; }
}