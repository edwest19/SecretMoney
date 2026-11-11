// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
namespace SecretMoney.Export;

/// <summary>Writes a plan's QIF files into a folder (docs/export-rules.md, "Writing the files").</summary>
public static class ExportFiles
{
    /// <summary>The files with entries, which are the ones written.</summary>
    public static IEnumerable<ExportFile> ToWrite(ExportPlan plan) => plan.Files.Where(f => f.Entries.Count > 0);

    /// <summary>The names of the files that writing would replace in <paramref name="folder"/>.</summary>
    public static IReadOnlyList<string> Existing(ExportPlan plan, string folder) =>
        ToWrite(plan).Select(f => f.FileName).Where(name => File.Exists(Path.Combine(folder, name))).ToArray();

    /// <summary>
    /// Writes each file with entries into <paramref name="folder"/>, replacing a file of the same name, and
    /// returns their paths. Refuses a plan that <see cref="ExportPlan.CanExport"/> says can't be written.
    /// </summary>
    public static IReadOnlyList<string> Write(ExportPlan plan, string folder)
    {
        ArgumentNullException.ThrowIfNull(plan);
        if (!plan.CanExport)
        {
            throw new InvalidOperationException("This export can't be written: " +
                (plan.Problems.Count > 0 ? $"{plan.Problems.Count} problem(s) stop it." : "it has no entries."));
        }

        if (!Directory.Exists(folder))
        {
            throw new DirectoryNotFoundException($"The QIF folder {folder} doesn't exist.");
        }

        var written = new List<string>();
        foreach (var file in ToWrite(plan))
        {
            var path = Path.Combine(folder, file.FileName);
            File.WriteAllBytes(path, file.ToBytes());
            written.Add(path);
        }

        return written;
    }
}
