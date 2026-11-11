// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Qif;

namespace SecretMoney.Export;

/// <summary>
/// One SecretBeans account that SecretMoney exports to one MYM account (docs/export-rules.md).
/// </summary>
/// <param name="HledgerAccount">The account's full hledger name, for example <c>assets:bank:checking</c>.</param>
/// <param name="SecretBeansName">The account's short name in SecretBeans (its <c>Name</c>).</param>
/// <param name="FileName">The QIF file's name without <c>.QIF</c>: 1 to 8 of A–Z, 0–9, <c>_</c>, <c>-</c>.</param>
/// <param name="Type"><see cref="QifAccountType.Bank"/>, or <see cref="QifAccountType.CCard"/> for a MYM Charge account.</param>
/// <param name="LastExported">The last date already exported, or null if the account has never been exported.</param>
public sealed record MymAccount(string HledgerAccount, string SecretBeansName, string FileName, QifAccountType Type, DateOnly? LastExported = null);

/// <summary>
/// Everything SecretMoney remembers (docs/settings-format.md). The app keeps it in its MSIX package
/// data folder, so uninstalling SecretMoney deletes it.
/// </summary>
/// <param name="HledgerPath">Where hledger.exe is, or null if not chosen yet.</param>
/// <param name="LedgerFolder">SecretBeans' ledger root, the folder holding its period folders, or null.</param>
/// <param name="QifFolder">The folder QIF files are written to (DOSBox-X's D: drive), or null.</param>
/// <param name="ReviewAccount">SecretBeans' review account (its <c>ReviewAccount</c>, normally <c>expenses:review</c>).</param>
/// <param name="Accounts">The accounts to export, in the order shown.</param>
/// <param name="Categories">MYM category names chosen in place of the default, by hledger account.</param>
public sealed record ExportSettings(
    string? HledgerPath,
    string? LedgerFolder,
    string? QifFolder,
    string ReviewAccount,
    IReadOnlyList<MymAccount> Accounts,
    IReadOnlyDictionary<string, string> Categories)
{
    /// <summary>SecretBeans' default review account.</summary>
    public const string DefaultReviewAccount = "expenses:review";

    /// <summary>Settings with nothing chosen yet.</summary>
    public static ExportSettings Empty { get; } = new(null, null, null, DefaultReviewAccount, [], new Dictionary<string, string>());

    /// <summary>
    /// These settings after <paramref name="plan"/>'s files were written: every account in the plan,
    /// with or without entries, has its <see cref="MymAccount.LastExported"/> set to the plan's last date,
    /// unless it was already later; it never moves back (docs/export-rules.md, "Dates").
    /// </summary>
    public ExportSettings AfterExport(ExportPlan plan)
    {
        ArgumentNullException.ThrowIfNull(plan);
        var to = plan.Files.ToDictionary(f => f.Account.HledgerAccount, f => f.To, StringComparer.Ordinal);
        return this with
        {
            Accounts = Accounts.Select(a => to.TryGetValue(a.HledgerAccount, out var date) && !(a.LastExported > date) ? a with { LastExported = date } : a).ToArray(),
        };
    }
}

/// <summary>Thrown when a settings file isn't what docs/settings-format.md specifies. The message says why.</summary>
public sealed class SettingsFormatException(string message) : Exception(message);
