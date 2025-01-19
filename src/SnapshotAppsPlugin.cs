using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.AppsSnapshoter.Extensions;
using Flow.Launcher.Plugin.AppsSnapshoter.Models;
using Flow.Launcher.Plugin.AppsSnapshoter.Services;
using Microsoft.Win32;

namespace Flow.Launcher.Plugin.AppsSnapshoter
{
    public class AppsSnapshoter : IAsyncPlugin
    {
        private string _pluginDirectory;
        private string _pluginKeyWord;

        private PluginInitContext _context;
        private OpenedAppsService _openedAppsService;
        private IconService _iconService;
        private FileDialogService _dialogService;
        private SnapshotManager _snapshotManager;

        private const string PluginIconPath = "/icon.png";
        private const string SnapshotStandardIconPath = "snapshot.png";

        private const string ListSnapshotsKeyword = "list";
        private const string ListSnapshotAppsKeyword = "apps";

        private const int MaxSearchTermsWithAppNameParts = 10;

        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _pluginKeyWord = _context.CurrentPluginMetadata.ActionKeyword;
            _pluginDirectory = _context.CurrentPluginMetadata.PluginDirectory;
            _iconService = new IconService(_pluginDirectory);
            _dialogService = new FileDialogService(_iconService);
            _snapshotManager = new SnapshotManager(_pluginDirectory);

            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken cancellationToken)
        {
            var snapshotName = query.FirstSearch;
            var command = query.SecondSearch;

            if (string.Equals(snapshotName, ListSnapshotsKeyword, StringComparison.OrdinalIgnoreCase))
            {
                return GetSnapshotsResultsList();
            }

            return _snapshotManager.IsSnapshotExists(snapshotName)
                ? HandleExistingSnapshot(query, snapshotName, command)
                : HandleNonExistingSnapshot(snapshotName, command, cancellationToken);
        }

        private List<Result> HandleExistingSnapshot(Query query, string snapshotName, string command)
        {
            if (!string.Equals(command, ListSnapshotAppsKeyword, StringComparison.OrdinalIgnoreCase))
            {
                return GetSingleSnapshotResults(snapshotName, false, command);
            }

            if (query.SearchTerms.Length > MaxSearchTermsWithAppNameParts)
                return GetSnapshotAppsResultsList(snapshotName);

            var appNameParts = query.SearchTerms.Skip(2).ToList();
            var fullAppName = string.Join(" ", appNameParts);

            if (_snapshotManager.IsAppExists(snapshotName, fullAppName))
            {
                return GetSingleAppResults(fullAppName, snapshotName, false);
            }

            return GetSnapshotAppsResultsList(snapshotName);
        }

        private List<Result> HandleNonExistingSnapshot(string snapshotName, string command,
            CancellationToken cancellationToken)
        {
            var results = new List<Result>
            {
                GetCreateSnapshotResult(snapshotName, cancellationToken)
            };

            if (!_snapshotManager.IsAnySnapshotExists())
            {
                return results;
            }

            results.AddRange(new[]
            {
                GetOpenSnapshotResult(snapshotName),
                GetListSnapshotsResult(),
                GetRemoveSnapshotResult(snapshotName),
                GetRenameSnapshotResult(snapshotName, command)
            });

            return results;
        }


        #region Snapshot Actions

        private Result GetOpenSnapshotResult(string selectedSnapshotName)
        {
            return new Result()
                .WithTitle("Open Snapshot")
                .WithSubtitle($"Open {selectedSnapshotName} snapshot")
                .WithIconPath(ActionsIconsPaths.Open)
                .WithFuncReturningBoolActionAsync(async c =>
                {
                    try
                    {
                        await _snapshotManager.OpenSnapshotApps(selectedSnapshotName);
                    }
                    catch (Exception e)
                    {
                        return ShowMsg("No such snapshot", e.Message);
                    }

                    return true;
                });
        }

        private Result GetSnapshotAppsListResult(string currentSnapshotName)
        {
            return new Result()
                .WithTitle("List Apps")
                .WithIconPath(ActionsIconsPaths.List)
                .WithFuncReturningBoolAction(
                    _ =>
                    {
                        _context.API.ChangeQuery($"{_pluginKeyWord} {currentSnapshotName} {ListSnapshotAppsKeyword}");
                        return false;
                    }
                );
        }

