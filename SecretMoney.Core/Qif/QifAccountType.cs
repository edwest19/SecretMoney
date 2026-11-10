// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
namespace SecretMoney.Qif;

/// <summary>The kind of MYM account a QIF file is imported into (docs/qif-format.md, "Header").</summary>
public enum QifAccountType
{
    /// <summary>A bank account: the file starts <c>!Type:Bank</c>.</summary>
    Bank,

    /// <summary>
    /// A Charge (credit-card) account: the file starts <c>!Type:CCard</c>. Round 2 of the MYM 12
    /// research showed MYM imports it into a Charge account, a charge negative and a payment positive.
    /// </summary>
    CCard,
}
