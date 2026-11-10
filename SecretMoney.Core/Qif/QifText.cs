// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Globalization;
using System.Text;

namespace SecretMoney.Qif;

/// <summary>
/// Turns any text into what a QIF field may hold: printable ASCII on one line
/// (docs/qif-format.md, "Text"). MYM 12 has only been tested with ASCII files.
/// </summary>
public static class QifText
{
    /// <summary>
    /// Returns <paramref name="text"/> as printable ASCII (0x20 to 0x7E), in these steps:
    /// compatibility decomposition (NFKD), so "é" becomes "e" plus an accent and "…" becomes "...";
    /// accents dropped; curly quotes to straight ones and dashes to "-"; control characters, including
    /// tabs and line breaks, to a space; any other character outside ASCII to "?"; runs of spaces to
    /// one; leading and trailing spaces removed. Null gives "".
    /// </summary>
    public static string Clean(string? text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return "";
        }

        var builder = new StringBuilder(text.Length);
        foreach (var rune in text.Normalize(NormalizationForm.FormKD).EnumerateRunes())
        {
            if (Rune.GetUnicodeCategory(rune) == UnicodeCategory.NonSpacingMark)
            {
                continue;
            }

            var c = rune.Value switch
            {
                0x2018 or 0x2019 or 0x201A or 0x201B or 0x2032 => '\'',
                0x201C or 0x201D or 0x201E or 0x201F or 0x2033 => '"',
                0x2010 or 0x2011 or 0x2012 or 0x2013 or 0x2014 or 0x2015 or 0x2212 => '-',
                < 0x20 or 0x7F => ' ',
                > 0x7E when Rune.IsControl(rune) => ' ',
                > 0x7E => '?',
                _ => (char)rune.Value,
            };

            if (c == ' ' && (builder.Length == 0 || builder[^1] == ' '))
            {
                continue;
            }

            builder.Append(c);
        }

        return builder.ToString().TrimEnd(' ');
    }

    /// <summary>True when <paramref name="text"/> is already what <see cref="Clean"/> returns.</summary>
    public static bool IsClean(string text) => string.Equals(text, Clean(text), StringComparison.Ordinal);

    /// <summary>
    /// <see cref="Clean"/>, then cut to at most <paramref name="maxLength"/> characters, with any space
    /// left at the end of the cut removed.
    /// </summary>
    public static string Fit(string? text, int maxLength)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxLength);
        var clean = Clean(text);
        return clean.Length <= maxLength ? clean : clean[..maxLength].TrimEnd(' ');
    }
}
