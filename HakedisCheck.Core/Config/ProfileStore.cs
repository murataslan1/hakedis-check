using System.Text.Json;
using System.Text.Json.Serialization;
using HakedisCheck.Core.Models;

namespace HakedisCheck.Core.Config;

public sealed class ProfileStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly string _profileDirectory;

    public ProfileStore(string? profileDirectory = null)
    {
        _profileDirectory = profileDirectory
            ?? Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "HakedisCheck",
                "profiles");

        Directory.CreateDirectory(_profileDirectory);
    }

    public string ProfileDirectory => _profileDirectory;

    public void Save(ColumnProfile profile)
    {
        profile.SavedAtUtc = DateTimeOffset.UtcNow;
        var fullPath = Path.Combine(_profileDirectory, GetFileName(profile));
        File.WriteAllText(fullPath, JsonSerializer.Serialize(profile, SerializerOptions));
    }

    public IReadOnlyList<ColumnProfile> LoadAll(ExcelFileKind? fileKind = null)
    {
        if (!Directory.Exists(_profileDirectory))
        {
            return [];
        }

        var profiles = new List<ColumnProfile>();
        foreach (var file in Directory.GetFiles(_profileDirectory, "*.json"))
        {
            try
            {
                var content = File.ReadAllText(file);
                var profile = JsonSerializer.Deserialize<ColumnProfile>(content, SerializerOptions);
                if (profile is null)
                {
                    continue;
                }

                if (fileKind is null || profile.FileKind == fileKind)
                {
                    profiles.Add(profile);
                }
            }
            catch
            {
                // Ignore malformed profiles and continue loading the rest.
            }
        }

        return profiles
            .OrderByDescending(profile => profile.SavedAtUtc)
            .ThenBy(profile => profile.ProfileName, StringComparer.CurrentCultureIgnoreCase)
            .ToArray();
    }

    public void Delete(ColumnProfile profile)
    {
        var fullPath = Path.Combine(_profileDirectory, GetFileName(profile));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private static string GetFileName(ColumnProfile profile)
    {
        var safeName = new string(
            profile.ProfileName
                .Trim()
                .Select(character => Path.GetInvalidFileNameChars().Contains(character) ? '_' : character)
                .ToArray());

        if (string.IsNullOrWhiteSpace(safeName))
        {
            safeName = "profil";
        }

        return $"{profile.FileKind}-{safeName}.json";
    }
}
