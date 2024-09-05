using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps
{
    /// <summary>
    /// 
    /// </summary>
    public class SnapshotApps : IAsyncPlugin
    {
        private string _pluginDirectory;
        private string _pluginKeyWord;

        private PluginInitContext _context;
        private OpenedAppsHelper _openedAppsHelper;
        private SnapshotManager _snapshotManager;

        private const string PluginIconPath = "/icon.png";
        private const string SnapshotStandardIconPath = "snapshot.png";

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

            if (queryFirstSearch.ToLower() == "list")
            {
                return GetSnaphotsList();
            }

            if (_snapshotManager.IsSnapshotExists(queryFirstSearch))
            {
                return GetSingleSnapshotResults(queryFirstSearch, querySecondSearch, false);
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
            return CreateSingleResult(
                "Open Snapshot",
                $"Open {selectedSnapshotName} snapshot",
                "ActionsIcons/open-icon.png",
                c =>
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
                }
            );
        }


        private Result GetRemoveSnapshotResult(string selectedSnapshotName)
        {
            return CreateSingleResult(
                "Remove Snapshot",
                $"Remove {selectedSnapshotName} snapshot",
                "ActionsIcons/remove-icon.png",
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
            return CreateSingleResult(
                "List Snapshot",
                "List existing locally snapshots",
                "ActionsIcons/list-icon.png",
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
            return CreateSingleResult(
                $"Create {queryFirstSearch} Snapshot",
                $"Save locally currently active apps for later for subsequent launch",
                "ActionsIcons/add-icon.png",
                c =>
                {
                    if (string.IsNullOrEmpty(queryFirstSearch))
                    {
                        return ShowMsg("Snapshot name", "Snapshot name was not written");
                    }

                    _ = CreateAppsSnapshot(queryFirstSearch, cancellationToken);

                    return true;
                }
            );
        }

        private Result GetRenameSnapshotResult(string currentSnapshotName, string futureSnapshotName)
        {
            return CreateSingleResult(
                "Rename Snapshot",
                $"Rename snapshot wi {currentSnapshotName} to {futureSnapshotName}",
                "ActionsIcons/add-icon.png",
                c =>
                {
                    if (string.IsNullOrEmpty(currentSnapshotName) || string.IsNullOrEmpty(futureSnapshotName))
                    {
                        return ShowMsg("There is no current snapshot name or future snapshot name", string.Empty);
                    }

                    try
                    {
                        _snapshotManager.RenameSnapshot(currentSnapshotName, futureSnapshotName);
                    }
                    catch (Exception e)
                    {
                        return ShowMsg("Renaming Snapshot", e.Message);
                    }

                    ResetSearchToActionWord();
                    return true;
                }
            );
        }


        private List<Result> GetSingleSnapshotResults(string selectedSnapshotName, string newSnapshotName = "",
            bool fromList = true)
        {
            _context.API.ChangeQuery($"{_pluginKeyWord} {selectedSnapshotName}");

            var results = new List<Result>
            {
                GetRemoveSnapshotResult(selectedSnapshotName),
                GetOpenSnapshotResult(selectedSnapshotName),
                GetRenameSnapshotResult(selectedSnapshotName, newSnapshotName)
            };

            return results;
        }

        private async Task CreateAppsSnapshot(string snapshotName, CancellationToken cancellationToken)
        {
            try
            {
                var openedApps = await GetCurrentlyOpenApps(cancellationToken);
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


        private async Task<List<AppModel>> GetCurrentlyOpenApps(CancellationToken cancellationToken)
        {
            _openedAppsHelper = new OpenedAppsHelper(_pluginDirectory);

            await Task.Run(() => _openedAppsHelper.WriteAppsIconsToModels(), cancellationToken);

            return _openedAppsHelper.AppModels;
        }

        private Result CreateSingleResult(string title,
            string subtitle = "",
            string icoPath = PluginIconPath,
            Func<ActionContext, bool> action = default)
        {
            return new Result
            {
                Title = title,
                SubTitle = subtitle,
                IcoPath = icoPath,
                Action = action
            };
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