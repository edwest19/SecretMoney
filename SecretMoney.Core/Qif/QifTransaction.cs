// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
namespace SecretMoney.Qif;

/// <summary>
/// One register entry in a QIF file (docs/qif-format.md, "Record"). <see cref="QifWriter"/> refuses
/// an entry that breaks the format's rules rather than changing it.
/// </summary>
/// <param name="Date">The date, written <c>MM/dd/yyyy</c>.</param>
/// <param name="Amount">
/// The account's own amount, in whole cents and not zero: negative for a payment or a charge,
/// positive for a deposit or a payment received by a card.
/// </param>
/// <param name="Payee">Required. Clean text (<see cref="QifText.Clean"/>), at most <see cref="MymLimits.PayeeLength"/> characters.</param>
/// <param name="Category">
/// Required: every entry MYM 12 imported in testing had one. Clean text, at most
/// <see cref="MymLimits.CategoryLength"/> characters, with no colon and not starting with <c>[</c>.
/// </param>
/// <param name="Memo">Optional. Clean text, at most <see cref="MymLimits.MemoLength"/> characters. Null or empty writes no M line.</param>
public sealed record QifTransaction(DateOnly Date, decimal Amount, string Payee, string Category, string? Memo = null);
