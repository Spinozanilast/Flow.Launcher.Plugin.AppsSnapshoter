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

            if (query.FirstSearch.ToLower() == "list")
            {
                return GetSnaphotsList();
            }

            var createResult = GetCreateSnapshotResult(query, cancellationToken);
            var listResult = GetListSnapshotsResult();
            var removeSnapshotResult = GetRemoveSnapshotResult(query);
            var openSnapshotResult = GetOpenSnapshotResult(query);

            results.Add(createResult);

            if (_snapshotManager.IsAnySnapshotExists())
            {
                results.Add(openSnapshotResult);
                results.Add(listResult);
                results.Add(removeSnapshotResult);
            }

            return results;
        }

        private Result GetOpenSnapshotResult(Query query)
        {
            return CreateSingleResult(
                $"Open Snapshot",
                "Open apps stored is snapshot",
                "ActionsImages/open-icon.png",
                c =>
                {
                    try
                    {
                        _snapshotManager.OpenSnapshotApps(query.FirstSearch);
                    }
                    catch (Exception e)
                    {
                        _context.API.ShowMsg("No such snapshot", $"There is no snapshot with name {query.FirstSearch}");
                        return false;
                    }

                    return true;
                }
            );
        }

        private Result GetOpenSnapshotResult(string selectedSnapshotName)
        {
            return CreateSingleResult(
                $"Open {selectedSnapshotName} Snapshot",
                "",
                "ActionsImages/open-icon.png",
                c =>
                {
                    _snapshotManager.OpenSnapshotApps(selectedSnapshotName);
                    return true;
                }
            );
        }

        private Result GetRemoveSnapshotResult(Query query)
        {
            return CreateSingleResult(
                $"Remove Snapshot",
                "Remove selected Snapshot",
                "ActionsImages/remove-icon.png",
                c =>
                {
                    var snapshotName = query.FirstSearch;
                    _snapshotManager.RemoveSnapshot(snapshotName);
                    return false;
                }
            );
        }

        private Result GetRemoveSnapshotResult(string selectedSnapshotName)
        {
            return CreateSingleResult(
                $"Remove {selectedSnapshotName} Snapshot",
                "",
                "ActionsImages/remove-icon.png",
                c =>
                {
                    _snapshotManager.RemoveSnapshot(selectedSnapshotName);
                    return false;
                }
            );
        }

        private Result GetListSnapshotsResult()
        {
            return CreateSingleResult(
                "List Snapshot",
                "List existing locally snapshots",
                "ActionsImages/list-icon.png",
                c =>
                {
                    try
                    {
                        _context.API.ChangeQuery(_pluginKeyWord + " list");
                    }
                    catch (Exception ex)
                    {
                        _context.API.ShowMsg("There are no snapshots located.", "Create Snapshots");
                    }

                    return false;
                }
            );
        }

        private Result GetCreateSnapshotResult(Query query, CancellationToken cancellationToken)
        {
            return CreateSingleResult(
                $"Create Snapshot",
                $"Save locally currently active apps for later for subsequent launch",
                "ActionsImages/add-icon.png", c =>
                {
                    if (string.IsNullOrEmpty(query.FirstSearch))
                    {
                        return ShowMsg("Snapshot name", "Snapshot Name was not written");
                    }

                    try
                    {
                        _ = CreateAppsSnapshot(query.FirstSearch, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        ShowMsg("There is the snapshot with same name", e.Message);
                        return false;
                    }

                    return true;
                }
            );
        }

        private List<Result> GetSnaphotsList() => _snapshotManager.GetSnapshots().ToResults(GetSingleSnapshotResults);

        private Result[] GetSingleSnapshotResults(string selectedSnapshotName)
        {
            _context.API.ChangeQuery($"{_pluginKeyWord} {selectedSnapshotName}");
            return new Result[]
            {
                GetRemoveSnapshotResult(selectedSnapshotName),
                GetOpenSnapshotResult(selectedSnapshotName)
            };
        }

        private bool ShowMsg(string msgTitle, string msgSubtitle, string icoPath = PluginIconPath,
            bool isClosingAfterMsg = false)
        {
            _context.API.ShowMsg(msgTitle,
                msgSubtitle, icoPath);
            return isClosingAfterMsg;
        }

        private async Task CreateAppsSnapshot(string snapshotName, CancellationToken cancellationToken)
        {
            var appModels = await GetCurrentlyOpenApps(cancellationToken);
            var snapshotIcon = appModels[0].IconPath ?? SnapshotStandardIconPath;
            var snapshot = new Snapshot
            {
                SnapshotName = snapshotName,
                AppModelsIncluded = appModels,
                IcoPath = snapshotIcon
            };
            _snapshotManager.CreateSnapshot(snapshot);
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
    }
}