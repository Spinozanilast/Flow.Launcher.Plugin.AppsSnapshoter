using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.SnapshotApps
{
    /// <summary>
    /// 
    /// </summary>
    public class SnapshotApps : IAsyncPlugin
    {
        private PluginInitContext _context;
        private OpenedAppsHelper _openedAppsHelper;


        private readonly Dictionary<string, Func<string, List<Result>>> startResults =
            new(StringComparer.InvariantCultureIgnoreCase);

        public Task InitAsync(PluginInitContext context)
        {
            _context = context;

            // startResults.Add("Create",);

            return Task.CompletedTask;
        }

        public async Task<List<Result>> QueryAsync(Query query, CancellationToken cancellationToken)
        {
            return new List<Result>();
        }

        private async Task<List<Result>> GetCurrentlyOpenApps(CancellationToken cancellationToken)
        {
            _openedAppsHelper = new OpenedAppsHelper(_context.CurrentPluginMetadata.PluginDirectory);

            await Task.Run(() => _openedAppsHelper.WriteAppsIconsToModels(), cancellationToken);

            var resultList = new List<Result>();
            foreach (var appModel in _openedAppsHelper.AppModels)
            {
                resultList.Add(new Result
                {
                    Title = appModel.AppModuleName,
                    SubTitle = appModel.ExecutionFilePath,
                    IcoPath = appModel.IconPath
                });
            }

            return resultList;
        }

        private List<Result> CreateAppsSnapshot(string snapshotName)
        {
            return new List<Result>();
        }
    }
}