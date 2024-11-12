using System;

namespace Flow.Launcher.Plugin.AppsSnapshoter.HandlesViewers;

public static class FileNameExtractFromHandles
{
    public static bool TryExtractFilenameFromHandleOutput(in string handleOutput, out string path)
    {
        path = null;

        var filenameIndex = handleOutput.IndexOf(":\\", StringComparison.Ordinal) - 1;
        if (filenameIndex < 0)
        {
            return false;
        }

        path = handleOutput.Substring(filenameIndex);
        return true;
    }
}