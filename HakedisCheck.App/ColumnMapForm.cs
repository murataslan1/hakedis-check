using HakedisCheck.Core.Config;
using HakedisCheck.Core.Excel;
using HakedisCheck.Core.Models;

namespace HakedisCheck.App;

public sealed class ColumnMapForm : Form
{
    private readonly WorkbookPreview _preview;
    private readonly ColumnProfile _profile;
    private readonly Dictionary<LogicalField, ComboBox> _fieldSelectors = [];

    private readonly TextBox _profileNameTextBox = new() { Dock = DockStyle.Fill };
    private readonly CheckedListBox _sheetList = new() { CheckOnClick = true, Dock = DockStyle.Fill, Height = 120 };
    private readonly NumericUpDown _headerRowInput = new() { Minimum = 1, Maximum = 20, Dock = DockStyle.Left, Width = 80 };
    private readonly NumericUpDown _firstDataRowInput = new() { Minimum = 1, Maximum = 50, Dock = DockStyle.Left, Width = 80 };
    private readonly TextBox _previewTextBox = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Both };
    private readonly CheckBox _saveProfileCheckBox = new() { Text = "Profili JSON olarak kaydet", AutoSize = true };
    private readonly TableLayoutPanel _mappingTable = new() { Dock = DockStyle.Top, AutoSize = true, ColumnCount = 2 };

    public ColumnMapForm(ExcelFileKind kind, WorkbookPreview preview, ColumnProfile profile)
    {
        _preview = preview;
        _profile = profile.Clone();

        Text = $"{kind.GetDisplayName()} Kolon Eşleme";
        Width = 920;
        Height = 720;
        StartPosition = FormStartPosition.CenterParent;

        BuildLayout(kind);
        LoadProfile(kind);
        _sheetList.ItemCheck += (_, _) => BeginInvoke(RefreshHeaderChoices);
    }

    public ColumnProfile ResultProfile => _profile.Clone();

    public bool SaveRequested => _saveProfileCheckBox.Checked;

    private void BuildLayout(ExcelFileKind kind)
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(12),
            AutoScroll = true
        };

        root.Controls.Add(CreateLabeledRow("Profil Adı", _profileNameTextBox));
        root.Controls.Add(CreateSheetSelector());
        root.Controls.Add(CreateRowSettings());

        var mappingGroup = new GroupBox
        {
            Text = "Mantıksal Alanlar",
            Dock = DockStyle.Top,
            AutoSize = true
        };
        mappingGroup.Controls.Add(_mappingTable);
        root.Controls.Add(mappingGroup);

        var previewGroup = new GroupBox
        {
            Text = "Önizleme",
            Dock = DockStyle.Fill,
            Height = 220
        };
        previewGroup.Controls.Add(_previewTextBox);
        root.Controls.Add(previewGroup);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Bottom,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        var okButton = new Button { Text = "Tamam", AutoSize = true };
        okButton.Click += (_, _) => Confirm();
        var cancelButton = new Button { Text = "İptal", AutoSize = true };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        buttonPanel.Controls.Add(okButton);
        buttonPanel.Controls.Add(cancelButton);
        buttonPanel.Controls.Add(_saveProfileCheckBox);
        root.Controls.Add(buttonPanel);

        Controls.Add(root);

        _headerRowInput.ValueChanged += (_, _) => RefreshHeaderChoices();
        _firstDataRowInput.ValueChanged += (_, _) => UpdatePreviewText();

        foreach (var field in ProfileSchema.GetFields(kind))
        {
            var label = new Label
            {
                Text = ProfileSchema.GetDisplayName(field),
                AutoSize = true,
                Anchor = AnchorStyles.Left
            };
            var selector = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Dock = DockStyle.Fill
            };

            _fieldSelectors[field] = selector;
            _mappingTable.RowStyles.Add(new RowStyle(SizeType.AutoSize));
            _mappingTable.Controls.Add(label);
            _mappingTable.Controls.Add(selector);
        }
    }

    private void LoadProfile(ExcelFileKind kind)
    {
        _profileNameTextBox.Text = _profile.ProfileName;
        foreach (var worksheetName in _preview.WorksheetNames)
        {
            var index = _sheetList.Items.Add(worksheetName);
            var shouldCheck = _profile.SelectedSheets.Count == 0 || _profile.SelectedSheets.Contains(worksheetName);
            _sheetList.SetItemChecked(index, shouldCheck);
        }

        _headerRowInput.Value = _profile.HeaderRowIndex;
        _firstDataRowInput.Value = _profile.FirstDataRowIndex;

        RefreshHeaderChoices();

        foreach (var field in ProfileSchema.GetFields(kind))
        {
            var selector = _fieldSelectors[field];
            var mappedHeader = _profile.GetMappedHeader(field);
            if (mappedHeader is not null && selector.Items.Contains(mappedHeader))
            {
                selector.SelectedItem = mappedHeader;
            }
        }

        UpdatePreviewText();
    }

    private Control CreateSheetSelector()
    {
        var group = new GroupBox
        {
            Text = "Sayfalar",
            Dock = DockStyle.Top,
            Height = 160
        };
        group.Controls.Add(_sheetList);
        return group;
    }

    private Control CreateRowSettings()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true
        };

        panel.Controls.Add(new Label { Text = "Başlık Satırı", AutoSize = true, Padding = new Padding(0, 8, 0, 0) });
        panel.Controls.Add(_headerRowInput);
        panel.Controls.Add(new Label { Text = "İlk Veri Satırı", AutoSize = true, Padding = new Padding(16, 8, 0, 0) });
        panel.Controls.Add(_firstDataRowInput);

        return panel;
    }

    private static Control CreateLabeledRow(string labelText, Control control)
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        panel.Controls.Add(new Label { Text = labelText, AutoSize = true, Padding = new Padding(0, 8, 8, 0) }, 0, 0);
        panel.Controls.Add(control, 1, 0);
        return panel;
    }

    private void RefreshHeaderChoices()
    {
        var worksheet = GetReferenceWorksheet();
        var headers = worksheet?.GetHeaders((int)_headerRowInput.Value) ?? Array.Empty<string>();
        var headerOptions = new[] { "(yok)" }
            .Concat(headers.Where(header => !string.IsNullOrWhiteSpace(header)).Distinct(StringComparer.OrdinalIgnoreCase))
            .ToArray();

        foreach (var selector in _fieldSelectors.Values)
        {
            var currentSelection = selector.SelectedItem?.ToString();
            selector.BeginUpdate();
            selector.Items.Clear();
            selector.Items.AddRange(headerOptions);
            selector.SelectedItem = currentSelection is not null && headerOptions.Contains(currentSelection)
                ? currentSelection
                : "(yok)";
            selector.EndUpdate();
        }

        UpdatePreviewText();
    }

    private WorksheetPreview? GetReferenceWorksheet()
    {
        var checkedSheetName = _sheetList.CheckedItems.Cast<string>().FirstOrDefault();
        return checkedSheetName is null
            ? _preview.Worksheets.FirstOrDefault()
            : _preview.FindWorksheet(checkedSheetName);
    }

    private void UpdatePreviewText()
    {
        var worksheet = GetReferenceWorksheet();
        if (worksheet is null)
        {
            _previewTextBox.Text = "Önizleme yok.";
            return;
        }

        _previewTextBox.Text = worksheet.ToMultilinePreview()
            + Environment.NewLine
            + Environment.NewLine
            + $"Başlık satırı: {_headerRowInput.Value}, İlk veri satırı: {_firstDataRowInput.Value}";
    }

    private void Confirm()
    {
        var selectedSheets = _sheetList.CheckedItems.Cast<string>().ToList();
        if (selectedSheets.Count == 0)
        {
            MessageBox.Show(this, "En az bir sayfa seçmelisiniz.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _profile.ProfileName = string.IsNullOrWhiteSpace(_profileNameTextBox.Text)
            ? $"{_profile.FileKind.GetDisplayName()} Profil"
            : _profileNameTextBox.Text.Trim();
        _profile.SelectedSheets = selectedSheets;
        _profile.HeaderRowIndex = (int)_headerRowInput.Value;
        _profile.FirstDataRowIndex = (int)_firstDataRowInput.Value;

        foreach (var (field, selector) in _fieldSelectors)
        {
            _profile.ColumnMappings[field] = selector.SelectedItem?.ToString() switch
            {
                "(yok)" or null => null,
                var value => value
            };
        }

        DialogResult = DialogResult.OK;
    }
}
