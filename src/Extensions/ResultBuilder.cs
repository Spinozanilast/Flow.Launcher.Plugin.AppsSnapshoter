using System;

namespace Flow.Launcher.Plugin.SnapshotApps.Extensions;

public static class ResultBuilder
{
    public static Result WithTitle(this Result result, string title)
    {
        result.Title = title;
        return result;
    }
    
    public static Result WithSubtitle(this Result result, string subtitle)
    {
        result.SubTitle = subtitle;
        return result;
    }
    
    public static Result WithIconPath(this Result result, string iconPath)
    {
        result.IcoPath = iconPath;
        return result;
    }

    /// <summary>
    /// Return current result with actual action
    /// </summary>
    /// <param name="action"> Delegate accepting method that returns bool and accepts ActionContext </param>
    /// <returns></returns>
    public static Result WithFuncReturningBoolAction(this Result result, Func<ActionContext, bool> action)
    {
        result.Action = action;
        return result;
    }
}