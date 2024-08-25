using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps;

using ProtobufSerializer = ProtoBuf.Serializer;

public class SnapshotManager
{
    private const string DefaultSnapshotsDirectoryName = "Snapshots";
    private const string DefaultSerializedFileExtension = ".bin";

    private readonly string _snapshotsDirectory;

    public SnapshotManager(string pluginDirectory)
    {
        _snapshotsDirectory = Path.Combine(pluginDirectory, DefaultSnapshotsDirectoryName);

        if (!Directory.Exists(_snapshotsDirectory))
        {
            Directory.CreateDirectory(_snapshotsDirectory);
        }
    }

    public bool IsAnySnapshotExists()
    {
        return Directory.GetFiles(_snapshotsDirectory).Length > 0;
    }

    public bool IsSnapshotExists(string snapshotName)
    {
        return GetSnapshotsNames().Contains(snapshotName);
    }

    public string[] GetSnapshotsNames()
    {
        var snapshotFiles = Directory.GetFiles(_snapshotsDirectory);

        var snapshotsNames = new string[snapshotFiles.Length];

        for (int i = 0; i < snapshotsNames.Length; i++)
        {
            snapshotsNames[i] = Path.GetFileName(snapshotFiles[i])[..^(DefaultSnapshotsDirectoryName.Length)];
        }

        return snapshotsNames;
    }

    public void OpenSnapshotApps(string snapshotName)
    {
        if (!IsSnapshotExists(snapshotName))
        {
            throw new NullReferenceException("There is no such snapshot to open apps.");
        }

        var snapshot = GetSnapshot(snapshotName);
        foreach (var appModel in snapshot.AppModelsIncluded)
        {
            Process.Start(appModel.ExecutionFilePath);
        }
    }

    public void RemoveSnapshot(string snapshotName)
    {
        var snapshotFileName = GetSnapshotFileName(snapshotName);

        if (File.Exists(snapshotFileName))
        {
            DeleteFileWithSnapshot(snapshotName);
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

    private string GetSnapshotFileName(string snapshotName) =>
        Path.Combine(_snapshotsDirectory, snapshotName) + DefaultSerializedFileExtension;
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