using System;
using System.IO;
using Flow.Launcher.Plugin.SnapshotApps.Models;
using ProtoBuf;

namespace Flow.Launcher.Plugin.SnapshotApps;

public class SnapshotBinaryFormatter
{
    public static void SerializeSnapshot(FileStream stream, Snapshot snapshot)
    {
        using (stream)
        {
            Serializer.Serialize(stream, snapshot);
        }
    }

    public static Snapshot DeserializeSnapshot(FileStream stream)
    {
        Snapshot snapshot;
        using (stream)
        {
            snapshot = Serializer.Deserialize<Snapshot>(stream);
        }

        if (snapshot is null)
        {
            throw new NullReferenceException(
                "Snapshot file was empty or not correct format.");
        }

        return snapshot;
    }
}