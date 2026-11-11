// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Export;
using SecretMoney.Qif;

namespace SecretMoney;

/// <summary>One row of the window's account list: an account the user can choose, rename and retype.</summary>
public sealed class AccountRow(AccountChoice choice)
{
    public MymAccount Account { get; } = choice.Account;

    public bool? Chosen { get; set; } = choice.Chosen;

    public string HledgerAccount => Account.HledgerAccount;

    public string FileName { get; set; } = choice.Account.FileName;

    /// <summary>0 for Bank, 1 for Charge.</summary>
    public int TypeIndex { get; set; } = choice.Account.Type == QifAccountType.CCard ? 1 : 0;

    public string LastExportedText => Account.LastExported is DateOnly date ? $"exported to {date:yyyy-MM-dd}" : "never exported";

    /// <summary>The account as the row now says: its file name in upper case, and its type.</summary>
    public MymAccount ToAccount() => Account with
    {
        FileName = FileName.Trim().ToUpperInvariant(),
        Type = TypeIndex == 1 ? QifAccountType.CCard : QifAccountType.Bank,
    };
}

/// <summary>One row of the window's category list: an hledger account and its MYM category name.</summary>
public sealed class CategoryRow(CategoryChoice choice)
{
    public string HledgerAccount { get; } = choice.HledgerAccount;

    public string DefaultName { get; } = choice.DefaultName;

    public string Name { get; set; } = choice.Name;

    public CategoryChoice ToChoice() => new(HledgerAccount, DefaultName, Name.Trim());
}
