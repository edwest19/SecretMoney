// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Ledger;
using SecretMoney.Qif;

namespace SecretMoney.Export;

/// <summary>Something that stops an export, and where (docs/export-rules.md, "What stops an export").</summary>
/// <param name="Account">The hledger account it concerns, or "" for the settings as a whole.</param>
/// <param name="Date">The transaction's date, or null when it isn't about one transaction.</param>
/// <param name="Description">The transaction's description, or "".</param>
/// <param name="Reason">What is wrong and what to do about it.</param>
public sealed record ExportProblem(string Account, DateOnly? Date, string Description, string Reason);

/// <summary>One account's QIF file: its entries from <see cref="From"/> to <see cref="To"/>, and its balance either side.</summary>
/// <param name="BalanceBefore">The account's balance at the end of the day before <see cref="From"/>: what MYM should show before the import.</param>
/// <param name="BalanceAfter">The balance at the end of <see cref="To"/>: what MYM should show after it.</param>
public sealed record ExportFile(MymAccount Account, DateOnly From, DateOnly To, IReadOnlyList<QifTransaction> Entries, decimal BalanceBefore, decimal BalanceAfter)
{
    /// <summary>The file's name: the stem and <c>.QIF</c>.</summary>
    public string FileName => Account.FileName + ".QIF";

    /// <summary>The file's bytes, written by <see cref="QifWriter"/>.</summary>
    public byte[] ToBytes() => QifWriter.WriteBytes(Account.Type, Entries);
}

/// <summary>What an export would write. It may be written only when <see cref="CanExport"/> is true.</summary>
public sealed record ExportPlan(IReadOnlyList<ExportFile> Files, IReadOnlyList<ExportProblem> Problems)
{
    /// <summary>True when nothing stops the export and at least one file has entries.</summary>
    public bool CanExport => Problems.Count == 0 && Files.Any(f => f.Entries.Count > 0);
}

