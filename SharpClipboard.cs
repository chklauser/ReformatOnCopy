using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace ReformatOnCopy;

public class SharpClipboard : Form
{
    /// <summary>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-addclipboardformatlistener">AddClipboardFormatListener</a>
    /// </summary>
    /// <param name="hwnd">A handle to the window to be placed in the clipboard format listener list.</param>
    /// <returns>Returns TRUE if successful, FALSE otherwise. Call GetLastError for additional details.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool AddClipboardFormatListener(nint hwnd);
    
    /// <summary>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-removeclipboardformatlistener">RemoveClipboardFormatListener</a>
    /// </summary>
    /// <param name="hwnd">A handle to the window to remove from the clipboard format listener list.</param>
    /// <returns>Returns TRUE if successful, FALSE otherwise. Call GetLastError for additional details.</returns>
    [DllImport("user32.dll", SetLastError = true)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool RemoveClipboardFormatListener(nint hwnd);
    
    /// <summary>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/api/winuser/nf-winuser-setparent">SetParent</a>
    /// </summary>
    /// <param name="hWndChild">A handle to the child window.</param>
    /// <param name="hWndNewParent">A handle to the new parent window. If this parameter is NULL, the desktop window becomes the new parent window. If this parameter is <see cref="HWND_MESSAGE"/>, the child window becomes a message-only window.</param>
    /// <returns></returns>
    [DllImport("user32.dll", SetLastError = true)]
    public static extern nint SetParent(nint hWndChild, nint hWndNewParent);
    
    //Reference https://www.pinvoke.net/default.aspx/Constants.HWND
    // ReSharper disable once InconsistentNaming
    public const nint HWND_MESSAGE = -3;
    
    /// <summary>
    /// <a href="https://docs.microsoft.com/en-us/windows/win32/dataxchg/wm-clipboardupdate">WM_CLIPBOARDUPDATE</a>
    /// </summary>
    // ReSharper disable once InconsistentNaming
    private const int WM_CLIPBOARDUPDATE = 0x031D;

    public record ClipboardChangedEventArgs(string Text, string? ProgramName);
    public event EventHandler<ClipboardChangedEventArgs>? ClipboardChanged; 


    public bool Ready { get; private set; }
    public bool MonitoringEnabled { get; set; }
    
    public SharpClipboard()
    {
        // Turn the window into a message-only window
        SetParent(Handle, HWND_MESSAGE);
        ShowIcon = false;
        ShowInTaskbar = false;
        WindowState = FormWindowState.Minimized;
        FormClosing += OnClose;
        Load += OnLoad;
        ResumeLayout(false);
    }

    protected override void WndProc(ref Message m)
    {
        base.WndProc(ref m);
        if(!Ready || !MonitoringEnabled) return;

        switch (m.Msg)
        {
            case WM_CLIPBOARDUPDATE when Ready:
                OnClipboardUpdate();
                break;
        }
    }

    private void OnClipboardUpdate(int remaining = 5)
    {
        if (Clipboard.GetDataObject()?.GetData(DataFormats.UnicodeText) is string text)
        {
            if (text == "" && remaining > 0)
            {
                Invoke(() => { OnClipboardUpdate(remaining-1); });
            }
            else
            {
                ClipboardChanged?.Invoke(this, new(text, getApplicationName()));
            }
        }
    }

    private void OnLoad(object? sender, EventArgs e)
    {
        // Start listening for clipboard changes.
        if (!AddClipboardFormatListener(Handle))
        {
            throw new("Failed to register hidden window as format listener.");
        }
        Hide();

        Ready = true;
    }

    private void OnClose(object? sender, FormClosingEventArgs e)
    {
        RemoveClipboardFormatListener(Handle); // can't really do anything if this fails

        Ready = false;
    }
    /// <summary>
    /// Modifications in this overriden method have
    /// been added to disable viewing of the handle-
    /// window in the Task Manager.
    /// </summary>
    protected override CreateParams CreateParams
    {
        get 
        {
            var cp = base.CreateParams;

            // Turn on WS_EX_TOOLWINDOW.
            cp.ExStyle |= 0x80;

            return cp;

        }
    }
    
    [DllImport("user32.dll")]
    private static extern int GetForegroundWindow();
    
    [DllImport("user32")]
    private static extern uint GetWindowThreadProcessId(nint hWnd, out int lpdwProcessId);
    
    private static int getProcessId(nint hwnd)
    {
        var _ = GetWindowThreadProcessId(hwnd, out var processId);

        return processId;
    }
    
    private static string? getApplicationName()
    {
        try
        {
            var hwnd = GetForegroundWindow();
            var process = Process.GetProcessById(getProcessId(hwnd));
            var executablePath = process.MainModule?.FileName;
            return executablePath != null ? Path.GetFileNameWithoutExtension(executablePath) : null;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine($"Failed to read process name: {e.Message}");
            return null;
        }
    }
}