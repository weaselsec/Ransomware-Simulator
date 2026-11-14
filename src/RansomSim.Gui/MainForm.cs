using System.Diagnostics;
using RansomSim.Core;

namespace RansomSim.Gui;

public partial class MainForm : Form
{
    private CancellationTokenSource? _cts;
    private bool _busy;

    public MainForm()
    {
        InitializeComponent();

        txtPath.Text = DefaultLabPath();
        Log("RansomSim ready.");
        Log("SIMULATION ONLY: originals are read-only; encrypted copies get a .locked extension.");
        Log("Pick a target, preview the file list, then start the simulation.");
        Log(string.Empty);
    }

    // ------------------------------------------------------------------ helpers

    private static string DefaultLabPath() =>
        Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments),
            "RansomSimLab");

    private SimulationScope SelectedScope =>
        rdoRemote.Checked ? SimulationScope.RemoteShare : SimulationScope.LocalComputer;

    private void Log(string message)
    {
        if (txtLog.IsDisposed)
            return;

        if (txtLog.InvokeRequired)
        {
            txtLog.BeginInvoke(new Action<string>(Log), message);
            return;
        }

        txtLog.AppendText($"[{DateTime.Now:HH:mm:ss}] {message}{Environment.NewLine}");
    }

    private void SetStatus(string text)
    {
        if (lblStatus.IsDisposed)
            return;

        if (lblStatus.InvokeRequired)
        {
            lblStatus.BeginInvoke(new Action<string>(SetStatus), text);
            return;
        }

        lblStatus.Text = text;
    }

    private void SetBusy(bool busy)
    {
        _busy = busy;
        btnStart.Enabled = !busy;
        btnPreview.Enabled = !busy;
        btnRestore.Enabled = !busy;
        btnBrowse.Enabled = !busy;
        btnOpenFolder.Enabled = !busy;
        rdoLocal.Enabled = !busy;
        rdoRemote.Enabled = !busy;
        btnStop.Enabled = busy;

        progressBar.Style = busy ? ProgressBarStyle.Marquee : ProgressBarStyle.Blocks;
        if (!busy)
            progressBar.Value = 0;
    }

    /// <summary>Builds options from the form, surfacing validation problems to the user.</summary>
    private SimulationOptions? BuildOptionsFromForm()
    {
        var scope = SelectedScope;
        var path = txtPath.Text.Trim();

        if (string.IsNullOrWhiteSpace(path))
        {
            MessageBox.Show(this, "Choose a folder first.", "No folder",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        if (scope == SimulationScope.RemoteShare &&
            !path.StartsWith(@"\\", StringComparison.Ordinal) &&
            !path.StartsWith("//", StringComparison.Ordinal))
        {
            MessageBox.Show(this,
                "Remote share mode needs a UNC path, for example:" + Environment.NewLine + Environment.NewLine +
                @"\\fileserver\lab-share" + Environment.NewLine + Environment.NewLine +
                "Mapped drive letters (Z:\\) point at a local path and are rejected on purpose.",
                "Not a UNC path", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return null;
        }

        if (!Directory.Exists(path))
        {
            var answer = MessageBox.Show(this,
                $"Folder not reachable:{Environment.NewLine}{path}{Environment.NewLine}{Environment.NewLine}" +
                "Create it now?",
                "Folder missing", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
                return null;

            try
            {
                Directory.CreateDirectory(path);
                Log($"Created folder: {path}");
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, $"Could not create the folder:{Environment.NewLine}{ex.Message}",
                    "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        var options = new SimulationOptions
        {
            Scope = scope,
            RootPath = path,
            Recurse = chkRecurse.Checked,
            DryRun = chkDryRun.Checked,
            WriteRansomNote = chkWriteNote.Checked,
            DelayBetweenFilesMs = (int)numDelay.Value,
            MaxFiles = (int)numMaxFiles.Value,
            MaxFileSizeBytes = (long)numMaxSize.Value * 1024 * 1024,
            Passphrase = string.IsNullOrWhiteSpace(txtPass.Text) ? null : txtPass.Text,
            // 0 = no percentage guard: a dedicated lab folder is normally 100% in scope.
            // The CLI exposes --max-percent for operators who want that guard on a broad share.
            MaxPercentOfFiles = 0
        };

        foreach (var item in txtExclusions.Text.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries))
            options.Exclusions.Add(item.Trim());

        options.Exclusions.Add("*.locked");
        options.Exclusions.Add("*.ransomsim.json");
        options.Exclusions.Add("READ_ME_SIMULATION*.txt");

        return options;
    }

    // ------------------------------------------------------------------ events

    private void rdoScope_CheckedChanged(object? sender, EventArgs e)
    {
        if (sender == rdoRemote && rdoRemote.Checked)
        {
            lblHint.Text = @"Remote: UNC path, e.g. \\fileserver\lab-share" + Environment.NewLine +
                           "Uses the credentials of the account running this app.";
            if (!txtPath.Text.StartsWith(@"\\", StringComparison.Ordinal))
                txtPath.Text = @"\\";
        }
        else if (sender == rdoLocal && rdoLocal.Checked)
        {
            lblHint.Text = "Local: any folder on this computer." + Environment.NewLine +
                           "A dedicated lab folder is strongly recommended.";
            if (txtPath.Text == @"\\")
                txtPath.Text = DefaultLabPath();
        }
    }

    private void btnBrowse_Click(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = SelectedScope == SimulationScope.RemoteShare
                ? @"Pick a network share (UNC) to simulate against"
                : "Pick the folder to simulate against",
            UseDescriptionForTitle = true,
            ShowNewFolderButton = true
        };

        if (Directory.Exists(txtPath.Text))
            dialog.InitialDirectory = txtPath.Text;

        if (dialog.ShowDialog(this) == DialogResult.OK && !string.IsNullOrWhiteSpace(dialog.SelectedPath))
        {
            txtPath.Text = dialog.SelectedPath;
            Log($"Target set to: {txtPath.Text}");
        }
    }

    private async void btnPreview_Click(object? sender, EventArgs e)
    {
        var options = BuildOptionsFromForm();
        if (options is null)
            return;

        try
        {
            SetBusy(true);
            SetStatus("Scanning...");
            Log($"Preview ({options.Scope}): {options.RootPath}");

            var target = SimulationTarget.Create(options.Scope, options.RootPath);
            var files = await Task.Run(() => SimulationScanner.Collect(target, options));

            if (files.Count == 0)
            {
                Log("No eligible files found. Add a few test documents and try again.");
                SetStatus("No eligible files.");
                return;
            }

            long total = 0;
            foreach (var file in files)
            {
                try { total += new FileInfo(file).Length; } catch { /* ignore */ }
            }

            Log($"Eligible files: {files.Count:N0} ({FormatSize(total)})");
            foreach (var file in files.Take(25))
                Log($"   {Path.GetRelativePath(target.RootPath, file)}");

            if (files.Count > 25)
                Log($"   ... and {files.Count - 25:N0} more");

            SetStatus($"{files.Count:N0} file(s) eligible.");
        }
        catch (SimulationException ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Cannot use this target",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex}");
            MessageBox.Show(this, ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    private async void btnStart_Click(object? sender, EventArgs e)
    {
        if (_busy)
            return;

        var options = BuildOptionsFromForm();
        if (options is null)
            return;

        var confirm = MessageBox.Show(this,
            "About to run a ransomware SIMULATION against:" + Environment.NewLine + Environment.NewLine +
            $"Scope : {options.Scope}" + Environment.NewLine +
            $"Folder: {options.RootPath}" + Environment.NewLine +
            $"Mode  : {(options.DryRun ? "Dry run (writes nothing)" : "Simulate encryption")}" + Environment.NewLine + Environment.NewLine +
            "Originals will NOT be modified, renamed or deleted." + Environment.NewLine +
            "Encrypted .locked copies will be created next to each original." + Environment.NewLine + Environment.NewLine +
            "Continue?",
            "Confirm simulation", MessageBoxButtons.YesNo, MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (confirm != DialogResult.Yes)
            return;

        var simulator = new RansomSimulator();
        _cts = new CancellationTokenSource();
        simulator.CancellationToken = _cts.Token;

        simulator.Status += Log;
        simulator.FileProcessed += result =>
        {
            var prefix = result.Outcome switch
            {
                FileOutcome.Encrypted => "+",
                FileOutcome.Failed => "!",
                _ => "-"
            };
            Log($"  {prefix} {result.Path} [{FormatSize(result.SizeBytes)}] {result.Message}".TrimEnd());
        };

        try
        {
            SetBusy(true);
            SetStatus("Simulation running...");
            Log(string.Empty);
            Log($"=== Starting simulation ({options.Scope}) ===");

            var report = await Task.Run(() => simulator.Run(options));

            Log(string.Empty);
            Log($"Run ID     : {report.RunId}");
            Log($"Encrypted  : {report.EncryptedCount:N0}");
            Log($"Skipped    : {report.SkippedCount:N0}");
            Log($"Failed     : {report.FailedCount:N0}");
            Log($"Data       : {FormatSize(report.BytesProcessed)}");
            Log($"Duration   : {report.Duration.TotalSeconds:0.00}s");
            Log($"Rate       : {report.FilesPerSecond:0.0} files/s, {report.MegabytesPerSecond:0.00} MB/s");
            if (report.ManifestPath is not null)
                Log($"Manifest   : {report.ManifestPath}");
            if (report.RansomNotePath is not null)
                Log($"Note       : {report.RansomNotePath}");
            Log(report.DryRun
                ? "Dry run finished. Nothing was written."
                : "Originals untouched. Use Restore to remove the .locked copies.");
            Log("=== Simulation finished ===");

            lblCounts.Text =
                $"Last run : {report.RunId}{Environment.NewLine}" +
                $"Encrypted: {report.EncryptedCount:N0}   Failed: {report.FailedCount:N0}   " +
                $"{FormatSize(report.BytesProcessed)}";

            SetStatus(report.Success
                ? $"Finished: {report.EncryptedCount:N0} file(s) simulated in {report.Duration.TotalSeconds:0.0}s."
                : $"Finished with {report.FailedCount} error(s).");
        }
        catch (SimulationException ex)
        {
            Log($"ERROR: {ex.Message}");
            SetStatus("Simulation aborted.");
            MessageBox.Show(this, ex.Message, "Simulation aborted",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex}");
            SetStatus("Simulation failed.");
            MessageBox.Show(this, ex.Message, "Simulation failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            _cts?.Dispose();
            _cts = null;
            SetBusy(false);
        }
    }

    private void btnStop_Click(object? sender, EventArgs e)
    {
        if (_cts is null)
            return;

        Log("Stop requested - finishing the current file...");
        SetStatus("Stopping...");
        _cts.Cancel();
        btnStop.Enabled = false;
    }

    private async void btnRestore_Click(object? sender, EventArgs e)
    {
        var root = txtPath.Text.Trim();

        using var dialog = new OpenFileDialog
        {
            Title = "Select the run manifest to restore from",
            Filter = "RansomSim manifest (*.ransomsim.json)|*.ransomsim.json|All files (*.*)|*.*",
            CheckFileExists = true
        };

        if (Directory.Exists(root))
        {
            var latest = SimulationManifest.FindLatest(root);
            if (latest is not null)
            {
                dialog.InitialDirectory = Path.GetDirectoryName(latest);
                dialog.FileName = Path.GetFileName(latest);
            }
            else
            {
                dialog.InitialDirectory = root;
            }
        }

        if (dialog.ShowDialog(this) != DialogResult.OK)
            return;

        var simulator = new RansomSimulator();
        simulator.Status += Log;
        simulator.FileProcessed += r => Log(
            $"  {(r.Outcome == FileOutcome.Restored ? "+" : r.Outcome == FileOutcome.Failed ? "!" : "-")} {r.Path} {r.Message}".TrimEnd());

        try
        {
            // Runs started with a passphrase need it again to derive the restore key.
            string? passphrase = null;
            if (SimulationManifest.Load(dialog.FileName).KeyFromPassphrase)
            {
                passphrase = PromptForPassphrase(this);
                if (passphrase is null)
                {
                    Log("Restore cancelled (no passphrase entered).");
                    return;
                }
            }

            SetBusy(true);
            SetStatus("Restoring...");
            Log(string.Empty);
            Log($"=== Restoring from {Path.GetFileName(dialog.FileName)} ===");

            var report = await Task.Run(() => simulator.Restore(dialog.FileName, keepLockedFiles: false, passphrase: passphrase));

            Log($"Restored : {report.RestoredCount:N0}");
            Log($"Failed   : {report.FailedCount:N0}");
            Log($"Skipped  : {report.SkippedCount:N0}");
            Log("=== Restore finished ===");

            SetStatus($"Restored {report.RestoredCount:N0} file(s).");
        }
        catch (SimulationException ex)
        {
            Log($"ERROR: {ex.Message}");
            MessageBox.Show(this, ex.Message, "Restore failed",
                MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        catch (Exception ex)
        {
            Log($"ERROR: {ex}");
            MessageBox.Show(this, ex.Message, "Restore failed",
                MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            SetBusy(false);
        }
    }

    /// <summary>Asks for the passphrase needed by a passphrase-derived run.</summary>
    private static string? PromptForPassphrase(IWin32Window owner)
    {
        using var form = new Form
        {
            Width = 420,
            Height = 160,
            FormBorderStyle = FormBorderStyle.FixedDialog,
            StartPosition = FormStartPosition.CenterParent,
            Text = "Passphrase required",
            MaximizeBox = false,
            MinimizeBox = false
        };

        var label = new Label
        {
            Text = "This run derived its key from a passphrase.\nEnter the same passphrase to restore:",
            Location = new Point(12, 12),
            Size = new Size(380, 40)
        };

        var input = new TextBox
        {
            Location = new Point(12, 56),
            Size = new Size(280, 23),
            UseSystemPasswordChar = true
        };

        var ok = new Button
        {
            Text = "OK",
            DialogResult = DialogResult.OK,
            Location = new Point(200, 85),
            Size = new Size(90, 28)
        };

        var cancel = new Button
        {
            Text = "Cancel",
            DialogResult = DialogResult.Cancel,
            Location = new Point(300, 85),
            Size = new Size(90, 28)
        };

        form.Controls.AddRange(new Control[] { label, input, ok, cancel });
        form.AcceptButton = ok;
        form.CancelButton = cancel;

        return form.ShowDialog(owner) == DialogResult.OK && input.Text.Length > 0 ? input.Text : null;
    }

    private void btnOpenFolder_Click(object? sender, EventArgs e)
    {
        var path = txtPath.Text.Trim();
        if (!Directory.Exists(path))
        {
            MessageBox.Show(this, $"Folder does not exist:{Environment.NewLine}{path}",
                "Not found", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }

        try
        {
            Process.Start(new ProcessStartInfo { FileName = path, UseShellExecute = true });
        }
        catch (Exception ex)
        {
            Log($"ERROR opening folder: {ex.Message}");
        }
    }

    private static string FormatSize(long bytes)
    {
        string[] units = { "B", "KB", "MB", "GB", "TB" };
        double size = bytes;
        int unit = 0;
        while (size >= 1024 && unit < units.Length - 1)
        {
            size /= 1024;
            unit++;
        }
        return $"{size:0.#} {units[unit]}";
    }
}
