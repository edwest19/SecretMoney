// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Globalization;
using System.Text;
using System.Text.Json;
using SecretMoney.Qif;

namespace SecretMoney.Export;

/// <summary>
/// Reads and writes the settings file exactly as docs/settings-format.md (version 1) specifies.
/// Locked by the known-answer tests in SettingsFileTests. Reading is strict: anything the format
/// doesn't allow is refused with <see cref="SettingsFormatException"/>, never ignored.
/// </summary>
public static class SettingsFile
{
    /// <summary>The version of docs/settings-format.md this code reads and writes.</summary>
    public const int FormatVersion = 1;

    private static readonly string[] TopLevel = ["formatVersion", "hledgerPath", "ledgerFolder", "qifFolder", "reviewAccount", "accounts", "categories"];
    private static readonly string[] AccountFields = ["hledgerAccount", "secretBeansName", "fileName", "type", "lastExported"];

    /// <summary>The settings as the file's text: indented JSON, LF line endings, ending with LF.</summary>
    public static string Write(ExportSettings settings)
    {
        ArgumentNullException.ThrowIfNull(settings);
        using var stream = new MemoryStream();
        using (var w = new Utf8JsonWriter(stream, new JsonWriterOptions { Indented = true, NewLine = "\n" }))
        {
            w.WriteStartObject();
            w.WriteNumber("formatVersion", FormatVersion);
            WriteStringOrNull(w, "hledgerPath", settings.HledgerPath);
            WriteStringOrNull(w, "ledgerFolder", settings.LedgerFolder);
            WriteStringOrNull(w, "qifFolder", settings.QifFolder);
            w.WriteString("reviewAccount", settings.ReviewAccount);
            w.WriteStartArray("accounts");
            foreach (var a in settings.Accounts)
            {
                w.WriteStartObject();
                w.WriteString("hledgerAccount", a.HledgerAccount);
                w.WriteString("secretBeansName", a.SecretBeansName);
                w.WriteString("fileName", a.FileName);
                w.WriteString("type", a.Type.ToString());
                WriteStringOrNull(w, "lastExported", a.LastExported?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture));
                w.WriteEndObject();
            }

            w.WriteEndArray();
            w.WriteStartObject("categories");
            foreach (var (account, category) in settings.Categories.OrderBy(kv => kv.Key, StringComparer.Ordinal))
            {
                w.WriteString(account, category);
            }

            w.WriteEndObject();
            w.WriteEndObject();
        }

        return Encoding.UTF8.GetString(stream.ToArray()) + "\n";
    }

    /// <summary>Reads a settings file's text, refusing anything docs/settings-format.md doesn't allow.</summary>
    public static ExportSettings Read(string text)
    {
        ArgumentNullException.ThrowIfNull(text);
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(text);
        }
        catch (JsonException ex)
        {
            throw new SettingsFormatException($"The settings file isn't valid JSON: {ex.Message}");
        }

        using (document)
        {
            var root = Object(document.RootElement, "The settings file", TopLevel);

            var version = root["formatVersion"];
            if (version.ValueKind != JsonValueKind.Number || !version.TryGetInt32(out var number) || number != FormatVersion)
            {
                throw new SettingsFormatException($"The settings file is format version {version.GetRawText()}; this SecretMoney reads version {FormatVersion}.");
            }

            var hledgerPath = StringOrNull(root["hledgerPath"], "hledgerPath");
            var ledgerFolder = StringOrNull(root["ledgerFolder"], "ledgerFolder");
            var qifFolder = StringOrNull(root["qifFolder"], "qifFolder");
            var reviewAccount = StringOrNull(root["reviewAccount"], "reviewAccount");
            if (string.IsNullOrEmpty(reviewAccount))
            {
                throw new SettingsFormatException("'reviewAccount' must be a non-empty string.");
            }

            if (root["accounts"].ValueKind != JsonValueKind.Array)
            {
                throw new SettingsFormatException("'accounts' must be an array.");
            }

            var accounts = new List<MymAccount>();
            var seen = new HashSet<string>(StringComparer.Ordinal);
            var n = 0;
            foreach (var element in root["accounts"].EnumerateArray())
            {
                n++;
                var where = $"Account {n}";
                var a = Object(element, where, AccountFields);
                var hledgerAccount = Required(a["hledgerAccount"], $"{where} 'hledgerAccount'");
                if (!seen.Add(hledgerAccount))
                {
                    throw new SettingsFormatException($"{where}: {hledgerAccount} is listed twice.");
                }

                var type = Required(a["type"], $"{where} 'type'") switch
                {
                    "Bank" => QifAccountType.Bank,
                    "CCard" => QifAccountType.CCard,
                    var other => throw new SettingsFormatException($"{where}: 'type' is '{other}'; it must be 'Bank' or 'CCard'."),
                };

                DateOnly? lastExported = null;
                if (StringOrNull(a["lastExported"], $"{where} 'lastExported'") is string date)
                {
                    lastExported = DateOnly.TryParseExact(date, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                        ? d
                        : throw new SettingsFormatException($"{where}: 'lastExported' is '{date}'; it must be YYYY-MM-DD or null.");
                }

                accounts.Add(new MymAccount(
                    hledgerAccount,
                    Required(a["secretBeansName"], $"{where} 'secretBeansName'"),
                    Required(a["fileName"], $"{where} 'fileName'"),
                    type,
                    lastExported));
            }

            if (root["categories"].ValueKind != JsonValueKind.Object)
            {
                throw new SettingsFormatException("'categories' must be an object.");
            }

            var categories = new Dictionary<string, string>(StringComparer.Ordinal);
            foreach (var property in root["categories"].EnumerateObject())
            {
                if (!categories.TryAdd(property.Name, Required(property.Value, $"The category for {property.Name}")))
                {
                    throw new SettingsFormatException($"'categories' lists {property.Name} twice.");
                }
            }

            return new ExportSettings(hledgerPath, ledgerFolder, qifFolder, reviewAccount, accounts, categories);
        }
    }

    private static Dictionary<string, JsonElement> Object(JsonElement element, string what, string[] fields)
    {
        if (element.ValueKind != JsonValueKind.Object)
        {
            throw new SettingsFormatException($"{what} must be a JSON object.");
        }

        var found = new Dictionary<string, JsonElement>(StringComparer.Ordinal);
        foreach (var property in element.EnumerateObject())
        {
            if (!fields.Contains(property.Name, StringComparer.Ordinal))
            {
                throw new SettingsFormatException($"{what} has '{property.Name}', which the format doesn't have.");
            }

            if (!found.TryAdd(property.Name, property.Value))
            {
                throw new SettingsFormatException($"{what} has '{property.Name}' twice.");
            }
        }

        var missing = fields.Where(f => !found.ContainsKey(f)).ToArray();
        return missing.Length == 0
            ? found
            : throw new SettingsFormatException($"{what} has no {string.Join(", ", missing.Select(m => $"'{m}'"))}.");
    }

    private static string? StringOrNull(JsonElement element, string what) => element.ValueKind switch
    {
        JsonValueKind.String => element.GetString(),
        JsonValueKind.Null => null,
        _ => throw new SettingsFormatException($"{what} must be a string or null."),
    };

    private static string Required(JsonElement element, string what) =>
        element.ValueKind == JsonValueKind.String && element.GetString() is { Length: > 0 } value
            ? value
            : throw new SettingsFormatException($"{what} must be a non-empty string.");

    private static void WriteStringOrNull(Utf8JsonWriter writer, string name, string? value)
    {
        if (value is null)
        {
            writer.WriteNull(name);
        }
        else
        {
            writer.WriteString(name, value);
        }
    }
}
