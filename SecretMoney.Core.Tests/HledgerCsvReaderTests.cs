// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Ledger;

namespace SecretMoney.Core.Tests;

/// <summary>The CSV rules and refusals in docs/secretbeans-input.md, on small hand-made inputs.</summary>
[TestClass]
public sealed class HledgerCsvReaderTests
{
    private const string Header = "\"txnidx\",\"date\",\"date2\",\"status\",\"code\",\"description\",\"comment\",\"account\",\"amount\",\"commodity\",\"credit\",\"debit\",\"posting-status\",\"posting-comment\"";

    private static string Row(string txnidx, string date, string description, string account, string amount, string commodity = "$") =>
        string.Join(",", new[] { txnidx, date, "", "", "", description, "", account, amount, commodity, "", "", "", "" }.Select(f => "\"" + f.Replace("\"", "\"\"") + "\""));

    private static string Csv(string newLine, params string[] rows) => string.Join(newLine, new[] { Header }.Concat(rows)) + newLine;

    private static readonly string[] Pair = [Row("1", "2026-07-03", "SHOP", "assets:a", "-1.00"), Row("1", "2026-07-03", "SHOP", "expenses:b", "1.00")];

    [TestMethod]
    public void LfLineEndings_AreReadLikeCrLf()
    {
        var crlf = HledgerCsvReader.Read(Csv("\r\n", Pair));
        var lf = HledgerCsvReader.Read(Csv("\n", Pair));

        Assert.HasCount(1, crlf);
        Assert.AreEqual(crlf[0].Description, lf[0].Description);
        CollectionAssert.AreEqual(crlf[0].Postings.ToArray(), lf[0].Postings.ToArray());
    }

    [TestMethod]
    public void ByteOrderMark_IsIgnored()
    {
        Assert.HasCount(1, HledgerCsvReader.Read("﻿" + Csv("\r\n", Pair)));
    }

    [TestMethod]
    public void QuotedFields_KeepCommasQuotesAndLineBreaks()
    {
        var description = "A, \"B\"\r\nC";
        var t = HledgerCsvReader.Read(Csv("\r\n", Row("1", "2026-07-03", description, "assets:a", "-1.00"), Row("1", "2026-07-03", description, "expenses:b", "1.00")));

        Assert.AreEqual(description, t[0].Description);
    }

    [TestMethod]
    public void OtherColumnsAndOrder_DoNotMatter()
    {
        var csv = "\"account\",\"amount\",\"commodity\",\"description\",\"date\",\"txnidx\",\"extra\"\n" +
                  "\"assets:a\",\"-2.50\",\"$\",\"X\",\"2026-01-31\",\"7\",\"z\"\n" +
                  "\"expenses:b\",\"2.50\",\"$\",\"X\",\"2026-01-31\",\"7\",\"z\"\n";

        var t = HledgerCsvReader.Read(csv).Single();

        Assert.AreEqual(7, t.Index);
        Assert.AreEqual(new DateOnly(2026, 1, 31), t.Date);
        Assert.AreEqual(-2.50m, t.Postings[0].Amount);
    }

    [TestMethod]
    [DataRow(0, "The CSV is empty")]
    [DataRow(1, "has no column 'amount'")]
    [DataRow(2, "the amount '1,500.00' is not a plain number")]
    [DataRow(3, "the amount '$1.00' is not a plain number")]
    [DataRow(4, "the date '07/03/2026' is not YYYY-MM-DD")]
    [DataRow(5, "Transaction 1 doesn't balance: its $ postings add up to 0.01")]
    [DataRow(6, "transaction 1 appears in two places")]
    [DataRow(7, "has a different date or description than its first row")]
    [DataRow(8, "has 3 fields; the header has 14")]
    [DataRow(9, "a quoted field is never closed")]
    [DataRow(10, "the txnidx 'x' is not a positive whole number")]
    [DataRow(11, "the account is empty")]
    public void BrokenInput_IsRefused(int which, string message)
    {
        var csv = which switch
        {
            0 => "",
            1 => Header.Replace("\"amount\"", "\"amt\"") + "\r\n",
            2 => Csv("\r\n", Row("1", "2026-07-03", "S", "assets:a", "1,500.00"), Row("1", "2026-07-03", "S", "income:b", "-1500.00")),
            3 => Csv("\r\n", Row("1", "2026-07-03", "S", "assets:a", "$1.00")),
            4 => Csv("\r\n", Row("1", "07/03/2026", "S", "assets:a", "1.00")),
            5 => Csv("\r\n", Row("1", "2026-07-03", "S", "assets:a", "1.01"), Row("1", "2026-07-03", "S", "income:b", "-1.00")),
            6 => Csv("\r\n", Pair[0], Pair[1], Row("2", "2026-07-04", "T", "assets:a", "-1.00"), Row("2", "2026-07-04", "T", "expenses:b", "1.00"), Row("1", "2026-07-05", "U", "assets:a", "0.00")),
            7 => Csv("\r\n", Row("1", "2026-07-03", "S", "assets:a", "-1.00"), Row("1", "2026-07-04", "S", "expenses:b", "1.00")),
            8 => Csv("\r\n", "\"1\",\"2026-07-03\",\"S\""),
            9 => Header + "\r\n\"1\",\"2026-07-03\r\n",
            10 => Csv("\r\n", Row("x", "2026-07-03", "S", "assets:a", "1.00")),
            _ => Csv("\r\n", Row("1", "2026-07-03", "S", "", "1.00")),
        };

        var error = Assert.ThrowsExactly<LedgerFormatException>(() => HledgerCsvReader.Read(csv));
        Assert.Contains(message, error.Message);
    }

    [TestMethod]
    public void AccountPostedTwiceInOneTransaction_IsOneEntryWithTheSum()
    {
        var t = HledgerCsvReader.Read(Csv("\r\n",
            Row("1", "2026-07-03", "S", "assets:a", "-1.00"),
            Row("1", "2026-07-03", "S", "assets:a", "-2.00"),
            Row("1", "2026-07-03", "S", "expenses:b", "3.00")));

        var entry = LedgerAccounts.EntriesFor(t, "assets:a").Single();

        Assert.AreEqual(-3.00m, entry.Amount);
        Assert.HasCount(1, entry.Others);
    }

    [TestMethod]
    public void AccountInTwoCommoditiesInOneTransaction_IsRefused()
    {
        var t = HledgerCsvReader.Read(Csv("\r\n",
            Row("1", "2026-07-03", "S", "assets:a", "-1.00", "$"),
            Row("1", "2026-07-03", "S", "assets:a", "-1.00", "EUR"),
            Row("1", "2026-07-03", "S", "expenses:b", "1.00", "$"),
            Row("1", "2026-07-03", "S", "expenses:b", "1.00", "EUR")));

        var error = Assert.ThrowsExactly<LedgerFormatException>(() => LedgerAccounts.EntriesFor(t, "assets:a"));
        Assert.Contains("in more than one commodity ($, EUR)", error.Message);
    }
}
