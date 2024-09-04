using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using Flow.Launcher.Plugin.SnapshotApps.Models;
using Flow.Launcher.Plugin.SnapshotApps.Services;

namespace Flow.Launcher.Plugin.SnapshotApps;

using ProtobufSerializer = ProtoBuf.Serializer;

public class SnapshotManager
{
    private const string DefaultSnapshotsDirectoryName = "Snapshots";
    private const string DefaultSerializedFileExtension = ".bin";

    private readonly List<string> _snapshotNames;
    private readonly FileService _fileService;

    public SnapshotManager(string pluginDirectory)
    {
        _fileService = new FileService(baseDirectory: pluginDirectory, partToCombine: DefaultSnapshotsDirectoryName,
            defaultFileExtension: DefaultSerializedFileExtension);
        _snapshotNames = new List<string>(GetSnapshotsNames());
    }

    public bool IsAnySnapshotExists()
    {
        return _fileService.IsAnyFileInsideDirectory();
    }

    public bool IsSnapshotExists(string snapshotName)
    {
        return _snapshotNames.Contains(snapshotName);
    }

    public List<Snapshot> GetSnapshots()
    {
        var snapshotFiles = _fileService.GetFiles();
        var snapshots = new List<Snapshot>(_snapshotNames.Count);

        foreach (var snapshotFile in snapshotFiles)
        {
            snapshots.Add(GetSnapshot(
                _fileService.GetFileNameWithoutExtension(snapshotFile)));
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

    private string[] GetSnapshotsNames() => _fileService.GetFileNames();

    private FileStream OpenExistingFileWithSnapshot(string snapshotName) =>
        _fileService.OpenReadExistingFile(snapshotName);

    private FileStream CreateFileForSnapshot(string newSnapshotName) =>
        _fileService.CreateFile(newSnapshotName);

    private void DeleteFileWithSnapshot(string snapshotName) =>
        _fileService.DeleteFile(snapshotName);

    private void RenameSnapshotFile(string oldSnapshotName, string newSnapshotName) =>
        _fileService.RenameFile(oldSnapshotName, newSnapshotName);
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