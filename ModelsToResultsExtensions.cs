using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps;

public static class ModelsToResultsExtensions
{
    private static readonly bool IsFromList = true;

    public static List<Result> ToResults(this List<Snapshot> snapshots, Func<string, bool, List<Result>> listResultAction)
    {
        return snapshots.Select(snapshot =>
        {
            var apps = string.Join(", ", snapshot.AppModelsIncluded.Select(model => model.AppModuleName).ToList());
            return new Result
            {
                Title = snapshot.SnapshotName,
                SubTitle = apps,
                SubTitleToolTip = string.Join(", ",
                    snapshot.AppModelsIncluded.Select(model => model.AppModuleName).ToList()),
                IcoPath = snapshot.IcoPath,
                Action = _ =>
                {
                    listResultAction.Invoke(snapshot.SnapshotName, IsFromList);
                    return false;
                }
            };
        }).ToList();
    }
}