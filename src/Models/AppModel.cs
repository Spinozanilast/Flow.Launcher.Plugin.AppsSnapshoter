using ProtoBuf;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Models;

[ProtoContract]
public class AppModel
{
    [ProtoMember(1)] public string AppModuleName { get; set; } = string.Empty;
    [ProtoMember(2)] public string ExecutionFilePath { get; set; }
    [ProtoMember(3)] public string IconPath { get; set; }
}