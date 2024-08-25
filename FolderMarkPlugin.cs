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
        private PluginInitContext _context;
        private OpenedAppsHelper _openedAppsHelper;
        private SnapshotManager _snapshotManager;

        private const string PluginIconPath = "/icon.png";

        private Dictionary<string, Func<string, List<Result>>> _listAfterResults =
            new(StringComparer.InvariantCultureIgnoreCase);

        private Dictionary<string, Func<string>> _singleResults =
            new(StringComparer.InvariantCultureIgnoreCase);

        public Task InitAsync(PluginInitContext context)
        {
            _context = context;
            _pluginDirectory = _context.CurrentPluginMetadata.PluginDirectory;
            _snapshotManager = new SnapshotManager(_pluginDirectory);

            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken cancellationToken)
        {
            var results = new List<Result>();

            var createResult = CreateSingleResult(
                $"Create {query.FirstSearch} Snapshot",
                $"Save locally currently active apps for later for subsequent launch",
                "ActionsImages/add-item-icon.png",
                c =>
                {
                    if (string.IsNullOrEmpty(query.FirstSearch))
                    {
                        return ShowMsg("Snapshot name", "Snapshot Name was not written");
                    }

                    _ = CreateAppsSnapshot(query.FirstSearch, cancellationToken);
                    return true;
                }
            );
            results.Add(createResult);

            var listResult = CreateSingleResult(
                "List Snapshot",
                "List existing locally snapshots",
                "ActionsImages/list-icon.png",
                c =>
                {
                    try
                    {
                        GetSnaphotsList();
                    }
                    catch (Exception ex)
                    {
                        _context.API.ShowMsg("There are no snapshots located.", "Create Snapshots");
                    }

                    return true;
                }
            );

            var removeSnapshotResult = CreateSingleResult(
                $"Remove {query.FirstSearch} Snapshot",
                "Remove selected Snapshot",
                "ActionsImages/remove-icon.png",
                c =>
                {
                    var snapshotToDeleteName = query.FirstSearch;
                    _snapshotManager.RemoveSnapshot(snapshotToDeleteName);
                    return true;
                }
            );

            var openSnapshotResult = CreateSingleResult(
                $"Open {query.FirstSearch} Snapshot",
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

            if (_snapshotManager.IsAnySnapshotExists())
            {
                results.Add(openSnapshotResult);
                results.Add(listResult);
                results.Add(removeSnapshotResult);
            }

            return results;
        }

        private List<Result> GetSnaphotsList() => _snapshotManager.GetSnapshotsNames().ToResults();


        private bool ShowMsg(string msgTitle, string msgSubtitle, string icoPath = PluginIconPath,
            bool isClosingAfterMsg = false)
        {
            _context.API.ShowMsg(msgTitle,
                msgSubtitle, icoPath);
            return isClosingAfterMsg;
        }

        private async Task CreateAppsSnapshot(string snapshotName, CancellationToken cancellationToken)
        {
            if (string.IsNullOrEmpty(snapshotName))
            {
            }

            var appModels = await GetCurrentlyOpenApps(cancellationToken);
            var snapshot = new Snapshot
            {
                SnapshotName = snapshotName,
                AppModelsIncluded = appModels
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