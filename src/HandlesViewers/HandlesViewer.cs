using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Flow.Launcher.Plugin.SnapshotApps.HandlesViewers;

public class HandlesViewer
{
    private static string HandlesCliFilename = "handle.exe";

    private readonly ProcessStartInfo _handlesStartInfo;

    public HandlesViewer()
    {
        _handlesStartInfo = new ProcessStartInfo
        {
            FileName = HandlesCliFilename,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
    }

    public async Task<List<string>> GetOpenedPaths(int processId, IHandlesExplorer handlesExplorer)
    {
        var handles = await FindConcreteHandlesByLineEndAsync(processId);
        return handlesExplorer.GetPathsByHandles(handles, ExtractFilenameFromHandleOutput);
    }

    private async Task<HashSet<string>> FindConcreteHandlesByLineEndAsync(int processId)
    {
        ChangeHandlesArgs($"-p {processId}");
        var process = Process.Start(_handlesStartInfo);

        ArgumentNullException.ThrowIfNull(process, nameof(process));
        var handlesLines = new HashSet<string>();
        while (!process.StandardOutput.EndOfStream)
        {
            handlesLines.Add(await process.StandardOutput.ReadLineAsync());
        }
        
        
        return handlesLines;
    }

    private void ChangeHandlesArgs(string args) => _handlesStartInfo.Arguments = args;
    
    private string ExtractFilenameFromHandleOutput(string handleOutput)
    {
        var filenameIndex = handleOutput.IndexOf(":\\", StringComparison.Ordinal) - 1;
        return handleOutput.Substring(filenameIndex);
    }
}