using System;
using System.Drawing;
using System.Linq;
using System.Management;
using System.Windows.Forms;
using System.Diagnostics;

internal static class Program
{
    private const string VidPid = "VID_045E&PID_02D1";
    private static NotifyIcon tray = null!;
    private static ContextMenuStrip menu = null!;
    private static ToolStripMenuItem stateItem = null!;
    private static bool busy;

    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();

        tray = new NotifyIcon
        {
            Visible = true,
            Icon = SystemIcons.Application,
            Text = "Xbox Controller Toggle"
        };

        menu = new ContextMenuStrip();
        stateItem = new ToolStripMenuItem("Estado: comprobando…") { Enabled = false };

        var toggle = new ToolStripMenuItem("⏻ Alternar Xbox", null, (_, _) => Toggle());
        var enable = new ToolStripMenuItem("🟢 Activar Xbox", null, (_, _) => ChangeState(true));
        var disable = new ToolStripMenuItem("🔴 Desactivar Xbox", null, (_, _) => ChangeState(false));
        var refresh = new ToolStripMenuItem("🔄 Actualizar estado", null, (_, _) => UpdateStatus());
        var exit = new ToolStripMenuItem("❌ Salir", null, (_, _) => Application.Exit());

        menu.Items.Add(stateItem);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(toggle);
        menu.Items.Add(enable);
        menu.Items.Add(disable);
        menu.Items.Add(refresh);
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(exit);

        tray.ContextMenuStrip = menu;
        tray.MouseClick += (_, e) =>
        {
            if (e.Button == MouseButtons.Left) Toggle();
        };

        tray.DoubleClick += (_, _) => UpdateStatus();
        UpdateStatus();
        Application.Run();
    }

    private static ManagementObject? FindXbox()
    {
        using var searcher = new ManagementObjectSearcher(
            "SELECT PNPDeviceID, ConfigManagerErrorCode, Name FROM Win32_PnPEntity " +
            "WHERE PNPDeviceID LIKE '%" + VidPid + "%'");
        return searcher.Get().Cast<ManagementObject>().FirstOrDefault();
    }

    private static bool IsDisabled(ManagementObject dev)
    {
        int code = Convert.ToInt32(dev["ConfigManagerErrorCode"] ?? 0);
        return code == 22; // CM_PROB_DISABLED
    }

    private static void PnpChange(string command, string instanceId)
    {
        // The application itself requests elevation through the manifest.
        // Therefore the user gets UAC once when launching the app, not on every click.
        string escaped = instanceId.Replace("'", "''");
        string script = $"{command}-PnPDevice -InstanceId '{escaped}' -Confirm:$false";
        var psi = new ProcessStartInfo
        {
            FileName = "powershell.exe",
            Arguments = $"-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"{script}\"",
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true
        };

        using var p = Process.Start(psi)!;
        p.WaitForExit();
        if (p.ExitCode != 0)
        {
            string error = p.StandardError.ReadToEnd();
            throw new InvalidOperationException(string.IsNullOrWhiteSpace(error)
                ? $"PowerShell terminó con código {p.ExitCode}."
                : error.Trim());
        }
    }

    private static void ChangeState(bool enable)
    {
        if (busy) return;
        busy = true;
        try
        {
            using var dev = FindXbox();
            if (dev == null)
            {
                tray.ShowBalloonTip(2500, "Xbox Controller Toggle",
                    "No se encontró USB\\VID_045E&PID_02D1.", ToolTipIcon.Warning);
                return;
            }

            string id = (string)dev["PNPDeviceID"];
            PnpChange(enable ? "Enable" : "Disable", id);
            System.Threading.Thread.Sleep(500);
            UpdateStatus();
        }
        catch (Exception ex)
        {
            tray.ShowBalloonTip(3500, "Xbox Controller Toggle",
                "No se pudo cambiar el estado: " + ex.Message, ToolTipIcon.Error);
        }
        finally { busy = false; }
    }

    private static void Toggle()
    {
        try
        {
            using var dev = FindXbox();
            if (dev == null)
            {
                tray.ShowBalloonTip(2500, "Xbox Controller Toggle",
                    "No se encontró el mando Xbox.", ToolTipIcon.Warning);
                return;
            }
            ChangeState(IsDisabled(dev) == true);
        }
        catch { ChangeState(true); }
    }

    private static void UpdateStatus()
    {
        try
        {
            using var dev = FindXbox();
            if (dev == null)
            {
                stateItem.Text = "Estado: ⚪ Xbox no encontrado";
                tray.Text = "Xbox Controller Toggle — no encontrado";
                return;
            }

            bool disabled = IsDisabled(dev);
            stateItem.Text = disabled ? "Estado: 🔴 Desactivado" : "Estado: 🟢 Activado";
            tray.Text = disabled
                ? "Xbox Controller Toggle — Desactivado"
                : "Xbox Controller Toggle — Activado";
        }
        catch
        {
            stateItem.Text = "Estado: ⚪ Desconocido";
            tray.Text = "Xbox Controller Toggle — desconocido";
        }
    }
}