/// <summary>Applies docs/export-rules.md to a SecretBeans period and the settings.</summary>
public static class ExportPlanner
{
    /// <summary>
    /// Plans an export. Each account's entries run from the day after its <see cref="MymAccount.LastExported"/>
    /// (or <paramref name="firstExportStart"/> if it has never been exported) to <paramref name="to"/>, inclusive.
    /// </summary>
    /// <param name="generated">hledger's print of the period's <c>generated.journal</c>.</param>
    /// <param name="opening">hledger's print of the period's <c>opening.journal</c>.</param>
    public static ExportPlan Plan(
        IReadOnlyList<LedgerTransaction> generated,
        IReadOnlyList<LedgerTransaction> opening,
        ExportSettings settings,
        DateOnly firstExportStart,
        DateOnly to)
    {
        ArgumentNullException.ThrowIfNull(generated);
        ArgumentNullException.ThrowIfNull(opening);
        ArgumentNullException.ThrowIfNull(settings);

        var problems = new List<ExportProblem>();
        CheckSettings(settings, problems);

        // MYM category name (compared ignoring case) -> the hledger accounts that would use it.
        var categoryUse = new Dictionary<string, SortedSet<string>>(StringComparer.OrdinalIgnoreCase);
        var files = new List<ExportFile>();

        foreach (var account in settings.Accounts)
        {
            var from = account.LastExported?.AddDays(1) ?? firstExportStart;
            var entries = LedgerAccounts.EntriesFor(generated, account.HledgerAccount);
            var before = LedgerAccounts.Total(opening, account.HledgerAccount) + entries.Where(e => e.Transaction.Date < from).Sum(e => e.Amount);
            var inRange = entries.Where(e => e.Transaction.Date >= from && e.Transaction.Date <= to).ToArray();
            var qif = new List<QifTransaction>();

            foreach (var entry in inRange)
            {
                var date = entry.Transaction.Date;
                var description = entry.Transaction.Description;
                void Stop(string reason) => problems.Add(new ExportProblem(account.HledgerAccount, date, description, reason));

                if (entry.Commodity != "$")
                {
                    Stop($"its amount is in '{entry.Commodity}', not '$'.");
                    continue;
                }

                if (entry.Amount == 0m)
                {
                    Stop("this account's amount in it is zero.");
                    continue;
                }

                if (entry.Others.Count != 1)
                {
                    Stop($"it has {entry.Others.Count} other postings; splits aren't written (docs/qif-format.md).");
                    continue;
                }

                var other = entry.Others[0].Account;
                string category;
                if (ExportRules.IsUncategorised(other, settings.ReviewAccount))
                {
                    Stop($"it is in {other}. Categorise it in SecretBeans and build again.");
                    continue;
                }

                if (other.StartsWith("equity:", StringComparison.Ordinal))
                {
                    Stop($"it goes to {other}. Give it a real account in SecretBeans' rules (its docs/TRANSFERS.md warns against catch-alls).");
                    continue;
                }

                if (ExportRules.IsTransferAccount(other))
                {
                    category = ExportRules.TransferCategory;
                    Use(categoryUse, category, "(transfers)");
                }
                else
                {
                    category = settings.Categories.TryGetValue(other, out var chosen) ? chosen : ExportRules.DefaultCategory(other);
                    if (ExportRules.CategoryProblem(category) is string why)
                    {
                        Stop($"its category '{category}' (from {other}) can't be written: {why}. Choose a name for {other} in the settings.");
                        continue;
                    }

                    Use(categoryUse, category, other);
                }

                var (payee, memo) = ExportRules.PayeeAndMemo(description);
                if (payee.Length == 0)
                {
                    Stop("its description is empty, so it has no payee.");
                    continue;
                }

                qif.Add(new QifTransaction(date, entry.Amount, payee, category, memo));
            }

            files.Add(new ExportFile(account, from, to, qif, before, before + inRange.Sum(e => e.Amount)));
        }

        foreach (var (category, accounts) in categoryUse.Where(kv => kv.Value.Count > 1).OrderBy(kv => kv.Key, StringComparer.OrdinalIgnoreCase))
        {
            problems.Add(new ExportProblem("", null, "",
                $"{string.Join(" and ", accounts)} would all be the MYM category '{category}'. Choose a different name for one of them in the settings."));
        }

        return new ExportPlan(files, problems);
    }

    private static void Use(Dictionary<string, SortedSet<string>> categoryUse, string category, string account)
    {
        if (!categoryUse.TryGetValue(category, out var accounts))
        {
            categoryUse[category] = accounts = new SortedSet<string>(StringComparer.Ordinal);
        }

        accounts.Add(account);
    }

    private static void CheckSettings(ExportSettings settings, List<ExportProblem> problems)
    {
        void Stop(string account, string reason) => problems.Add(new ExportProblem(account, null, "", reason));

        if (settings.Accounts.Count == 0)
        {
            Stop("", "No accounts are chosen for export.");
        }

        foreach (var account in settings.Accounts)
        {
            if (!ExportRules.IsValidFileStem(account.FileName))
            {
                Stop(account.HledgerAccount, $"The file name '{account.FileName}' must be 1 to 8 of A-Z, 0-9, '_' or '-'.");
            }
        }

        foreach (var group in settings.Accounts.GroupBy(a => a.FileName, StringComparer.OrdinalIgnoreCase).Where(g => g.Count() > 1))
        {
            Stop("", $"{string.Join(" and ", group.Select(a => a.HledgerAccount))} would all be written to {group.Key.ToUpperInvariant()}.QIF. Give each its own file name.");
        }

        foreach (var group in settings.Accounts.GroupBy(a => a.HledgerAccount, StringComparer.Ordinal).Where(g => g.Count() > 1))
        {
            Stop(group.Key, "This account is listed more than once.");
        }

        foreach (var (account, category) in settings.Categories.OrderBy(kv => kv.Key, StringComparer.Ordinal))
        {
            if (ExportRules.CategoryProblem(category) is string why)
            {
                Stop(account, $"The chosen category '{category}' can't be written: {why}.");
            }
        }
    }
}
