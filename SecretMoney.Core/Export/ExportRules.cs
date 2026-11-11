// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text.RegularExpressions;
using SecretMoney.Qif;

namespace SecretMoney.Export;

/// <summary>The individual rules of docs/export-rules.md, each small enough to test on its own.</summary>
public static partial class ExportRules
{
    /// <summary>The category for a transfer or card payment, in both accounts (round 2: <c>t4chk.qif</c>, <c>t4sav.qif</c>).</summary>
    public const string TransferCategory = "Transfer";

    /// <summary>The longest file name stem DOS allows: 8 characters before <c>.QIF</c>.</summary>
    public const int FileStemLength = 8;

    /// <summary>The category for an hledger account with no chosen name: its last part, in lower case, fitted to 20 characters.</summary>
    public static string DefaultCategory(string hledgerAccount) =>
        QifText.Fit(hledgerAccount.Split(':')[^1].ToLowerInvariant(), MymLimits.CategoryLength);

    /// <summary>Why <paramref name="category"/> can't be written (docs/qif-format.md, "Record"), or null if it can.</summary>
    public static string? CategoryProblem(string category) =>
        category.Length == 0 ? "it is empty"
        : !QifText.IsClean(category) ? "it isn't clean QIF text (printable ASCII, single spaces, no space at either end)"
        : category.Length > MymLimits.CategoryLength ? $"it is {category.Length} characters; MYM 12 keeps {MymLimits.CategoryLength}"
        : category.Contains(':', StringComparison.Ordinal) ? "it contains a colon"
        : category.StartsWith('[') ? "it starts with '['"
        : null;

    /// <summary>True for an account whose postings are transfers: one starting <c>assets:</c> or <c>liabilities:</c>.</summary>
    public static bool IsTransferAccount(string account) =>
        account.StartsWith("assets:", StringComparison.Ordinal) || account.StartsWith("liabilities:", StringComparison.Ordinal);

    /// <summary>
    /// True for an account SecretBeans uses for what it couldn't categorise: the review account or one
    /// under it, or any account with a part named <c>unknown</c> (SecretBeans' build report uses the same tests).
    /// </summary>
    public static bool IsUncategorised(string account, string reviewAccount) =>
        string.Equals(account, reviewAccount, StringComparison.Ordinal)
        || account.StartsWith(reviewAccount + ":", StringComparison.Ordinal)
        || account.Split(':').Contains("unknown", StringComparer.Ordinal);

    /// <summary>A file name stem made from a SecretBeans name: upper case, only A–Z, 0–9, <c>_</c> and <c>-</c>, at most 8 characters.</summary>
    public static string FileStem(string name) =>
        new string(name.ToUpperInvariant().Where(IsFileStemCharacter).Take(FileStemLength).ToArray());

    /// <summary>True when <paramref name="stem"/> is 1 to 8 of A–Z, 0–9, <c>_</c>, <c>-</c>.</summary>
    public static bool IsValidFileStem(string stem) =>
        stem.Length is >= 1 and <= FileStemLength && stem.All(IsFileStemCharacter);

    /// <summary>
    /// The payee and memo for a bank description: the payee is its first 25 characters; when it is
    /// longer than 25, the memo is its first 40, so the text MYM would otherwise move to the memo is kept.
    /// </summary>
    public static (string Payee, string? Memo) PayeeAndMemo(string description)
    {
        var clean = QifText.Clean(description);
        var payee = QifText.Fit(clean, MymLimits.PayeeLength);
        return (payee, clean.Length > MymLimits.PayeeLength ? QifText.Fit(clean, MymLimits.MemoLength) : null);
    }

    /// <summary>
    /// The accounts in a SecretBeans <c>generated.journal</c>, from the line SecretBeans writes before each
    /// account's block: <c>; ---- name (account) ----</c>. In the order they appear.
    /// </summary>
    public static IReadOnlyList<MymAccount> AccountsInGeneratedJournal(string journalText) =>
        BlockMarker().Matches(journalText)
            .Select(m => new MymAccount(
                m.Groups["account"].Value,
                m.Groups["name"].Value,
                FileStem(m.Groups["name"].Value),
                m.Groups["account"].Value.StartsWith("liabilities:", StringComparison.Ordinal) ? QifAccountType.CCard : QifAccountType.Bank))
            .DistinctBy(a => a.HledgerAccount, StringComparer.Ordinal)
            .ToArray();

    private static bool IsFileStemCharacter(char c) => c is (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_' or '-';

    [GeneratedRegex(@"^;\s*----\s*(?<name>\S+)\s*\((?<account>.+)\)\s*----\s*$", RegexOptions.Multiline)]
    private static partial Regex BlockMarker();
}
