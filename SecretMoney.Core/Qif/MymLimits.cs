// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
namespace SecretMoney.Qif;

/// <summary>
/// Field lengths Managing Your Money 12 was seen to keep when importing QIF, from round 2 of the
/// research (docs/research/qif-compatibility.md, "Field lengths"). SecretMoney writes no more than
/// these, so every character it writes is one MYM was seen to keep.
/// </summary>
public static class MymLimits
{
    /// <summary>MYM kept 25 characters of an 80-character payee.</summary>
    public const int PayeeLength = 25;

    /// <summary>MYM kept 40 characters of an 80-character memo.</summary>
    public const int MemoLength = 40;

    /// <summary>
    /// MYM's category list showed 20 characters of a 40-character category, in a column 20 characters
    /// wide; its register shows 15. Whether it keeps more than 20 is not known, so 20 is the most
    /// SecretMoney writes.
    /// </summary>
    public const int CategoryLength = 20;
}
