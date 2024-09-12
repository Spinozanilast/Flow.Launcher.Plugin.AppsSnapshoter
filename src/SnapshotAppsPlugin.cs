using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.SnapshotApps.Extensions;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps
{
    public class SnapshotApps : IAsyncPlugin
    {
        private string _pluginDirectory;
        private string _pluginKeyWord;

        private PluginInitContext _context;
        private OpenedAppsService _openedAppsService;
        private SnapshotManager _snapshotManager;

        private const string PluginIconPath = "/icon.png";
        private const string SnapshotStandardIconPath = "snapshot.png";
        private const string ListSnapshotsKeyword = "list";

        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _pluginKeyWord = _context.CurrentPluginMetadata.ActionKeyword;
            _pluginDirectory = _context.CurrentPluginMetadata.PluginDirectory;
            _snapshotManager = new SnapshotManager(_pluginDirectory);

            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken cancellationToken)
        {
            var results = new List<Result>();
            var queryFirstSearch = query.FirstSearch;
            var querySecondSearch = query.SecondSearch;

            if (queryFirstSearch.ToLower() == ListSnapshotsKeyword)
            {
                return GetSnaphotsList();
            }

            if (_snapshotManager.IsSnapshotExists(queryFirstSearch))
            {
                return GetSingleSnapshotResults(queryFirstSearch, false, querySecondSearch);
            }

            var createResult = GetCreateSnapshotResult(queryFirstSearch, cancellationToken);
            var listResult = GetListSnapshotsResult();
            var removeSnapshotResult = GetRemoveSnapshotResult(queryFirstSearch);
            var openSnapshotResult = GetOpenSnapshotResult(queryFirstSearch);
            var renameSnapshotResult = GetRenameSnapshotResult(queryFirstSearch, querySecondSearch);

            results.Add(createResult);

            if (_snapshotManager.IsAnySnapshotExists())
            {
                results.Add(openSnapshotResult);
                results.Add(listResult);
                results.Add(removeSnapshotResult);
                results.Add(renameSnapshotResult);
            }

            return results;
        }

        private Result GetOpenSnapshotResult(string selectedSnapshotName)
        {
            return new Result()
                .WithTitle("Open Snapshot")
                .WithSubtitle($"Open {selectedSnapshotName} snapshot")
                .WithIconPath("ActionsIcons/open-icon.png")
                .WithFuncReturningBoolAction(c =>
                {
                    try
                    {
                        _snapshotManager.OpenSnapshotApps(selectedSnapshotName);
                    }
                    catch (Exception)
                    {
                        return ShowMsg("No such snapshot", $"There is no snapshot with name {selectedSnapshotName}");
                    }

                    return true;
                });
        }

        private Result GetRemoveSnapshotResult(string selectedSnapshotName)
        {
            return new Result()
                .WithTitle("Remove Snapshot")
                .WithSubtitle($"Remove {selectedSnapshotName} snapshot")
                .WithIconPath("ActionsIcons/remove-icon.png")
                .WithFuncReturningBoolAction(
                    c =>
                    {
                        _snapshotManager.RemoveSnapshot(selectedSnapshotName);
                        ResetSearchToActionWord();
                        return true;
                    }
                );
        }

        private Result GetListSnapshotsResult()
        {
            return new Result()
                .WithTitle("List Snapshots")
                .WithIconPath("ActionsIcons/list-icon.png")
                .WithFuncReturningBoolAction(
                    c =>
                    {
                        try
                        {
                            _context.API.ChangeQuery(_pluginKeyWord + " list");
                        }
                        catch (Exception)
                        {
                            _context.API.ShowMsg("There are no snapshots located.", "Create Snapshots");
                        }

                        return false;
                    }
                );
        }

        private Result GetCreateSnapshotResult(string queryFirstSearch, CancellationToken cancellationToken)
        {
            return new Result()
                .WithTitle($"Create {queryFirstSearch} Snapshot")
                .WithSubtitle($"Save locally currently active apps for later for subsequent launch")
                .WithIconPath(
                    "ActionsIcons/add-icon.png").WithFuncReturningBoolAction(
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

        private Result GetRenameSnapshotResult(string currentSnapshotName, string futureSnapshotName)
        {
            return new Result()
                .WithTitle("Rename Snapshot")
                .WithSubtitle($"Rename {currentSnapshotName} snapshot to {futureSnapshotName}")
                .WithIconPath("ActionsIcons/rename-icon.png")
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
                GetRenameSnapshotResult(selectedSnapshotName, newSnapshotName)
            };

            return results;
        }

        private async Task CreateAppsSnapshotAsync(string snapshotName, CancellationToken cancellationToken)
        {
            try
            {
                var openedApps = await GetCurrentlyOpenAppsAsync(cancellationToken);
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

        private async Task<List<AppModel>> GetCurrentlyOpenAppsAsync(CancellationToken cancellationToken)
        {
            _openedAppsService = new OpenedAppsService(_pluginDirectory);

            await Task.Run(() => _openedAppsService.WriteAppsIconsToModels(), cancellationToken);

            return _openedAppsService.AppModels;
        }

        private bool ShowMsg(string msgTitle, string msgSubtitle, string icoPath = PluginIconPath,
            bool isClosingAfterMsg = false)
        {
            _context.API.ShowMsg(msgTitle,
                msgSubtitle, icoPath);
            return isClosingAfterMsg;
        }

        private List<Result> GetSnaphotsList() => _snapshotManager.GetSnapshots().ToResults(GetSingleSnapshotResults);

        private void ResetSearchToActionWord() => _context.API.ChangeQuery(_pluginKeyWord);
    }
}