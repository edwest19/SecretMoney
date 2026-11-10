// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Globalization;

namespace SecretMoney.Ledger;

/// <summary>
/// One account's view of a set of transactions (docs/secretbeans-input.md, "One account's entries").
/// SecretBeans records a transfer or a card payment once, in one account's block; looked at from
/// either account here, it is an entry in both.
/// </summary>
public static class LedgerAccounts
{
    /// <summary>
    /// One entry for every transaction with a posting to exactly <paramref name="account"/> (ordinal,
    /// case-sensitive, no subaccounts), in the order of <paramref name="transactions"/>. The entry's amount
    /// is that account's postings added together; its other postings are everything else in the transaction.
    /// </summary>
    public static IReadOnlyList<AccountEntry> EntriesFor(IReadOnlyList<LedgerTransaction> transactions, string account)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        ArgumentException.ThrowIfNullOrEmpty(account);

        var entries = new List<AccountEntry>();
        foreach (var transaction in transactions)
        {
            var own = transaction.Postings.Where(p => string.Equals(p.Account, account, StringComparison.Ordinal)).ToArray();
            if (own.Length == 0)
            {
                continue;
            }

            var commodity = SingleCommodity(own, transaction, account);
            var others = transaction.Postings.Where(p => !string.Equals(p.Account, account, StringComparison.Ordinal)).ToArray();
            entries.Add(new AccountEntry(transaction, account, own.Sum(p => p.Amount), commodity, others));
        }

        return entries;
    }

    /// <summary>
    /// <paramref name="account"/>'s postings across all of <paramref name="transactions"/>, added together:
    /// for SecretBeans' opening.journal, the account's opening balance. Zero when it has none.
    /// </summary>
    public static decimal Total(IReadOnlyList<LedgerTransaction> transactions, string account)
    {
        var entries = EntriesFor(transactions, account);
        var commodities = entries.Select(e => e.Commodity).Distinct(StringComparer.Ordinal).ToArray();
        return commodities.Length <= 1
            ? entries.Sum(e => e.Amount)
            : throw new LedgerFormatException($"{account} has postings in more than one commodity ({string.Join(", ", commodities)}).");
    }

    private static string SingleCommodity(LedgerPosting[] postings, LedgerTransaction transaction, string account)
    {
        var commodities = postings.Select(p => p.Commodity).Distinct(StringComparer.Ordinal).ToArray();
        return commodities.Length == 1
            ? commodities[0]
            : throw new LedgerFormatException(
                $"Transaction {transaction.Index} on {transaction.Date.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture)} posts to {account} in more than one commodity ({string.Join(", ", commodities)}).");
    }
}