        private Result GetRemoveSnapshotResult(string selectedSnapshotName)
        {
            return new Result()
                .WithTitle("Remove Snapshot")
                .WithSubtitle($"Remove {selectedSnapshotName} snapshot")
                .WithIconPath(ActionsIconsPaths.RemoveSnapshot)
                .WithFuncReturningBoolAction(
                    _ =>
                    {
                        _snapshotManager.RemoveSnapshot(selectedSnapshotName);
                        ResetSearchToActionWord();
                        return true;
                    }
                );
        }

        private Result GetRenameSnapshotResult(string currentSnapshotName, string futureSnapshotName)
        {
            return new Result()
                .WithTitle("Rename Snapshot")
                .WithSubtitle($"Rename {currentSnapshotName} snapshot to {futureSnapshotName}")
                .WithIconPath(ActionsIconsPaths.Rename)
                .WithFuncReturningBoolAction(
                    c =>
                    {
                        if (string.IsNullOrEmpty(currentSnapshotName) || string.IsNullOrEmpty(futureSnapshotName))
                        {
                            return ShowMsg("There is no current snapshot  name or future snapshot name", string.Empty);
                        }

                        _context.API.ChangeQuery($"{_pluginKeyWord} {currentSnapshotName} to {futureSnapshotName}");

                        try
                        {
                            _snapshotManager.RenameSnapshot(currentSnapshotName, futureSnapshotName);
                            ResetSearchToActionWord();
                        }
                        catch (Exception e)
                        {
                            return ShowMsg("Renaming Snapshot", e.Message);
                        }

                        return true;
                    }
                );
        }

        #endregion

        #region Snapshot App Actions

        private Result GetRemoveAppFromSnapshotResult(string selectedSnapshotName, string appToRemove)
        {
            return new Result()
                .WithTitle("Remove app")
                .WithSubtitle($"Remove {appToRemove} from {selectedSnapshotName} snapshot")
                .WithIconPath(ActionsIconsPaths.RemoveApp)
                .WithFuncReturningBoolAction(c =>
                {
                    _snapshotManager.RemoveSnapshotApp(selectedSnapshotName, appToRemove);
                    RemoveQueryAppName(selectedSnapshotName);
                    return false;
                });
        }

        private Result GetEditAppResult(string currentSnapshotName, string appToEdit)
        {
            return new Result()
                .WithTitle("Replace this app with other from dialog")
                .WithIconPath(ActionsIconsPaths.Replace)
                .WithFuncReturningBoolAction(
                    _ =>
                    {
                        var appModel =
                            _dialogService.WriteAppModelFromFileDialog($"Select new executable for {appToEdit}");
                        if (appModel is not null)
                            _snapshotManager.EditSnapshotApp(currentSnapshotName, appToEdit, appModel);
                        return true;
                    }
                );
        }

        private Result GetAddAppResult(string currentSnapshotName)
        {
            return new Result()
                .WithTitle("Add another app to snapshot")
                .WithSubtitle($"Add app to {currentSnapshotName}")
                .WithIconPath(ActionsIconsPaths.AddApp)
                .WithFuncReturningBoolAction(
                    _ =>
                    {
                        var appModel =
                            _dialogService.WriteAppModelFromFileDialog(
                                $"Select new executable to add to {currentSnapshotName} snapshot");
                        if (appModel is not null)
                            _snapshotManager.AddSnapshotApp(currentSnapshotName, appModel);
                        return true;
                    }
                );
        }

        private Result GetBlockDeleteAppResult(string selectedSnapshotName, string appToBlockRemove)
        {
            return new Result()
                .WithTitle("Delete and block app")
                .WithSubtitle("Remove app from all snapshots and do not add on future snapshots creation")
                .WithIconPath(ActionsIconsPaths.RemoveAll)
                .WithFuncReturningBoolAction(
                    _ =>
                    {
                        _snapshotManager.RemoveAppFromAllSnapshotsIfExists(appToBlockRemove);
                        RemoveQueryAppName(selectedSnapshotName);
                        return false;
                    }
                );
        }

        #endregion

        private Result GetListSnapshotsResult()
        {
            return new Result()
                .WithTitle("List Snapshots")
                .WithIconPath(ActionsIconsPaths.List)
                .WithFuncReturningBoolAction(
                    _ =>
                    {
                        _context.API.ChangeQuery(_pluginKeyWord + " " + ListSnapshotsKeyword);
                        return false;
                    }
                );
        }

