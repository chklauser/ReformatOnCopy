using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;
using DefaultNamespace;
using ReformatOnCopy;

var shouldExit = new ManualResetEventSlim();
Console.CancelKeyPress += (sender, e) =>
{
    shouldExit.Set();
};

var reformatter = new Reformatter();
var pump = new Thread(() =>
{
    var monitor = new SharpClipboard()
    {
        MonitoringEnabled = true
    };
    monitor.ClipboardChanged += (_, e) =>
    {
        Console.WriteLine($"Changed, source app name: {e.ProgramName}");
        // if(e.ProgramName.Contains("ReformatOnCopy", StringComparison.OrdinalIgnoreCase))
        // {
        //     Console.WriteLine("Ignoring self");
        //     return;
        // }
        Console.WriteLine($"{e.Text.Count(c => c == '\n')}<{e.Text}>");
        var reformatted = reformatter.Reformat(e.Text);
        Console.WriteLine($"{reformatted.Count(c => c == '\r')}>{reformatted}<");
//        Clipboard.SetText(reformatted);
        Console.WriteLine();
    };
    Application.Run(monitor);
}) { IsBackground = true, Name = "pump" };
pump.SetApartmentState(ApartmentState.STA);
pump.Start();

Console.WriteLine("Ctrl+C to exit");
shouldExit.Wait();
