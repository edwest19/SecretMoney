// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Globalization;
using System.Text;
using SecretMoney.Qif;
using static SecretMoney.Core.Tests.TestHelper;

namespace SecretMoney.Core.Tests;

/// <summary>
/// Known-answer tests that lock docs/qif-format.md, version 1. Never change an expected value here
/// to make a test pass: a format change needs the owner's approval, a new version of the spec, and a
/// new import test in MYM 12.
/// </summary>
[TestClass]
public sealed class QifWriterTests
{
    // The example in docs/qif-format.md, exactly.
    private const string SpecExample =
        "!Type:Bank\r\n" +
        "D11/03/2026\r\n" +
        "T-52.40\r\n" +
        "PEXAMPLE GROCERY\r\n" +
        "Lgroceries\r\n" +
        "^\r\n" +
        "D11/15/2026\r\n" +
        "T1500.00\r\n" +
        "PEXAMPLE PAYROLL\r\n" +
        "MPay for 1-15 November\r\n" +
        "Lsalary\r\n" +
        "^\r\n";

    private static readonly QifTransaction[] SpecEntries =
    [
        new(new DateOnly(2026, 11, 3), -52.40m, "EXAMPLE GROCERY", "groceries"),
        new(new DateOnly(2026, 11, 15), 1500.00m, "EXAMPLE PAYROLL", "salary", "Pay for 1-15 November"),
    ];

    private static QifTransaction Entry(decimal amount = -1.00m, string payee = "PAYEE", string category = "misc", string? memo = null) =>
        new(new DateOnly(2026, 11, 10), amount, payee, category, memo);

    [TestMethod]
    public void SpecExample_KnownAnswer()
    {
        var bytes = QifWriter.WriteBytes(QifAccountType.Bank, SpecEntries);

        CollectionAssert.AreEqual(Encoding.ASCII.GetBytes(SpecExample), bytes);
        Assert.HasCount(144, bytes);
    }

    [TestMethod]
    public void SpecDocument_ShowsTheSameExample()
    {
        var lines = ReadRepoFile("docs", "qif-format.md").Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        var heading = lines.IndexOf("## Example");
        Assert.IsGreaterThanOrEqualTo(0, heading, "docs/qif-format.md needs a '## Example' section.");
        var start = lines.IndexOf("```text", heading) + 1;
        var end = lines.IndexOf("```", start);
        Assert.IsGreaterThan(0, start, "The example must be a ```text block.");

        var shown = string.Concat(lines.Skip(start).Take(end - start).Select(l => l + "\r\n"));
        Assert.AreEqual(SpecExample, shown);
        Assert.Contains($"version {QifWriter.FormatVersion}", lines[0]);
    }

    [TestMethod]
    [DataRow("t1ccard.qif", QifAccountType.CCard, "T1")]
    [DataRow("t1bbank.qif", QifAccountType.Bank, "T1B")]
    public void ReproducesRoundTwoChargeFiles(string file, QifAccountType type, string prefix)
    {
        QifTransaction[] entries =
        [
            new(new DateOnly(2026, 11, 2), -31.31m, $"{prefix} CARD CHARGE TEST", "groceries", "Charge - should raise the amount owed"),
            new(new DateOnly(2026, 11, 5), 200.00m, $"{prefix} CARD PAYMENT TEST", "Transfer", "Payment - should lower the amount owed"),
        ];

        CollectionAssert.AreEqual(File.ReadAllBytes(Fixture(file)), QifWriter.WriteBytes(type, entries));
    }

    [TestMethod]
    public void ReproducesRoundTwoThreeEntryFile()
    {
        QifTransaction[] entries =
        [
            new(new DateOnly(2026, 11, 3), -52.40m, "T3 GROCERY TEST", "groceries"),
            new(new DateOnly(2026, 11, 5), -88.10m, "T3 GAS TEST", "utilities"),
            new(new DateOnly(2026, 11, 15), 1500.00m, "T3 PAYROLL TEST", "salary"),
        ];

        CollectionAssert.AreEqual(File.ReadAllBytes(Fixture("t3three.qif")), QifWriter.WriteBytes(QifAccountType.Bank, entries));
    }

    [TestMethod]
    public void ReproducesRoundTwoTransferFiles()
    {
        var chase = new QifTransaction(new DateOnly(2026, 11, 20), -75.00m, "T4 TRANSFER TO SAVINGS", "Transfer", "Transfer test - Chase side");
        var savings = new QifTransaction(new DateOnly(2026, 11, 20), 75.00m, "T4 TRANSFER FROM CHASE", "Transfer", "Transfer test - Savings side");

        CollectionAssert.AreEqual(File.ReadAllBytes(Fixture("t4chk.qif")), QifWriter.WriteBytes(QifAccountType.Bank, [chase]));
        CollectionAssert.AreEqual(File.ReadAllBytes(Fixture("t4sav.qif")), QifWriter.WriteBytes(QifAccountType.Bank, [savings]));
    }