        private Result GetCreateSnapshotResult(string queryFirstSearch, CancellationToken cancellationToken)
        {
            return new Result()
                .WithTitle($"Create {queryFirstSearch} Snapshot")
                .WithSubtitle("Save locally currently active apps for later for subsequent launch")
                .WithIconPath(
                    ActionsIconsPaths.CreateSnapshot).WithFuncReturningBoolAction(
                    c =>
                    {
                        if (string.IsNullOrEmpty(queryFirstSearch))
                        {
                            return ShowMsg("Snapshot name", "Snapshot name was not written");
                        }

                        _ = CreateAppsSnapshotAsync(queryFirstSearch, cancellationToken);

                        return true;
                    }
                );
        }

        private async Task CreateAppsSnapshotAsync(string snapshotName, CancellationToken cancellationToken)
        {
            try
            {
                var openedApps = await GetCurrentlyOpenAppsAsync(cancellationToken);

                if (openedApps.Count == 0)
                {
                    _context.API.ShowMsgError("Smth goes wrong", "There are no app models to write in snapshots");
                    return;
                }

                var snapshotIcon = openedApps[0].IconPath ?? SnapshotStandardIconPath;
                var snapshot = new Snapshot
                {
                    SnapshotName = snapshotName,
                    AppModelsIncluded = openedApps,
                    IcoPath = snapshotIcon
                };

                _snapshotManager.CreateSnapshot(snapshot);
            }
            catch (Exception e)
            {
                ShowMsg("Creating Snapshot Error", e.Message);
            }
        }


        private List<Result> GetSingleSnapshotResults(string selectedSnapshotName, bool isFromList,
            string newSnapshotName = " ")
        {
            if (isFromList)
            {
                _context.API.ChangeQuery($"{_pluginKeyWord} {selectedSnapshotName}");
            }

            var results = new List<Result>
            {
                GetRemoveSnapshotResult(selectedSnapshotName),
                GetOpenSnapshotResult(selectedSnapshotName),
                GetRenameSnapshotResult(selectedSnapshotName, newSnapshotName),
                GetSnapshotAppsListResult(selectedSnapshotName),
                GetAddAppResult(selectedSnapshotName)
            };

            return results;
        }

        private List<Result> GetSingleAppResults(string selectedAppName, string selectedSnapshotName, bool isFromList)
        {
            if (isFromList)
            {
                _context.API.ChangeQuery(
                    $"{_pluginKeyWord} {selectedSnapshotName} {ListSnapshotAppsKeyword} {selectedAppName}");
            }

            var results = new List<Result>
            {
                GetRemoveAppFromSnapshotResult(selectedSnapshotName, selectedAppName),
                GetEditAppResult(selectedSnapshotName, selectedAppName),
                GetBlockDeleteAppResult(selectedSnapshotName, selectedAppName),
            };

            return results;
        }

        private async Task<List<AppModel>> GetCurrentlyOpenAppsAsync(CancellationToken cancellationToken)
        {
            _openedAppsService = await OpenedAppsService.CreateAsync(_pluginDirectory, _context);
            await Task.Run(() => _openedAppsService.WriteAppsIconsToModels(), cancellationToken);

            var writedAppsCount = _openedAppsService.AppModels.Count;
            LogInfo(nameof(GetCurrentlyOpenAppsAsync),
                "Writed Apps Count is " + writedAppsCount);
            return _openedAppsService.AppModels;
        }

        private bool ShowMsg(string msgTitle, string msgSubtitle, string icoPath = PluginIconPath,
            bool isClosingAfterMsg = false)
        {
            _context.API.ShowMsg(msgTitle,
                msgSubtitle, icoPath);
            return isClosingAfterMsg;
        }

        private List<Result> GetSnapshotsResultsList() =>
            _snapshotManager.GetSnapshots().ToResults(snapshotActionsResults: GetSingleSnapshotResults);

        private List<Result> GetSnapshotAppsResultsList(string selectedSnapshotName) =>
            _snapshotManager.GetSnapshotApps(selectedSnapshotName)
                .ToResults(snapshotAppActionsResults: GetSingleAppResults, selectedSnapshotName);

        private void RemoveQueryAppName(string snapshotName) =>
            _context.API.ChangeQuery($"{_pluginKeyWord} {snapshotName} {ListSnapshotAppsKeyword}");

        private void ResetSearchToActionWord() => _context.API.ChangeQuery(_pluginKeyWord);

        private void LogInfo(string methodName, string message) =>
            _context.API.LogInfo(nameof(AppsSnapshoter), message, methodName);
    }
}