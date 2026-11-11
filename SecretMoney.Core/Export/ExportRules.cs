// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text.RegularExpressions;
using SecretMoney.Ledger;
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

    /// <summary>
    /// The accounts the window offers: those found in <c>generated.journal</c>, in its order, each as
    /// already chosen in the settings if it is there; then any chosen account no longer found. With no
    /// accounts chosen yet, every account found is offered as chosen.
    /// </summary>
    public static IReadOnlyList<AccountChoice> AccountChoices(IReadOnlyList<MymAccount> found, IReadOnlyList<MymAccount> chosen)
    {
        var byAccount = chosen.ToDictionary(a => a.HledgerAccount, StringComparer.Ordinal);
        var foundNames = found.Select(f => f.HledgerAccount).ToHashSet(StringComparer.Ordinal);
        return found
            .Select(f => byAccount.TryGetValue(f.HledgerAccount, out var c) ? new AccountChoice(c, true) : new AccountChoice(f, chosen.Count == 0))
            .Concat(chosen.Where(c => !foundNames.Contains(c.HledgerAccount)).Select(c => new AccountChoice(c, true)))
            .ToArray();
    }

    /// <summary>
    /// The categories the chosen accounts' entries use, in ordinal order of the hledger account: every
    /// other account of a one-other-posting entry that isn't a transfer, uncategorised or equity, with its
    /// default name and its name now (the chosen one, or the default).
    /// </summary>
    public static IReadOnlyList<CategoryChoice> CategoryChoices(IReadOnlyList<LedgerTransaction> generated, ExportSettings settings)
    {
        var accounts = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var account in settings.Accounts)
        {
            foreach (var entry in LedgerAccounts.EntriesFor(generated, account.HledgerAccount).Where(e => e.Others.Count == 1))
            {
                var other = entry.Others[0].Account;
                if (!IsTransferAccount(other) && !IsUncategorised(other, settings.ReviewAccount) && !other.StartsWith("equity:", StringComparison.Ordinal))
                {
                    accounts.Add(other);
                }
            }
        }

        return accounts
            .Select(a => new CategoryChoice(a, DefaultCategory(a), settings.Categories.TryGetValue(a, out var name) ? name : DefaultCategory(a)))
            .ToArray();
    }

    /// <summary>The names to keep in the settings: only those that differ from the default.</summary>
    public static IReadOnlyDictionary<string, string> ChosenCategories(IEnumerable<CategoryChoice> choices) =>
        choices.Where(c => !string.Equals(c.Name, c.DefaultName, StringComparison.Ordinal))
            .ToDictionary(c => c.HledgerAccount, c => c.Name, StringComparer.Ordinal);

    private static bool IsFileStemCharacter(char c) => c is (>= 'A' and <= 'Z') or (>= '0' and <= '9') or '_' or '-';

    [GeneratedRegex(@"^;\s*----\s*(?<name>\S+)\s*\((?<account>.+)\)\s*----\s*$", RegexOptions.Multiline)]
    private static partial Regex BlockMarker();
}

/// <summary>An account offered for export, and whether it is chosen.</summary>
public sealed record AccountChoice(MymAccount Account, bool Chosen);

/// <summary>A category the export uses: the hledger account, its default MYM name, and its name now.</summary>
public sealed record CategoryChoice(string HledgerAccount, string DefaultName, string Name);
