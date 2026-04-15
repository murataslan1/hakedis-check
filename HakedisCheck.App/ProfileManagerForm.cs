using HakedisCheck.Core.Config;
using HakedisCheck.Core.Models;

namespace HakedisCheck.App;

public sealed class ProfileManagerForm : Form
{
    private readonly ProfileStore _profileStore;
    private readonly ExcelFileKind _kind;
    private readonly ListBox _profilesList = new() { Dock = DockStyle.Fill };
    private readonly TextBox _detailsTextBox = new() { Dock = DockStyle.Fill, Multiline = true, ReadOnly = true };

    public ProfileManagerForm(ProfileStore profileStore, ExcelFileKind kind)
    {
        _profileStore = profileStore;
        _kind = kind;

        Text = $"{kind.GetDisplayName()} Profilleri";
        Width = 700;
        Height = 420;
        StartPosition = FormStartPosition.CenterParent;

        BuildLayout();
        ReloadProfiles();
    }

    public ColumnProfile? SelectedProfile { get; private set; }

    private void BuildLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 2,
            RowCount = 2,
            Padding = new Padding(12)
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        root.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        _profilesList.SelectedIndexChanged += (_, _) => UpdateDetails();
        root.Controls.Add(_profilesList, 0, 0);
        root.Controls.Add(_detailsTextBox, 1, 0);

        var buttonPanel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.RightToLeft,
            AutoSize = true
        };

        var loadButton = new Button { Text = "Yükle", AutoSize = true };
        loadButton.Click += (_, _) => LoadProfile();

        var deleteButton = new Button { Text = "Sil", AutoSize = true };
        deleteButton.Click += (_, _) => DeleteProfile();

        var cancelButton = new Button { Text = "Kapat", AutoSize = true };
        cancelButton.Click += (_, _) => DialogResult = DialogResult.Cancel;

        buttonPanel.Controls.Add(loadButton);
        buttonPanel.Controls.Add(deleteButton);
        buttonPanel.Controls.Add(cancelButton);

        root.SetColumnSpan(buttonPanel, 2);
        root.Controls.Add(buttonPanel, 0, 1);

        Controls.Add(root);
    }

    private void ReloadProfiles()
    {
        var profiles = _profileStore.LoadAll(_kind).ToArray();
        _profilesList.DataSource = profiles;
        _profilesList.DisplayMember = nameof(ColumnProfile.ProfileName);
        UpdateDetails();
    }

    private void UpdateDetails()
    {
        if (_profilesList.SelectedItem is not ColumnProfile profile)
        {
            _detailsTextBox.Text = "Profil seçilmedi.";
            return;
        }

        var headerLines = new[]
        {
            $"Profil: {profile.ProfileName}",
            $"Başlık satırı: {profile.HeaderRowIndex}",
            $"İlk veri satırı: {profile.FirstDataRowIndex}",
            $"Sayfalar: {string.Join(", ", profile.SelectedSheets)}",
            string.Empty,
            "Kolonlar:"
        };

        _detailsTextBox.Text = string.Join(
            Environment.NewLine,
            headerLines.Concat(profile.ColumnMappings.Select(pair => $"{ProfileSchema.GetDisplayName(pair.Key)} => {pair.Value ?? "(yok)"}")));
    }

    private void LoadProfile()
    {
        if (_profilesList.SelectedItem is not ColumnProfile profile)
        {
            return;
        }

        SelectedProfile = profile.Clone();
        DialogResult = DialogResult.OK;
    }

    private void DeleteProfile()
    {
        if (_profilesList.SelectedItem is not ColumnProfile profile)
        {
            return;
        }

        var confirm = MessageBox.Show(
            this,
            $"'{profile.ProfileName}' profili silinsin mi?",
            "Profil Sil",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (confirm != DialogResult.Yes)
        {
            return;
        }

        _profileStore.Delete(profile);
        ReloadProfiles();
    }
}
