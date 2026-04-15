using HakedisCheck.Core.Config;
using HakedisCheck.Core.Excel;
using HakedisCheck.Core.Models;
using HakedisCheck.Core.Processing;
using HakedisCheck.Core.Utilities;

namespace HakedisCheck.App;

public sealed class MainForm : Form
{
    private readonly ValidationService _validationService = new();
    private readonly ProfileStore _profileStore = new();
    private readonly ReportExporter _reportExporter = new();
    private readonly Dictionary<ExcelFileKind, ColumnProfile> _profiles = [];
    private readonly Dictionary<ExcelFileKind, WorkbookPreview> _previews = [];

    private readonly TextBox _leavePathTextBox = CreatePathTextBox();
    private readonly TextBox _mesaiPathTextBox = CreatePathTextBox();
    private readonly TextBox _hakedisPathTextBox = CreatePathTextBox();
    private readonly Label _leaveProfileLabel = CreateProfileLabel();
    private readonly Label _mesaiProfileLabel = CreateProfileLabel();
    private readonly Label _hakedisProfileLabel = CreateProfileLabel();
    private readonly ComboBox _hakedisSheetComboBox = new() { Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList };
    private readonly DataGridView _resultGrid = new() { Dock = DockStyle.Fill, ReadOnly = true, AutoGenerateColumns = true, AllowUserToAddRows = false };
    private readonly TextBox _warningsTextBox = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical };
    private readonly Label _statusLabel = new() { Dock = DockStyle.Fill, AutoSize = true, Padding = new Padding(0, 8, 0, 0) };
    private readonly Button _exportButton = new() { Text = "Excel Raporu", AutoSize = true, Enabled = false };

    private ValidationRunResult? _lastResult;

    public MainForm()
    {
        Text = "Hakediş Doğrulama";
        Width = 1320;
        Height = 900;
        StartPosition = FormStartPosition.CenterScreen;

        BuildLayout();
        _resultGrid.DataBindingComplete += (_, _) => ApplyGridStyles();
    }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
            Padding = new Padding(12)
        };
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));

        root.Controls.Add(CreateFileGroup(
            "İzin Dosyası",
            _leavePathTextBox,
            _leaveProfileLabel,
            () => BrowseFile(ExcelFileKind.Leave),
            () => EditMapping(ExcelFileKind.Leave),
            () => OpenProfiles(ExcelFileKind.Leave)));

        root.Controls.Add(CreateFileGroup(
            "Mesai/Puantaj Dosyası",
            _mesaiPathTextBox,
            _mesaiProfileLabel,
            () => BrowseFile(ExcelFileKind.Mesai),
            () => EditMapping(ExcelFileKind.Mesai),
            () => OpenProfiles(ExcelFileKind.Mesai)));

        root.Controls.Add(CreateFileGroup(
            "Hakediş Dosyası",
            _hakedisPathTextBox,
            _hakedisProfileLabel,
            () => BrowseFile(ExcelFileKind.Hakedis),
            () => EditMapping(ExcelFileKind.Hakedis),
            () => OpenProfiles(ExcelFileKind.Hakedis),
            CreateHakedisSheetRow()));

        root.Controls.Add(CreateActionBar());
        root.Controls.Add(CreateResultsArea());

        Controls.Add(root);
    }

    private static TextBox CreatePathTextBox() => new() { Dock = DockStyle.Fill, ReadOnly = true };

    private static Label CreateProfileLabel() => new()
    {
        Dock = DockStyle.Fill,
        AutoSize = true,
        Text = "Profil: otomatik öneri yok"
    };

    private Control CreateFileGroup(
        string title,
        TextBox pathTextBox,
        Label profileLabel,
        Action browseAction,
        Action editAction,
        Action profileAction,
        Control? extraRow = null)
    {
        var group = new GroupBox
        {
            Text = title,
            Dock = DockStyle.Top,
            AutoSize = true
        };

        var layout = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 5,
            RowCount = extraRow is null ? 2 : 3,
            AutoSize = true
        };
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        layout.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));

        var browseButton = new Button { Text = "Seç", AutoSize = true };
        browseButton.Click += (_, _) => browseAction();

        var editButton = new Button { Text = "Kolon Eşleme", AutoSize = true };
        editButton.Click += (_, _) => editAction();

        var profilesButton = new Button { Text = "Profiller", AutoSize = true };
        profilesButton.Click += (_, _) => profileAction();

        layout.Controls.Add(pathTextBox, 0, 0);
        layout.Controls.Add(browseButton, 1, 0);
        layout.Controls.Add(editButton, 2, 0);
        layout.Controls.Add(profilesButton, 3, 0);

        layout.SetColumnSpan(profileLabel, 4);
        layout.Controls.Add(profileLabel, 0, 1);

        if (extraRow is not null)
        {
            layout.SetColumnSpan(extraRow, 4);
            layout.Controls.Add(extraRow, 0, 2);
        }

        group.Controls.Add(layout);
        return group;
    }

    private Control CreateHakedisSheetRow()
    {
        var row = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            ColumnCount = 2,
            AutoSize = true
        };
        row.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        row.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        row.Controls.Add(new Label { Text = "Ay / Sayfa", AutoSize = true, Padding = new Padding(0, 8, 8, 0) }, 0, 0);
        row.Controls.Add(_hakedisSheetComboBox, 1, 0);
        return row;
    }

    private Control CreateActionBar()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true
        };

        var runButton = new Button { Text = "Kontrolü Çalıştır", AutoSize = true };
        runButton.Click += (_, _) => RunValidation();

        _exportButton.Click += (_, _) => ExportResult();

        panel.Controls.Add(runButton);
        panel.Controls.Add(_exportButton);
        panel.Controls.Add(_statusLabel);
        return panel;
    }

    private Control CreateResultsArea()
    {
        var split = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 520
        };

        var resultGroup = new GroupBox
        {
            Text = "Sonuçlar",
            Dock = DockStyle.Fill
        };
        resultGroup.Controls.Add(_resultGrid);
        split.Panel1.Controls.Add(resultGroup);

        var warningGroup = new GroupBox
        {
            Text = "Uyarılar",
            Dock = DockStyle.Fill
        };
        warningGroup.Controls.Add(_warningsTextBox);
        split.Panel2.Controls.Add(warningGroup);

        return split;
    }

    private void BrowseFile(ExcelFileKind kind)
    {
        using var dialog = new OpenFileDialog
        {
            Filter = "Excel Dosyaları|*.xlsx;*.xlsm|Tüm Dosyalar|*.*"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        var pathTextBox = GetPathTextBox(kind);
        pathTextBox.Text = dialog.FileName;

        try
        {
            var preview = _validationService.PreviewWorkbook(dialog.FileName);
            _previews[kind] = preview;
            _profiles[kind] = _validationService.CreateSuggestedProfile(kind, dialog.FileName);
            UpdateProfileLabel(kind);

            if (kind == ExcelFileKind.Hakedis)
            {
                PopulateHakedisSheets(preview, _profiles[kind].SelectedSheets.FirstOrDefault());
            }
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Dosya Okuma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void EditMapping(ExcelFileKind kind)
    {
        if (!EnsurePreview(kind))
        {
            return;
        }

        var preview = _previews[kind];
        var profile = EnsureProfile(kind);
        if (kind == ExcelFileKind.Hakedis && _hakedisSheetComboBox.SelectedItem is string selectedSheet)
        {
            profile.SelectedSheets = [selectedSheet];
        }

        using var form = new ColumnMapForm(kind, preview, profile);
        if (form.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _profiles[kind] = form.ResultProfile;
        UpdateProfileLabel(kind);

        if (form.SaveRequested)
        {
            _profileStore.Save(_profiles[kind]);
        }

        if (kind == ExcelFileKind.Hakedis)
        {
            PopulateHakedisSheets(preview, _profiles[kind].SelectedSheets.FirstOrDefault());
        }
    }

    private void OpenProfiles(ExcelFileKind kind)
    {
        using var form = new ProfileManagerForm(_profileStore, kind);
        if (form.ShowDialog(this) != DialogResult.OK || form.SelectedProfile is null)
        {
            return;
        }

        _profiles[kind] = form.SelectedProfile;
        UpdateProfileLabel(kind);

        if (kind == ExcelFileKind.Hakedis && _previews.TryGetValue(kind, out var preview))
        {
            PopulateHakedisSheets(preview, _profiles[kind].SelectedSheets.FirstOrDefault());
        }
    }

    private void RunValidation()
    {
        if (!ValidateInputs())
        {
            return;
        }

        try
        {
            UseWaitCursor = true;
            Cursor = Cursors.WaitCursor;

            var hakedisProfile = EnsureProfile(ExcelFileKind.Hakedis);
            if (_hakedisSheetComboBox.SelectedItem is string selectedSheet)
            {
                hakedisProfile.SelectedSheets = [selectedSheet];
            }

            _lastResult = _validationService.Run(new ValidationRunOptions
            {
                LeaveFilePath = _leavePathTextBox.Text,
                MesaiFilePath = _mesaiPathTextBox.Text,
                HakedisFilePath = _hakedisPathTextBox.Text,
                LeaveProfile = EnsureProfile(ExcelFileKind.Leave),
                MesaiProfile = EnsureProfile(ExcelFileKind.Mesai),
                HakedisProfile = hakedisProfile
            });

            BindResults(_lastResult);
            _exportButton.Enabled = true;
        }
        catch (Exception exception)
        {
            MessageBox.Show(this, exception.Message, "Çalıştırma Hatası", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        finally
        {
            UseWaitCursor = false;
            Cursor = Cursors.Default;
        }
    }

    private void ExportResult()
    {
        if (_lastResult is null)
        {
            return;
        }

        using var dialog = new SaveFileDialog
        {
            Filter = "Excel Dosyası|*.xlsx",
            FileName = $"hakediş-raporu-{DateTime.Now:yyyyMMdd-HHmm}.xlsx"
        };

        if (dialog.ShowDialog(this) != DialogResult.OK)
        {
            return;
        }

        _reportExporter.Export(_lastResult, dialog.FileName);
        MessageBox.Show(this, "Rapor oluşturuldu.", "Tamamlandı", MessageBoxButtons.OK, MessageBoxIcon.Information);
    }

    private void BindResults(ValidationRunResult result)
    {
        var gridRows = result.Rows
            .Select(row => new ResultGridRow
            {
                EmployeeName = row.EmployeeName,
                IdentityNumber = row.IdentityNumber ?? string.Empty,
                CheckName = row.CheckName,
                Status = row.Status.GetDisplayName(),
                ExpectedValue = FormatNumber(row.ExpectedValue),
                ActualValue = FormatNumber(row.ActualValue),
                Difference = FormatNumber(row.Difference),
                Description = row.Description
            })
            .ToList();

        _resultGrid.DataSource = gridRows;
        _warningsTextBox.Text = result.Warnings.Count == 0
            ? "Uyarı yok."
            : string.Join(
                Environment.NewLine,
                result.Warnings.Select(warning =>
                    $"{warning.FileKind.GetDisplayName()} | {warning.SheetName} | Satır {warning.RowNumber}: {warning.Message}"));

        _statusLabel.Text =
            $"İzin: {result.LeaveEntryCount} satır, Mesai: {result.MesaiEntryCount} satır, Hakediş: {result.HakedisEntryCount} satır | " +
            $"OK: {result.OkCount}, HATA: {result.ErrorCount}, EKSIK: {result.MissingCount}";
    }

    private void ApplyGridStyles()
    {
        foreach (DataGridViewRow row in _resultGrid.Rows)
        {
            var status = row.Cells[nameof(ResultGridRow.Status)].Value?.ToString();
            row.DefaultCellStyle.BackColor = status switch
            {
                "OK" => Color.Honeydew,
                "HATA" => Color.MistyRose,
                "EKSIK" => Color.LemonChiffon,
                _ => Color.White
            };
        }

        _resultGrid.AutoResizeColumns();
    }

    private bool ValidateInputs()
    {
        var missing = new List<string>();

        if (!File.Exists(_leavePathTextBox.Text))
        {
            missing.Add("İzin dosyası");
        }

        if (!File.Exists(_mesaiPathTextBox.Text))
        {
            missing.Add("Mesai dosyası");
        }

        if (!File.Exists(_hakedisPathTextBox.Text))
        {
            missing.Add("Hakediş dosyası");
        }

        foreach (var kind in Enum.GetValues<ExcelFileKind>())
        {
            if (!_profiles.ContainsKey(kind))
            {
                missing.Add($"{kind.GetDisplayName()} profili");
            }
        }

        if (_hakedisSheetComboBox.Items.Count > 0 && _hakedisSheetComboBox.SelectedItem is null)
        {
            missing.Add("Hakediş ay sayfası");
        }

        if (missing.Count == 0)
        {
            return true;
        }

        MessageBox.Show(
            this,
            "Eksik alanlar:" + Environment.NewLine + string.Join(Environment.NewLine, missing),
            "Eksik Bilgi",
            MessageBoxButtons.OK,
            MessageBoxIcon.Warning);

        return false;
    }

    private bool EnsurePreview(ExcelFileKind kind)
    {
        if (_previews.ContainsKey(kind))
        {
            return true;
        }

        var path = GetPathTextBox(kind).Text;
        if (!File.Exists(path))
        {
            MessageBox.Show(this, $"{kind.GetDisplayName()} dosyası seçilmedi.", "Eksik Bilgi", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return false;
        }

        _previews[kind] = _validationService.PreviewWorkbook(path);
        return true;
    }

    private ColumnProfile EnsureProfile(ExcelFileKind kind)
    {
        if (_profiles.TryGetValue(kind, out var profile))
        {
            return profile.Clone();
        }

        var path = GetPathTextBox(kind).Text;
        profile = _validationService.CreateSuggestedProfile(kind, path);
        _profiles[kind] = profile;
        UpdateProfileLabel(kind);
        return profile.Clone();
    }

    private void PopulateHakedisSheets(WorkbookPreview preview, string? selectedSheet)
    {
        var sheets = preview.WorksheetNames
            .Where(name => !TextUtilities.NormalizeForLookup(name).Contains("BIRIM FIYATLAR", StringComparison.Ordinal))
            .ToArray();

        _hakedisSheetComboBox.Items.Clear();
        _hakedisSheetComboBox.Items.AddRange(sheets);

        if (sheets.Length == 0)
        {
            return;
        }

        _hakedisSheetComboBox.SelectedItem = selectedSheet is not null && sheets.Contains(selectedSheet)
            ? selectedSheet
            : sheets[0];
    }

    private void UpdateProfileLabel(ExcelFileKind kind)
    {
        if (!_profiles.TryGetValue(kind, out var profile))
        {
            return;
        }

        var text = $"Profil: {profile.ProfileName} | Başlık: {profile.HeaderRowIndex} | Veri: {profile.FirstDataRowIndex}";
        switch (kind)
        {
            case ExcelFileKind.Leave:
                _leaveProfileLabel.Text = text;
                break;
            case ExcelFileKind.Mesai:
                _mesaiProfileLabel.Text = text;
                break;
            case ExcelFileKind.Hakedis:
                _hakedisProfileLabel.Text = text;
                break;
        }
    }

    private TextBox GetPathTextBox(ExcelFileKind kind) => kind switch
    {
        ExcelFileKind.Leave => _leavePathTextBox,
        ExcelFileKind.Mesai => _mesaiPathTextBox,
        ExcelFileKind.Hakedis => _hakedisPathTextBox,
        _ => throw new ArgumentOutOfRangeException(nameof(kind), kind, null)
    };

    private static string FormatNumber(decimal? value) => value?.ToString("0.####") ?? string.Empty;

    private sealed class ResultGridRow
    {
        public string EmployeeName { get; init; } = string.Empty;
        public string IdentityNumber { get; init; } = string.Empty;
        public string CheckName { get; init; } = string.Empty;
        public string Status { get; init; } = string.Empty;
        public string ExpectedValue { get; init; } = string.Empty;
        public string ActualValue { get; init; } = string.Empty;
        public string Difference { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
    }
}
