// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Globalization;
using System.Text;

namespace SecretMoney.Ledger;

/// <summary>
/// Reads the CSV that <c>hledger print -x -O csv</c> writes, as docs/secretbeans-input.md describes:
/// one row per posting, rows of one transaction together, in hledger's order. It checks what it reads
/// and refuses anything else with <see cref="LedgerFormatException"/> rather than guessing.
/// </summary>
public static class HledgerCsvReader
{
    /// <summary>The columns SecretMoney uses. hledger 1.51.2 prints these and others; the others are ignored.</summary>
    public static readonly IReadOnlyList<string> RequiredColumns = ["txnidx", "date", "description", "account", "amount", "commodity"];

    /// <summary>Reads every transaction in <paramref name="csv"/>, in the order hledger printed them.</summary>
    public static IReadOnlyList<LedgerTransaction> Read(string csv)
    {
        ArgumentNullException.ThrowIfNull(csv);
        var rows = ParseRows(csv.StartsWith('﻿') ? csv[1..] : csv);
        if (rows.Count == 0)
        {
            throw new LedgerFormatException("The CSV is empty: there is no header row.");
        }

        var header = rows[0];
        var missing = RequiredColumns.Where(c => !header.Contains(c)).ToArray();
        if (missing.Length > 0)
        {
            throw new LedgerFormatException($"The CSV has no column {string.Join(", ", missing.Select(c => $"'{c}'"))}. Is it hledger's 'print -x -O csv' output?");
        }

        int Column(string name) => header.IndexOf(name);
        int txnidx = Column("txnidx"), date = Column("date"), description = Column("description"),
            account = Column("account"), amount = Column("amount"), commodity = Column("commodity");

        var transactions = new List<LedgerTransaction>();
        var seen = new HashSet<int>();
        var postings = new List<LedgerPosting>();
        int? currentIndex = null;
        DateOnly currentDate = default;
        string currentDescription = "";

        void Finish()
        {
            if (currentIndex is int index)
            {
                CheckBalanced(index, postings);
                transactions.Add(new LedgerTransaction(index, currentDate, currentDescription, postings.ToArray()));
                postings.Clear();
            }
        }

        for (var r = 1; r < rows.Count; r++)
        {
            var row = rows[r];
            var rowNumber = r + 1;
            if (row.Count == 1 && row[0].Length == 0)
            {
                continue; // a blank line
            }

            if (row.Count != header.Count)
            {
                throw new LedgerFormatException($"Row {rowNumber} has {row.Count} fields; the header has {header.Count}.");
            }

            var index = ParseIndex(row[txnidx], rowNumber);
            var rowDate = ParseDate(row[date], rowNumber);
            if (index != currentIndex)
            {
                Finish();
                if (!seen.Add(index))
                {
                    throw new LedgerFormatException($"Row {rowNumber}: transaction {index} appears in two places; its rows must be together.");
                }

                currentIndex = index;
                currentDate = rowDate;
                currentDescription = row[description];
            }
            else if (rowDate != currentDate || row[description] != currentDescription)
            {
                throw new LedgerFormatException($"Row {rowNumber}: transaction {index} has a different date or description than its first row.");
            }

            if (row[account].Length == 0)
            {
                throw new LedgerFormatException($"Row {rowNumber}: the account is empty.");
            }

            postings.Add(new LedgerPosting(row[account], ParseAmount(row[amount], rowNumber), row[commodity]));
        }

        Finish();
        return transactions;
    }

    private static int ParseIndex(string text, int rowNumber) =>
        int.TryParse(text, NumberStyles.None, CultureInfo.InvariantCulture, out var value) && value > 0
            ? value
            : throw new LedgerFormatException($"Row {rowNumber}: the txnidx '{text}' is not a positive whole number.");

    private static DateOnly ParseDate(string text, int rowNumber) =>
        DateOnly.TryParseExact(text, "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out var value)
            ? value
            : throw new LedgerFormatException($"Row {rowNumber}: the date '{text}' is not YYYY-MM-DD.");

    private static decimal ParseAmount(string text, int rowNumber) =>
        decimal.TryParse(text, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out var value)
            ? value
            : throw new LedgerFormatException($"Row {rowNumber}: the amount '{text}' is not a plain number (digits, an optional '-' and '.').");

    private static void CheckBalanced(int index, List<LedgerPosting> postings)
    {
        foreach (var group in postings.GroupBy(p => p.Commodity, StringComparer.Ordinal))
        {
            var total = group.Sum(p => p.Amount);
            if (total != 0m)
            {
                throw new LedgerFormatException($"Transaction {index} doesn't balance: its {group.Key} postings add up to {total.ToString(CultureInfo.InvariantCulture)}.");
            }
        }
    }

    /// <summary>
    /// Splits CSV text into rows of fields: commas between fields, double quotes around a field that
    /// holds commas, quotes or line breaks, a doubled quote for a quote, CR LF or LF between rows.
    /// </summary>
    private static List<List<string>> ParseRows(string text)
    {
        var rows = new List<List<string>>();
        var row = new List<string>();
        var field = new StringBuilder();
        var quoted = false;
        var i = 0;

        while (i < text.Length)
        {
            var c = text[i];
            if (quoted)
            {
                if (c == '"')
                {
                    if (i + 1 < text.Length && text[i + 1] == '"')
                    {
                        field.Append('"');
                        i += 2;
                        continue;
                    }

                    quoted = false;
                }
                else
                {
                    field.Append(c);
                }

                i++;
                continue;
            }

            switch (c)
            {
                case '"' when field.Length == 0:
                    quoted = true;
                    break;
                case '"':
                    throw new LedgerFormatException($"Row {rows.Count + 1}: a quote in the middle of an unquoted field.");
                case ',':
                    row.Add(field.ToString());
                    field.Clear();
                    break;
                case '\r' when i + 1 < text.Length && text[i + 1] == '\n':
                    break; // the LF ends the row
                case '\n':
                    row.Add(field.ToString());
                    field.Clear();
                    rows.Add(row);
                    row = [];
                    break;
                default:
                    field.Append(c);
                    break;
            }

            i++;
        }

        if (quoted)
        {
            throw new LedgerFormatException($"Row {rows.Count + 1}: a quoted field is never closed.");
        }

        if (field.Length > 0 || row.Count > 0)
        {
            row.Add(field.ToString());
            rows.Add(row);
        }

        return rows;
    }
}
