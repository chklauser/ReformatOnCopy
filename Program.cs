using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using ReformatOnCopy;

var shouldExit = new ManualResetEventSlim();
Console.CancelKeyPress += (sender, e) =>
{
    shouldExit.Set();
};

bool isSelf(string? programName) =>
    programName != null && (
        programName.Contains("ReformatOnCopy", StringComparison.OrdinalIgnoreCase) 
        || programName.Contains("WindowsTerminal", StringComparison.OrdinalIgnoreCase)
    );

var reformatter = new Reformatter();
var pump = new Thread(() =>
{
    var monitor = new SharpClipboard()
    {
        MonitoringEnabled = true
    };
    string? lastOutput = null;
    monitor.ClipboardChanged += (_, e) =>
    {
        Console.WriteLine($"Changed, source app name: {e.ProgramName}");
        if(isSelf(e.ProgramName) || e.Text == lastOutput)
        {
            Console.WriteLine("Ignoring self");
            return;
        }
        if(!(e.ProgramName?.Contains("SumatraPDF", StringComparison.OrdinalIgnoreCase) ?? false))
        {
            Console.WriteLine("Not SumatraPDF");
            return;
        }
        Console.WriteLine($"{e.Text.Count(c => c == '\n')}<{e.Text}>");
        var reformatted = reformatter.Reformat(e.Text);
        Console.WriteLine($"{reformatted.Count(c => c == '\n')}>{reformatted}<");
        Clipboard.SetText(reformatted);
        lastOutput = reformatted;
        Console.WriteLine();
    };
    Application.Run(monitor);
}) { IsBackground = true, Name = "pump" };
pump.SetApartmentState(ApartmentState.STA);
pump.Start();

Console.WriteLine("Ctrl+C to exit");
shouldExit.Wait();
