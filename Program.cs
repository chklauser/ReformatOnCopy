﻿using System;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using JetBrains.Annotations;

namespace ReformatOnCopy;

[App]
public static class Program
{
    /// <summary default="true">
    /// Intercept clipboard copy from a particular program and reformat the copied text.
    /// </summary>
    /// <param name="program">Name of the process(es) (executable, without extension) to monitor.</param>
    /// <param name="ignore">Name of process(es) (executable, without extension) to ignore.</param>
    [UsedImplicitly]
    public static async Task<int> OnCopy(string[]? program = null, string[]? ignore = null)
    {
        if (program == null || program.Length == 0)
        {
            program = new[] { "SumatraPDF" };
        }
        if(ignore == null || ignore.Length == 0)
        {
            ignore = new[] { "WindowsTerminal" };
        }
        
        // Allow user to exit with Ctrl+C
        var shouldExit = new TaskCompletionSource();
        Console.CancelKeyPress += (_, _) =>
        {
            shouldExit.SetResult();
        };

        runInterceptorInBackground(program, ignore);

        Console.WriteLine("Ctrl+C to exit");
        await shouldExit.Task;
        return 0;
    }

    private static void runInterceptorInBackground(string[] program, string[] ignore)
    {
        var reFormatter = new Reformatter(Splittermond.Headings);
        
        // The clipboard monitor needs to run in an STA thread because it maintains a hidden window that
        // receives clipboard events.
        var pump = new Thread(() =>
        {
            bool isSelf(string? programName) =>
                programName != null && (
                    programName.Contains(Assembly.GetExecutingAssembly().GetName().Name ?? "ReformatOnCopy",
                        StringComparison.OrdinalIgnoreCase)
                    || ignore.Any(name => programName.Contains(name, StringComparison.OrdinalIgnoreCase))
                );

            var monitor = new SharpClipboard()
            {
                MonitoringEnabled = true
            };
            string? lastOutput = null;
            monitor.ClipboardChanged += (_, e) =>
            {
                // When we send the re-formatted text to the clipboard, windows will broadcast another
                // "clipboard changed" event. We need to ignore that event to avoid an infinite loop.
                // Unfortunately, the "program name" is only a heuristic. If the user switches programs quickly,
                // we might receive our own clipboard changed event under a different program name. To avoid that,
                // we also check that the clipboard content is not what we just re-formatted.
                if (isSelf(e.ProgramName) || e.Text == lastOutput)
                {
                    return;
                }

                lastOutput = intercept(e, reFormatter, program);
            };
            Application.Run(monitor);
        }) { IsBackground = true, Name = "pump" }; // background means that the thread will shut down when the app exits
        pump.SetApartmentState(ApartmentState.STA);
        pump.Start();
    }

    private static string? intercept(
        SharpClipboard.ClipboardChangedEventArgs e,
        Reformatter reFormatter,
        string[] program
    )
    {
        // Check whether the clipboard change came from one of the programs that we are monitoring.
        var programName = e.ProgramName;
        if (programName == null || !program.Any(name => programName.Contains(name, StringComparison.OrdinalIgnoreCase)))
        {
            Console.WriteLine(
                $"Ignoring copy from {programName ?? "untitled process"}. Not one of {string.Join(", ", program)}");
            return null;
        }

        // Re-format
        var numBreaksBefore = e.Text.Count(c => c == '\n');
        var reformatted = reFormatter.Reformat(e.Text);
        var numBreaksAfter = reformatted.Count(c => c == '\n');
        
        // Replace clipboard content with the re-formatted version
        Clipboard.SetText(reformatted);
        
        Console.WriteLine(
            $"re-formatted text copied from {programName} (line breaks {numBreaksBefore} -> {numBreaksAfter})");
        return reformatted;
    }
}