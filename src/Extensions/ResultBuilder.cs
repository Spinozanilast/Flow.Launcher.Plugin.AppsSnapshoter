﻿using System;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.AppsSnapshoter.Extensions;

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

    public static Result WithSubtitleToolTip(this Result result, string subtitleToolTip)
    {
        result.SubTitleToolTip = subtitleToolTip;
        return result;
    }

    /// <summary>
    /// Return current result with actual action
    /// </summary>
    /// <param name="action"> Delegate accepting method that returns bool and accepts ActionContext </param>
    public static Result WithFuncReturningBoolAction(this Result result, Func<ActionContext, bool> action)
    {
        result.Action = action;
        return result;
    }

    /// <summary>
    /// Return current result with actual action asyn
    /// c
    /// </summary>
    /// <param name="action"> Delegate accepting method that returns ValueTask with bool result and accepts ActionContext </param>
    public static Result WithFuncReturningBoolActionAsync(this Result result,
        Func<ActionContext, ValueTask<bool>> action)
    {
        result.AsyncAction = action;
        return result;
    }
}