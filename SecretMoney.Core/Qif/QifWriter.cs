// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Globalization;
using System.Text;

namespace SecretMoney.Qif;

/// <summary>
/// Writes a QIF file exactly as docs/qif-format.md (version 1) specifies, using only what
/// Managing Your Money 12 was seen to import. Locked by the known-answer tests in QifWriterTests.
/// It never changes an entry to make it fit: an entry that breaks a rule is refused with
/// <see cref="QifFormatException"/>.
/// </summary>
public static class QifWriter
{
    /// <summary>The version of docs/qif-format.md this writer follows.</summary>
    public const int FormatVersion = 1;

    private const string NewLine = "\r\n";

    /// <summary>The file as text: the header, then each entry in the order given.</summary>
    public static string Write(QifAccountType accountType, IReadOnlyList<QifTransaction> transactions)
    {
        ArgumentNullException.ThrowIfNull(transactions);
        if (transactions.Count == 0)
        {
            throw new QifFormatException("A QIF file needs at least one entry.");
        }

        var builder = new StringBuilder();
        builder.Append(Header(accountType)).Append(NewLine);

        for (var i = 0; i < transactions.Count; i++)
        {
            var entry = transactions[i] ?? throw new QifFormatException($"Entry {i + 1} is missing.");
            Check(entry, i + 1);

            builder.Append('D').Append(entry.Date.ToString("MM'/'dd'/'yyyy", CultureInfo.InvariantCulture)).Append(NewLine);
            builder.Append('T').Append(entry.Amount.ToString("0.00", CultureInfo.InvariantCulture)).Append(NewLine);
            builder.Append('P').Append(entry.Payee).Append(NewLine);
            if (!string.IsNullOrEmpty(entry.Memo))
            {
                builder.Append('M').Append(entry.Memo).Append(NewLine);
            }

            builder.Append('L').Append(entry.Category).Append(NewLine);
            builder.Append('^').Append(NewLine);
        }

        return builder.ToString();
    }

    /// <summary>The file as the bytes to save: <see cref="Write"/> in ASCII, with no byte-order mark.</summary>
    public static byte[] WriteBytes(QifAccountType accountType, IReadOnlyList<QifTransaction> transactions) =>
        Encoding.ASCII.GetBytes(Write(accountType, transactions));

    private static string Header(QifAccountType accountType) => accountType switch
    {
        QifAccountType.Bank => "!Type:Bank",
        QifAccountType.CCard => "!Type:CCard",
        _ => throw new QifFormatException($"Unknown account type {accountType}."),
    };

    private static void Check(QifTransaction entry, int number)
    {
        if (entry.Amount == 0m)
        {
            throw new QifFormatException($"Entry {number}: the amount is zero.");
        }

        if (decimal.Round(entry.Amount, 2) != entry.Amount)
        {
            throw new QifFormatException($"Entry {number}: the amount {entry.Amount.ToString(CultureInfo.InvariantCulture)} is not a whole number of cents.");
        }

        if (string.IsNullOrEmpty(entry.Payee))
        {
            throw new QifFormatException($"Entry {number}: the payee is empty.");
        }

        CheckText(entry.Payee, "payee", MymLimits.PayeeLength, number);

        if (!string.IsNullOrEmpty(entry.Memo))
        {
            CheckText(entry.Memo, "memo", MymLimits.MemoLength, number);
        }

        if (string.IsNullOrEmpty(entry.Category))
        {
            throw new QifFormatException($"Entry {number}: the category is empty.");
        }

        CheckText(entry.Category, "category", MymLimits.CategoryLength, number);
        if (entry.Category.Contains(':', StringComparison.Ordinal))
        {
            throw new QifFormatException($"Entry {number}: the category '{entry.Category}' contains a colon, which MYM 12 keeps as part of the name.");
        }

        if (entry.Category.StartsWith('['))
        {
            throw new QifFormatException($"Entry {number}: the category '{entry.Category}' starts with '[', the QIF transfer form MYM 12 did not import as a transfer.");
        }
    }

    private static void CheckText(string value, string field, int maxLength, int number)
    {
        if (!QifText.IsClean(value))
        {
            throw new QifFormatException($"Entry {number}: the {field} '{value}' is not clean QIF text (printable ASCII, one line, single spaces, no space at either end).");
        }

        if (value.Length > maxLength)
        {
            throw new QifFormatException($"Entry {number}: the {field} is {value.Length} characters; MYM 12 keeps {maxLength}.");
        }
    }
}
