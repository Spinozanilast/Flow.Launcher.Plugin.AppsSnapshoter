using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps;

public static class ModelsToResultsExtensions
{
    public static List<Result> ToResults(this Snapshot[] snapshots, Func<string, Result[]> listResultAction)
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
                Action = context =>
                {
                    listResultAction.Invoke(snapshot.SnapshotName);
                    return false;
                }
            };
        }).ToList();
    }
}