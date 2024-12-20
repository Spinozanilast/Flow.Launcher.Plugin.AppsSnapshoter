﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.AppsSnapshoter.Models;
using Flow.Launcher.Plugin.AppsSnapshoter.Services;

namespace Flow.Launcher.Plugin.AppsSnapshoter;

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

    public async ValueTask OpenAppsSnapshoter(string snapshotName)
    {
        if (!IsSnapshotExists(snapshotName))
        {
            throw new NullReferenceException("There is no such snapshot to open apps.");
        }

        var snapshot = GetSnapshot(snapshotName);
        var startInfo = new ProcessStartInfo
        {
            UseShellExecute = true
        };

        foreach (var exePath in snapshot.AppModelsIncluded.Select(appModel => appModel.ExecutionFilePath))
        {
            if (File.Exists(exePath) || Uri.IsWellFormedUriString(exePath, UriKind.RelativeOrAbsolute))
            {
                startInfo.FileName = exePath;
                Task appRunTask = Task.Run(() => Process.Start(startInfo));
                await appRunTask;
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
        SnapshotBinaryFormatter.SerializeSnapshot(fileStream, newSnapshot);
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
        return SnapshotBinaryFormatter.DeserializeSnapshot(fileStream);
    }

    public List<AppModel> GetAppsSnapshoter(string snapshotName) => GetSnapshot(snapshotName).AppModelsIncluded;

    private string[] GetSnapshotsNames() => _fileService.GetFileNames();

    private FileStream OpenExistingFileWithSnapshot(string snapshotName) =>
        _fileService.OpenReadExistingFile(snapshotName);

    private FileStream CreateFileForSnapshot(string newSnapshotName) =>
        _fileService.CreateFile(newSnapshotName);

    private void DeleteFileWithSnapshot(string snapshotName) =>
        _fileService.DeleteFile(snapshotName);
}