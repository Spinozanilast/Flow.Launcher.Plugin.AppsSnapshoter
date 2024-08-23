using System;
using System.IO;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps;

using ProtobufSerializer = ProtoBuf.Serializer;

public class SnapshotManager
{
    private const string DefaultSnapshotsDirectoryName = "Snapshots";
    private const string DefaultSerializedFileExtension = ".bin";


    private readonly string _pluginDirectory;
    private readonly string _snapshotsDirectory;

    public SnapshotManager(string pluginDirectory)
    {
        _pluginDirectory = _pluginDirectory ?? pluginDirectory;
        _snapshotsDirectory = Path.Combine(_pluginDirectory, DefaultSnapshotsDirectoryName);

        if (!Directory.Exists(_snapshotsDirectory))
        {
            Directory.CreateDirectory(_snapshotsDirectory);
        }
    }

    public void CreateSnapshot(Snapshot newSnapshot)
    {
        var fileStream = CreateFileForSnapshot(newSnapshot.SnapshotName);
        SnapshotFormatter.SerializeSnapshot(fileStream, newSnapshot);
    }

    public Snapshot GetSnapshot(string snapshotName)
    {
        var fileStream = CreateFileForSnapshot(snapshotName);
        return SnapshotFormatter.DeserializeSnapshot(fileStream);
    }

    public void RemoveSnapshot(string snapshotName)
    {
        var snapshotFileName = GetSnapshotFileName(snapshotName);

        if (File.Exists(snapshotFileName))
        {
            DeleteFileWithSnapshot(snapshotName);
        }
    }

    private FileStream OpenExistingFileWithSnapshot(string snapshotName)
    {
        var snapshotFileName = GetSnapshotFileName(snapshotName);

        if (!File.Exists(snapshotFileName))
        {
            throw new ArgumentException(
                "Snapshot file with selected snapshot name doesnt exists. Something went wrong!",
                nameof(snapshotFileName));
        }

        return File.OpenRead(snapshotFileName);
    }

    private FileStream CreateFileForSnapshot(string newSnapshotName) =>
        File.Create(GetSnapshotFileName(newSnapshotName));

    private void DeleteFileWithSnapshot(string snapshotName) =>
        File.Delete(GetSnapshotFileName(snapshotName));

    public string GetSnapshotFileName(string snapshotName) =>
        Path.Combine(_pluginDirectory, snapshotName) + DefaultSerializedFileExtension;
}

public class SnapshotFormatter
{
    public static void SerializeSnapshot(FileStream stream, Snapshot snapshot)
    {
        using (stream)
        {
            ProtobufSerializer.Serialize(stream, snapshot);
        }
    }

    public static Snapshot DeserializeSnapshot(FileStream stream)
    {
        Snapshot snapshot;
        using (stream)
        {
            snapshot = ProtobufSerializer.Deserialize<Snapshot>(stream);
        }

        if (snapshot is null)
        {
            throw new NullReferenceException(
                "Snapshot file was empty or not correct format.");
        }

        return snapshot;
    }
}