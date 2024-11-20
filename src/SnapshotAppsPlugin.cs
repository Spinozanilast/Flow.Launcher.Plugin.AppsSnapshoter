using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.AppsSnapshoter.Extensions;
using Flow.Launcher.Plugin.AppsSnapshoter.Models;
using Flow.Launcher.Plugin.AppsSnapshoter.Services;

namespace Flow.Launcher.Plugin.AppsSnapshoter
{
    public class AppsSnapshoter : IAsyncPlugin
    {
        private string _pluginDirectory;
        private string _pluginKeyWord;

        private PluginInitContext _context;
        private OpenedAppsService _openedAppsService;
        private SnapshotManager _snapshotManager;

        private const string PluginIconPath = "/icon.png";
        private const string SnapshotStandardIconPath = "snapshot.png";
        private const string ListSnapshotsKeyword = "list";
        private const string ListAppsSnapshoterKeyword = "apps";

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
                return GetSnaphotsResultsList();
            }

            if (_snapshotManager.IsSnapshotExists(queryFirstSearch))
            {
                return querySecondSearch.ToLower() == ListAppsSnapshoterKeyword
                    ? GetAppsSnapshoterResultsList(queryFirstSearch)
                    : GetSingleSnapshotResults(queryFirstSearch, false, querySecondSearch);
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
                .WithFuncReturningBoolActionAsync(async c =>
                {
                    try
                    {
                        await _snapshotManager.OpenAppsSnapshoter(selectedSnapshotName);
                    }
                    catch (Exception e)
                    {
                        return ShowMsg("No such snapshot", e.Message);
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
                        _context.API.ChangeQuery(_pluginKeyWord + " " + ListSnapshotsKeyword);
                        return false;
                    }
                );
        }

        private Result GetAppsSnapshoterListResult(string currentSnapshotName)
        {
            return new Result()
                .WithTitle("List Apps")
                .WithIconPath("ActionsIcons/list-icon.png")
                .WithFuncReturningBoolAction(
                    c =>
                    {
                        _context.API.ChangeQuery($"{_pluginKeyWord} {currentSnapshotName} {ListAppsSnapshoterKeyword}");
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
                GetAppsSnapshoterListResult(selectedSnapshotName)
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

        private List<Result> GetSnaphotsResultsList() =>
            _snapshotManager.GetSnapshots().ToResults(listResultAction: GetSingleSnapshotResults);

        private List<Result> GetAppsSnapshoterResultsList(string selectedSnapshotName) =>
            _snapshotManager.GetAppsSnapshoter(selectedSnapshotName).ToResults();

        private void ResetSearchToActionWord() => _context.API.ChangeQuery(_pluginKeyWord);

        private void LogInfo(string methodName, string message) =>
            _context.API.LogInfo(nameof(AppsSnapshoter), message, methodName);
    }
}