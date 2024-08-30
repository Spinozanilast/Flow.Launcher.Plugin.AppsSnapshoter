using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps;

using ProtobufSerializer = ProtoBuf.Serializer;

public class SnapshotManager
{
    private const string DefaultSnapshotsDirectoryName = "Snapshots";
    private const string DefaultSerializedFileExtension = ".bin";

    private readonly string _snapshotsDirectory;
    private readonly List<string> _snapshotNames;

    public SnapshotManager(string pluginDirectory)
    {
        _snapshotsDirectory = Path.Combine(pluginDirectory, DefaultSnapshotsDirectoryName);

        if (!Directory.Exists(_snapshotsDirectory))
        {
            Directory.CreateDirectory(_snapshotsDirectory);
        }

        _snapshotNames = new List<string>(GetSnapshotsNames());
    }

    public bool IsAnySnapshotExists()
    {
        return Directory.GetFiles(_snapshotsDirectory).Length > 0;
    }

    public bool IsSnapshotExists(string snapshotName)
    {
        return _snapshotNames.Contains(snapshotName);
    }

    public List<Snapshot> GetSnapshots()
    {
        var snapshotFiles = Directory.GetFiles(_snapshotsDirectory);
        var snapshots = new List<Snapshot>(_snapshotNames.Count);

        foreach (var snapshotFile in snapshotFiles)
        {
            snapshots.Add(GetSnapshot(
                Path.GetFileName(snapshotFile)[..^(DefaultSerializedFileExtension.Length)]));
        }

        return snapshots;
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
            var exePath = appModel.ExecutionFilePath;
            if (File.Exists(exePath))
            {
                Process.Start(exePath);
            }
            else
            {
                throw new Exception("Execution file of app written if file does not exists.");
            }
        }
    }

    public void RemoveSnapshot(string snapshotName)
    {
        if (IsSnapshotExists(snapshotName))
        {
            DeleteFileWithSnapshot(snapshotName);
            _snapshotNames.Remove(snapshotName);
        }
    }

    public void CreateSnapshot(Snapshot newSnapshot)
    {
        var newSnapshotName = newSnapshot.SnapshotName;

        if (IsSnapshotExists(newSnapshotName))
        {
            throw new Exception($"Snapshot with same name ({newSnapshotName}) already exists ");
        }

        _snapshotNames.Add(newSnapshotName);
        var fileStream = CreateFileForSnapshot(newSnapshotName);
        SnapshotFormatter.SerializeSnapshot(fileStream, newSnapshot);
    }

    public void RenameSnapshot(string currentSnapshotName, string futureSnapshotName)
    {
        if (IsSnapshotExists(futureSnapshotName))
        {
            throw new Exception("The snapshot with new snapshot name is exists");
        }

        var snapshot = GetSnapshot(currentSnapshotName);
        snapshot.SnapshotName = futureSnapshotName;
        DeleteFileWithSnapshot(currentSnapshotName);
        CreateSnapshot(snapshot);

        _snapshotNames.Remove(currentSnapshotName);
        _snapshotNames.Add(futureSnapshotName);
    }

    public Snapshot GetSnapshot(string snapshotName)
    {
        var fileStream = OpenExistingFileWithSnapshot(snapshotName);
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

    private string[] GetSnapshotsNames()
    {
        var snapshotFiles = Directory.GetFiles(_snapshotsDirectory);

        var snapshotsNames = new string[snapshotFiles.Length];

        for (int i = 0; i < snapshotsNames.Length; i++)
        {
            snapshotsNames[i] = Path.GetFileName(snapshotFiles[i])[..^DefaultSerializedFileExtension.Length];
        }

        return snapshotsNames;
    }

    private FileStream CreateFileForSnapshot(string newSnapshotName) =>
        File.Create(GetSnapshotFileName(newSnapshotName));

    private void DeleteFileWithSnapshot(string snapshotName) =>
        File.Delete(GetSnapshotFileName(snapshotName));

    private void RenameSnapshotFile(string oldSnapshotName, string newSnapshotName) =>
        File.Move(GetSnapshotFileName(oldSnapshotName), GetSnapshotFileName(newSnapshotName));

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