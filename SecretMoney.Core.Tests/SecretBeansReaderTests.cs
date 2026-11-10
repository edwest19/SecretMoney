// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Ledger;
using static SecretMoney.Core.Tests.TestHelper;

namespace SecretMoney.Core.Tests;

/// <summary>
/// Reads tests/secretbeans/print-*.csv: what hledger 1.51.2 printed, on Edwin's PC, for the synthetic
/// SecretBeans period in tests/secretbeans/period (docs/secretbeans-input.md).
/// </summary>
[TestClass]
public sealed class SecretBeansReaderTests
{
    private const string Checking = "assets:bank:checking";
    private const string Savings = "assets:bank:savings";
    private const string Visa = "liabilities:creditcard:visa";

    private static IReadOnlyList<LedgerTransaction> Printed(string name) =>
        HledgerCsvReader.Read(ReadRepoFile("tests", "secretbeans", $"print-{name}.csv"));

    private static (string Date, decimal Amount) Short(AccountEntry e) => (e.Transaction.Date.ToString("MM-dd"), e.Amount);

    [TestMethod]
    public void Generated_HasNineTransactionsInDateOrder()
    {
        var transactions = Printed("generated");

        Assert.HasCount(9, transactions);
        CollectionAssert.AreEqual(
            new[] { "07-03", "07-05", "07-08", "07-15", "07-20", "07-21", "07-22", "07-25", "07-31" },
            transactions.Select(t => t.Date.ToString("MM-dd")).ToArray());
        Assert.IsTrue(transactions.All(t => t.Date.Year == 2026 && t.Postings.Count == 2));
        Assert.IsTrue(transactions.SelectMany(t => t.Postings).All(p => p.Commodity == "$"));
    }

    [TestMethod]
    public void Generated_FirstTransaction_KnownAnswer()
    {
        var first = Printed("generated")[0];

        Assert.AreEqual(1, first.Index);
        Assert.AreEqual(new DateOnly(2026, 7, 3), first.Date);
        Assert.AreEqual("ACME GROCERY 123", first.Description);
        CollectionAssert.AreEqual(
            new[] { new LedgerPosting(Checking, -52.40m, "$"), new LedgerPosting("expenses:food:groceries", 52.40m, "$") },
            first.Postings.ToArray());
    }

    [TestMethod]
    public void Generated_KeepsHledgersTxnidx_NotThePrintedOrder()
    {
        // STOP & SHOP is the 8th transaction in generated.journal but is printed third, by date.
        var stopAndShop = Printed("generated")[2];

        Assert.AreEqual("STOP & SHOP 0042", stopAndShop.Description);
        Assert.AreEqual(8, stopAndShop.Index);
    }

    [TestMethod]
    public void Generated_DescriptionWithAccentsCommaAndQuotes_IsReadExactly()
    {
        var cafe = Printed("generated").Single(t => t.Date == new DateOnly(2026, 7, 22));

        Assert.AreEqual("CAFÉ ÉCLAIR, \"THE BEST\"", cafe.Description);
    }

    [TestMethod]
    public void Main_AmountsHaveNoThousandsSeparator_AndTheEquityPostingIsExplicit()
    {
        // opening.journal writes $14,884.32 and leaves the equity amount out; print -x -O csv gives plain numbers.
        var opening = Printed("main")[0];

        Assert.AreEqual("opening balances", opening.Description);
        CollectionAssert.AreEqual(
            new[] { 14884.32m, 2000.00m, -1250.00m, -15634.32m },
            opening.Postings.Select(p => p.Amount).ToArray());
        Assert.AreEqual("equity:opening balances", opening.Postings[3].Account);
        Assert.HasCount(10, Printed("main"));
    }

    [TestMethod]
    public void Opening_TotalsAreTheOpeningBalances()
    {
        var opening = Printed("opening");

        Assert.AreEqual(14884.32m, LedgerAccounts.Total(opening, Checking));
        Assert.AreEqual(2000.00m, LedgerAccounts.Total(opening, Savings));
        Assert.AreEqual(-1250.00m, LedgerAccounts.Total(opening, Visa));
        Assert.AreEqual(0m, LedgerAccounts.Total(opening, "assets:bank:nowhere"));
    }

    [TestMethod]
    public void Checking_HasSevenEntries_IncludingTheCardPaymentRecordedByVisa()
    {
        var entries = LedgerAccounts.EntriesFor(Printed("generated"), Checking);

        CollectionAssert.AreEqual(
            new[] { ("07-03", -52.40m), ("07-05", -88.10m), ("07-15", 1500.00m), ("07-20", -75.00m), ("07-21", -2400.00m), ("07-22", -6.25m), ("07-25", -200.00m) },
            entries.Select(Short).ToArray());

        var cardPayment = entries[^1];
        Assert.AreEqual("Payment Thank You - Web", cardPayment.Transaction.Description);
        CollectionAssert.AreEqual(new[] { new LedgerPosting(Visa, 200.00m, "$") }, cardPayment.Others.ToArray());
    }

    [TestMethod]
    public void Savings_HasTheTransferRecordedByChecking_AndItsInterest()
    {
        var entries = LedgerAccounts.EntriesFor(Printed("generated"), Savings);

        CollectionAssert.AreEqual(new[] { ("07-21", 2400.00m), ("07-31", 1.23m) }, entries.Select(Short).ToArray());
        CollectionAssert.AreEqual(new[] { new LedgerPosting(Checking, -2400.00m, "$") }, entries[0].Others.ToArray());
    }

    [TestMethod]
    public void Visa_HasItsChargeAndThePayment()
    {
        var entries = LedgerAccounts.EntriesFor(Printed("generated"), Visa);

        CollectionAssert.AreEqual(new[] { ("07-08", -31.31m), ("07-25", 200.00m) }, entries.Select(Short).ToArray());
        Assert.IsTrue(entries.All(e => e.Commodity == "$"));
    }

    [TestMethod]
    public void EntriesFor_MatchesTheWholeAccountName_Only()
    {
        var transactions = Printed("generated");

        Assert.HasCount(0, LedgerAccounts.EntriesFor(transactions, "assets:bank"));
        Assert.HasCount(0, LedgerAccounts.EntriesFor(transactions, "Assets:Bank:Checking"));
    }
}
