// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text.RegularExpressions;

namespace SecretMoney.Ledger;

/// <summary>
/// Finds a SecretBeans period and its journal files (docs/secretbeans-input.md, "Which period").
/// </summary>
public static partial class SecretBeansPeriod
{
    /// <summary>
    /// The newest period in <paramref name="ledgerRoot"/>, as SecretBeans' README picks it: the last folder,
    /// in name order, whose name starts <c>YYYY-MM </c> and that has <c>journal\main.journal</c>.
    /// Null when there is none.
    /// </summary>
    public static string? FindLatest(string ledgerRoot)
    {
        ArgumentException.ThrowIfNullOrEmpty(ledgerRoot);
        if (!Directory.Exists(ledgerRoot))
        {
            return null;
        }

        return Directory.EnumerateDirectories(ledgerRoot)
            .Where(d => PeriodName().IsMatch(Path.GetFileName(d)) && File.Exists(Journal(d, "main.journal")))
            .OrderBy(d => Path.GetFileName(d), StringComparer.Ordinal)
            .LastOrDefault();
    }

    /// <summary>The path of one of a period's journal files: <c>&lt;period&gt;\journal\&lt;name&gt;</c>.</summary>
    public static string Journal(string periodFolder, string name) => Path.Combine(periodFolder, "journal", name);

    [GeneratedRegex(@"^\d{4}-\d{2} ")]
    private static partial Regex PeriodName();
}
