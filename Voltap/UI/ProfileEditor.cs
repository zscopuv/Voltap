using Voltap.Models;

namespace Voltap.UI;

internal sealed class ProfileEditor : Form
{
    private readonly TextBox name = new() { Width = 220 };
    private readonly ComboBox output = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly ComboBox mode = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 220 };
    private readonly TextBox startKey = new() { ReadOnly = true, Width = 220, BackColor = Color.FromArgb(29, 36, 49) };
    private readonly TextBox outputKey = new() { ReadOnly = true, Width = 220, BackColor = Color.FromArgb(29, 36, 49) };
    private readonly NumericUpDown cps = new() { Minimum = 1, Maximum = 250, Value = 10, Width = 220 };
    private readonly CheckBox random = new() { Text = "Enable CPS randomizer", AutoSize = true };
    private readonly NumericUpDown min = new() { Minimum = 1, Maximum = 250, Value = 5, Width = 100 };
    private readonly NumericUpDown max = new() { Minimum = 1, Maximum = 250, Value = 15, Width = 100 };
    private Keys capturedStart = Keys.F6, capturedOutput = Keys.Space;

    public ClickerProfile Result { get; private set; }

    public ProfileEditor(ClickerProfile? source = null)
    {
        Result = source?.Copy() ?? new ClickerProfile();
        capturedStart = Result.StartKey;
        capturedOutput = Result.OutputKey;
        Text = source is null ? "Create clicker" : "Edit clicker";
        StartPosition = FormStartPosition.CenterParent;
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        ClientSize = new Size(470, 590);
        BackColor = Color.FromArgb(18, 23, 33);
        ForeColor = Color.FromArgb(231, 236, 245);
        Font = new Font("Segoe UI", 9.5f);

        output.Items.AddRange(["Left mouse", "Right mouse", "Middle mouse", "Keystroke"]);
        mode.Items.AddRange(["Toggle", "Hold"]);
        output.SelectedIndex = (int)Result.Output;
        mode.SelectedIndex = (int)Result.Mode;
        name.Text = Result.Name;
        cps.Value = Result.Cps;
        random.Checked = Result.RandomizerEnabled;
        min.Value = Result.RandomMin;
        max.Value = Result.RandomMax;
        RefreshKeyText();

        var layout = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(24), ColumnCount = 2, RowCount = 10 };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 38));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 62));
        for (int i = 0; i < 10; i++) layout.RowStyles.Add(new RowStyle(SizeType.Absolute, i == 8 ? 54 : 47));
        Controls.Add(layout);

        AddRow(layout, 0, "Name", name);
        AddRow(layout, 1, "Output", output);
        AddRow(layout, 2, "Activation", mode);
        AddRow(layout, 3, "Start key", startKey);
        AddRow(layout, 4, "Keystroke key", outputKey);
        AddRow(layout, 5, "Fixed CPS", cps);
        AddRow(layout, 6, "Randomizer", random);
        var range = new FlowLayoutPanel { AutoSize = true, FlowDirection = FlowDirection.LeftToRight, WrapContents = false };
        range.Controls.Add(new Label { Text = "Min", AutoSize = true, Padding = new Padding(0, 8, 5, 0) });
        range.Controls.Add(min);
        range.Controls.Add(new Label { Text = "Max", AutoSize = true, Padding = new Padding(12, 8, 5, 0) });
        range.Controls.Add(max);
        AddRow(layout, 7, "Random range", range);

        var note = new Label
        {
            Text = "Each clicker runs independently. Maximum rate: 250 CPS per instance.",
            ForeColor = Color.FromArgb(145, 157, 176), AutoSize = true, MaximumSize = new Size(400, 0)
        };
        layout.Controls.Add(note, 0, 8);
        layout.SetColumnSpan(note, 2);

        var buttons = new FlowLayoutPanel { Dock = DockStyle.Fill, FlowDirection = FlowDirection.RightToLeft };
        var save = MakeButton("Save", Color.FromArgb(48, 119, 255));
        var cancel = MakeButton("Cancel", Color.FromArgb(42, 51, 67));
        save.Click += (_, _) => SaveAndClose();
        cancel.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        buttons.Controls.Add(save);
        buttons.Controls.Add(cancel);
        layout.Controls.Add(buttons, 0, 9);
        layout.SetColumnSpan(buttons, 2);

        startKey.KeyDown += (_, e) => { e.SuppressKeyPress = true; e.Handled = true; if (IsUsableKey(e.KeyCode)) { capturedStart = e.KeyCode; RefreshKeyText(); } };
        outputKey.KeyDown += (_, e) => { e.SuppressKeyPress = true; e.Handled = true; if (IsUsableKey(e.KeyCode)) { capturedOutput = e.KeyCode; RefreshKeyText(); } };
        output.SelectedIndexChanged += (_, _) => UpdateOutputState();
        random.CheckedChanged += (_, _) => UpdateRandomState();
        min.ValueChanged += (_, _) => { if (max.Value < min.Value) max.Value = min.Value; };
        max.ValueChanged += (_, _) => { if (max.Value < min.Value) min.Value = max.Value; };
        UpdateOutputState();
        UpdateRandomState();
    }

    private static bool IsUsableKey(Keys key) => key != Keys.None && key != Keys.ControlKey &&
        key != Keys.ShiftKey && key != Keys.Menu && key != Keys.LWin && key != Keys.RWin;

    private void RefreshKeyText()
    {
        startKey.Text = capturedStart.ToString();
        outputKey.Text = capturedOutput.ToString();
    }

    private void UpdateOutputState() => outputKey.Enabled = output.SelectedIndex == (int)ClickOutput.Keystroke;
    private void UpdateRandomState()
    {
        min.Enabled = max.Enabled = random.Checked;
        cps.Enabled = !random.Checked;
    }

    private void SaveAndClose()
    {
        if (string.IsNullOrWhiteSpace(name.Text))
        {
            MessageBox.Show(this, "Enter a name for this clicker.", "Voltap", MessageBoxButtons.OK, MessageBoxIcon.Information);
            return;
        }
        Result.Name = name.Text.Trim();
        Result.Output = (ClickOutput)output.SelectedIndex;
        Result.Mode = (ActivationMode)mode.SelectedIndex;
        Result.StartKey = capturedStart;
        Result.OutputKey = capturedOutput;
        Result.Cps = (int)cps.Value;
        Result.RandomizerEnabled = random.Checked;
        Result.RandomMin = (int)min.Value;
        Result.RandomMax = (int)max.Value;
        DialogResult = DialogResult.OK;
        Close();
    }

    private static void AddRow(TableLayoutPanel panel, int row, string label, Control control)
    {
        panel.Controls.Add(new Label { Text = label, AutoSize = true, Anchor = AnchorStyles.Left, ForeColor = Color.FromArgb(164, 176, 194) }, 0, row);
        control.Anchor = AnchorStyles.Left;
        panel.Controls.Add(control, 1, row);
    }

    private static Button MakeButton(string text, Color color) => new()
    {
        Text = text, Width = 104, Height = 36, FlatStyle = FlatStyle.Flat,
        BackColor = color, ForeColor = Color.White, FlatAppearance = { BorderSize = 0 },
        Cursor = Cursors.Hand, Margin = new Padding(8, 5, 0, 0)
    };
}