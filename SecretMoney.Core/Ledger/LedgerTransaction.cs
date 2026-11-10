// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
namespace SecretMoney.Ledger;

/// <summary>One posting of an hledger transaction: an account, an amount and its commodity (for SecretBeans, "$").</summary>
public sealed record LedgerPosting(string Account, decimal Amount, string Commodity);

/// <summary>
/// One hledger transaction as <c>hledger print -x -O csv</c> prints it (docs/secretbeans-input.md).
/// <see cref="Index"/> is hledger's <c>txnidx</c>: the transaction's place in the journal files, not
/// in the printed order.
/// </summary>
public sealed record LedgerTransaction(int Index, DateOnly Date, string Description, IReadOnlyList<LedgerPosting> Postings);

/// <summary>
/// What one transaction means for one account: the account's own amount (its postings added
/// together) and the transaction's other postings, which say where the money came from or went.
/// </summary>
public sealed record AccountEntry(LedgerTransaction Transaction, string Account, decimal Amount, string Commodity, IReadOnlyList<LedgerPosting> Others);

/// <summary>Thrown when hledger's CSV isn't what docs/secretbeans-input.md describes. The message says where and why.</summary>
public sealed class LedgerFormatException(string message) : Exception(message);
