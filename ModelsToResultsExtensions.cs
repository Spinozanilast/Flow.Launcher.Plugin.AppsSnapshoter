using System.Collections.Generic;
using System.Linq;

namespace Flow.Launcher.Plugin.SnapshotApps;

public static class ModelsToResultsExtensions
{
    public static List<Result> ToResults(this string[] snapshotNames)
    {
        return snapshotNames.Select(name => new Result()
        {
            Title = name
        }).ToList();
    }
}