    [TestMethod]
    public void RefusesRoundTwoLongPayee()
    {
        // t2long.qif's first entry: an 80-character payee, more than MYM 12 keeps.
        var payee = string.Concat(Enumerable.Range(1, 8).Select(i => $"PAYEE-{i * 10:0000}"));

        var error = Assert.ThrowsExactly<QifFormatException>(() => QifWriter.Write(QifAccountType.Bank, [Entry(payee: payee)]));
        Assert.Contains("payee is 80 characters; MYM 12 keeps 25", error.Message);
    }

    [TestMethod]
    public void AmountsAndDates_AreWrittenTheSameInEveryCulture()
    {
        var saved = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = new CultureInfo("de-DE");
            var text = QifWriter.Write(QifAccountType.Bank, SpecEntries);
            Assert.AreEqual(SpecExample, text);
        }
        finally
        {
            CultureInfo.CurrentCulture = saved;
        }
    }

    [TestMethod]
    [DataRow("-0.01", "T-0.01")]
    [DataRow("0.5", "T0.50")]
    [DataRow("5", "T5.00")]
    [DataRow("-1234567.89", "T-1234567.89")]
    public void Amount_HasTwoDecimalsAndNoSeparators(string amount, string line)
    {
        var text = QifWriter.Write(QifAccountType.Bank, [Entry(amount: decimal.Parse(amount, CultureInfo.InvariantCulture))]);

        Assert.AreEqual(line, text.Split("\r\n")[2]);
    }

    [TestMethod]
    public void Date_HasTwoDigitMonthAndDay()
    {
        var text = QifWriter.Write(QifAccountType.Bank, [new QifTransaction(new DateOnly(2027, 1, 2), -1m, "P", "c")]);

        Assert.AreEqual("D01/02/2027", text.Split("\r\n")[1]);
    }

    [TestMethod]
    public void NoMemo_WritesNoMLine()
    {
        foreach (var memo in new string?[] { null, "" })
        {
            var text = QifWriter.Write(QifAccountType.Bank, [Entry(memo: memo)]);
            Assert.AreEqual("!Type:Bank\r\nD11/10/2026\r\nT-1.00\r\nPPAYEE\r\nLmisc\r\n^\r\n", text);
        }
    }

    [TestMethod]
    public void FieldsAtTheirLimits_AreWritten()
    {
        var entry = Entry(payee: new string('P', 25), memo: new string('M', 40), category: new string('c', 20));

        var lines = QifWriter.Write(QifAccountType.Bank, [entry]).Split("\r\n");

        CollectionAssert.AreEqual(
            new[] { "!Type:Bank", "D11/10/2026", "T-1.00", "P" + new string('P', 25), "M" + new string('M', 40), "L" + new string('c', 20), "^", "" },
            lines);
    }

    [TestMethod]
    [DataRow(0, "the amount is zero")]
    [DataRow(1, "is not a whole number of cents")]
    [DataRow(2, "the payee is empty")]
    [DataRow(3, "the payee is 26 characters; MYM 12 keeps 25")]
    [DataRow(4, "the memo is 41 characters; MYM 12 keeps 40")]
    [DataRow(5, "the category is 21 characters; MYM 12 keeps 20")]
    [DataRow(6, "contains a colon")]
    [DataRow(7, "starts with '['")]
    [DataRow(8, "the category is empty")]
    [DataRow(9, "is not clean QIF text")]
    [DataRow(10, "is not clean QIF text")]
    [DataRow(11, "is not clean QIF text")]
    [DataRow(12, "is not clean QIF text")]
    public void BrokenEntries_AreRefused(int which, string message)
    {
        QifTransaction entry = which switch
        {
            0 => Entry(amount: 0m),
            1 => Entry(amount: -1.005m),
            2 => Entry(payee: ""),
            3 => Entry(payee: new string('P', 26)),
            4 => Entry(memo: new string('M', 41)),
            5 => Entry(category: new string('c', 21)),
            6 => Entry(category: "food:groceries"),
            7 => Entry(category: "[Savings]"),
            8 => Entry(category: ""),
            9 => Entry(payee: " PAYEE"),
            10 => Entry(payee: "PAY\tEE"),
            11 => Entry(memo: "Café"),
            _ => Entry(category: "two  spaces"),
        };

        var error = Assert.ThrowsExactly<QifFormatException>(() => QifWriter.Write(QifAccountType.Bank, [Entry(), entry]));
        Assert.Contains("Entry 2:", error.Message);
        Assert.Contains(message, error.Message);
    }

    [TestMethod]
    public void EmptyFile_IsRefused()
    {
        var error = Assert.ThrowsExactly<QifFormatException>(() => QifWriter.Write(QifAccountType.Bank, []));
        Assert.Contains("at least one entry", error.Message);
    }

    private static string Fixture(string name) => Path.Combine(RepoRoot(), "tests", "qif", name);
}
