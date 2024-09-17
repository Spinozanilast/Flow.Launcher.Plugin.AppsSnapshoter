using System;
using System.Collections.Generic;
using System.Linq;
using Flow.Launcher.Plugin.SnapshotApps.Models;

namespace Flow.Launcher.Plugin.SnapshotApps.Extensions;

public static class ModelsToResultsExtensions
{
    private const bool IsFromList = true;

    public static List<Result> ToResults(this List<Snapshot> snapshots,
        Func<string, bool, string, List<Result>> listResultAction)
    {
        return snapshots.Select(snapshot =>
        {
            var apps = string.Join(", ", snapshot.AppModelsIncluded.Select(model => model.AppModuleName).ToList());
            return new Result()
                .WithTitle(snapshot.SnapshotName)
                .WithSubtitle(apps)
                .WithIconPath(snapshot.IcoPath)
                .WithFuncReturningBoolAction(_ =>
                {
                    listResultAction.Invoke(snapshot.SnapshotName, IsFromList, string.Empty);
                    return false;
                });
        }).ToList();
    }

    public static List<Result> ToResults(this List<AppModel> apps)
    {
        return apps.Select(app => new Result()
            .WithTitle(app.AppModuleName)
            .WithSubtitle(app.ExecutionFilePath)
            .WithIconPath(app.IconPath)).ToList();
    }
    
}