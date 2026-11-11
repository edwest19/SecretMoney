// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text;
using SecretMoney.Export;
using SecretMoney.Ledger;
using SecretMoney.Qif;
using static SecretMoney.Core.Tests.TestHelper;

namespace SecretMoney.Core.Tests;

/// <summary>
/// docs/export-rules.md applied to the synthetic SecretBeans period in tests/secretbeans (hledger 1.51.2's
/// real output), and to small hand-made transactions for each rule that stops an export.
/// </summary>
[TestClass]
public sealed class ExportPlannerTests
{
    private static readonly DateOnly July1 = new(2026, 7, 1);
    private static readonly DateOnly July21 = new(2026, 7, 21);
    private static readonly DateOnly July31 = new(2026, 7, 31);

    private static IReadOnlyList<LedgerTransaction> Generated => HledgerCsvReader.Read(ReadRepoFile("tests", "secretbeans", "print-generated.csv"));

    private static IReadOnlyList<LedgerTransaction> Opening => HledgerCsvReader.Read(ReadRepoFile("tests", "secretbeans", "print-opening.csv"));

    private static IReadOnlyList<MymAccount> Accounts =>
        ExportRules.AccountsInGeneratedJournal(ReadRepoFile("tests", "secretbeans", "period", "journal", "generated.journal"));

    private static ExportSettings Settings => ExportSettings.Empty with { Accounts = Accounts };

    private static string Text(ExportFile file) => Encoding.ASCII.GetString(file.ToBytes());

    [TestMethod]
    public void Accounts_AreFoundFromSecretBeansBlockLines()
    {
        CollectionAssert.AreEqual(
            new[]
            {
                new MymAccount("assets:bank:checking", "checking", "CHECKING", QifAccountType.Bank),
                new MymAccount("assets:bank:savings", "savings", "SAVINGS", QifAccountType.Bank),
                new MymAccount("liabilities:creditcard:visa", "visa", "VISA", QifAccountType.CCard),
            },
            Accounts.ToArray());
    }

    [TestMethod]
    public void WholeJuly_IsStoppedByTheCheckAwaitingReview()
    {
        var plan = ExportPlanner.Plan(Generated, Opening, Settings, July1, July31);

        Assert.IsFalse(plan.CanExport);
        var problem = plan.Problems.Single();
        Assert.AreEqual("assets:bank:checking", problem.Account);
        Assert.AreEqual(new DateOnly(2026, 7, 20), problem.Date);
        Assert.AreEqual("CHECK 501", problem.Description);
        Assert.Contains("it is in expenses:review:check", problem.Reason);
    }

    [TestMethod]
    public void WholeJuly_BalancesStartAtTheOpeningBalances()
    {
        var files = ExportPlanner.Plan(Generated, Opening, Settings, July1, July31).Files;

        CollectionAssert.AreEqual(new[] { 14884.32m, 2000.00m, -1250.00m }, files.Select(f => f.BalanceBefore).ToArray());
        CollectionAssert.AreEqual(new[] { 13562.57m, 4401.23m, -1081.31m }, files.Select(f => f.BalanceAfter).ToArray());
    }

    [TestMethod]
    public void FromJuly21_KnownAnswer()
    {
        var plan = ExportPlanner.Plan(Generated, Opening, Settings, July21, July31);

        Assert.IsTrue(plan.CanExport);
        Assert.HasCount(0, plan.Problems);
        CollectionAssert.AreEqual(new[] { "CHECKING.QIF", "SAVINGS.QIF", "VISA.QIF" }, plan.Files.Select(f => f.FileName).ToArray());
        CollectionAssert.AreEqual(new[] { 16168.82m, 2000.00m, -1281.31m }, plan.Files.Select(f => f.BalanceBefore).ToArray());
        CollectionAssert.AreEqual(new[] { 13562.57m, 4401.23m, -1081.31m }, plan.Files.Select(f => f.BalanceAfter).ToArray());

        Assert.AreEqual(
            "!Type:Bank\r\n" +
            "D07/21/2026\r\nT-2400.00\r\nPOnline Transfer to SAV ..\r\nMOnline Transfer to SAV ...3456 transacti\r\nLTransfer\r\n^\r\n" +
            "D07/22/2026\r\nT-6.25\r\nPCAFE ECLAIR, \"THE BEST\"\r\nLdining\r\n^\r\n" +
            "D07/25/2026\r\nT-200.00\r\nPPayment Thank You - Web\r\nLTransfer\r\n^\r\n",
            Text(plan.Files[0]));
        Assert.AreEqual(
            "!Type:Bank\r\n" +
            "D07/21/2026\r\nT2400.00\r\nPOnline Transfer to SAV ..\r\nMOnline Transfer to SAV ...3456 transacti\r\nLTransfer\r\n^\r\n" +
            "D07/31/2026\r\nT1.23\r\nPINTEREST PAYMENT\r\nLinterest\r\n^\r\n",
            Text(plan.Files[1]));
        Assert.AreEqual(
            "!Type:CCard\r\n" +
            "D07/25/2026\r\nT200.00\r\nPPayment Thank You - Web\r\nLTransfer\r\n^\r\n",
            Text(plan.Files[2]));
    }

    [TestMethod]
    public void LastExported_StartsThatAccountTheDayAfter()
    {
        var settings = Settings with { Accounts = [Accounts[0] with { LastExported = new DateOnly(2026, 7, 22) }, Accounts[1], Accounts[2]] };

        var plan = ExportPlanner.Plan(Generated, Opening, settings, July21, July31);

        Assert.AreEqual(new DateOnly(2026, 7, 23), plan.Files[0].From);
        Assert.AreEqual(July21, plan.Files[1].From);
        Assert.AreEqual("Payment Thank You - Web", plan.Files[0].Entries.Single().Payee);
        Assert.AreEqual(13762.57m, plan.Files[0].BalanceBefore);
    }

    [TestMethod]
    public void ChosenCategoryName_ReplacesTheDefault()
    {
        var settings = Settings with { Categories = new Dictionary<string, string> { ["expenses:food:dining"] = "Eating out" } };

        var checking = ExportPlanner.Plan(Generated, Opening, settings, July21, July31).Files[0];

        Assert.AreEqual("Eating out", checking.Entries[1].Category);
    }

    [TestMethod]
    public void NothingInRange_WritesNoEntries_AndCannotExport()
    {
        var plan = ExportPlanner.Plan(Generated, Opening, Settings, new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));

        Assert.HasCount(0, plan.Problems);
        Assert.IsTrue(plan.Files.All(f => f.Entries.Count == 0 && f.BalanceBefore == f.BalanceAfter));
        Assert.IsFalse(plan.CanExport);
    }

    // Hand-made transactions for the rules that stop an export.

    private static readonly MymAccount Bank = new("assets:bank:a", "a", "A", QifAccountType.Bank);

    private static LedgerTransaction T(int index, string description, params (string Account, decimal Amount, string Commodity)[] postings) =>
        new(index, new DateOnly(2026, 7, 10), description, postings.Select(p => new LedgerPosting(p.Account, p.Amount, p.Commodity)).ToArray());

    private static ExportPlan PlanOne(LedgerTransaction transaction, ExportSettings? settings = null) =>
        ExportPlanner.Plan([transaction], [], settings ?? ExportSettings.Empty with { Accounts = [Bank] }, July1, July31);

    [TestMethod]
    [DataRow("expenses:unknown", "it is in expenses:unknown")]
    [DataRow("expenses:review", "it is in expenses:review.")]
    [DataRow("expenses:review:check", "it is in expenses:review:check")]
    [DataRow("equity:start", "it goes to equity:start")]
    public void UncategorisedOrEquity_StopsTheExport(string other, string reason)
    {
        var plan = PlanOne(T(1, "SHOP", ("assets:bank:a", -5m, "$"), (other, 5m, "$")));

        Assert.IsFalse(plan.CanExport);
        Assert.Contains(reason, plan.Problems.Single().Reason);
    }

    [TestMethod]
    public void Split_StopsTheExport()
    {
        var plan = PlanOne(T(1, "SHOP", ("assets:bank:a", -5m, "$"), ("expenses:x", 3m, "$"), ("expenses:y", 2m, "$")));

        Assert.Contains("it has 2 other postings; splits aren't written", plan.Problems.Single().Reason);
    }

    [TestMethod]
    public void OtherCommodity_StopsTheExport()
    {
        var plan = PlanOne(T(1, "SHOP", ("assets:bank:a", -5m, "EUR"), ("expenses:x", 5m, "EUR")));

        Assert.Contains("its amount is in 'EUR', not '$'", plan.Problems.Single().Reason);
    }

    [TestMethod]
    public void ZeroAmount_StopsTheExport()
    {
        var plan = PlanOne(T(1, "SHOP", ("assets:bank:a", 5m, "$"), ("assets:bank:a", -5m, "$"), ("expenses:x", 0m, "$")));

        Assert.Contains("this account's amount in it is zero", plan.Problems.Single().Reason);
    }

    [TestMethod]
    public void EmptyDescription_StopsTheExport()
    {
        var plan = PlanOne(T(1, "  ", ("assets:bank:a", -5m, "$"), ("expenses:x", 5m, "$")));

        Assert.Contains("its description is empty", plan.Problems.Single().Reason);
    }

    [TestMethod]
    public void TwoAccountsWithOneCategoryName_StopTheExport_UntilOneIsRenamed()
    {
        LedgerTransaction[] transactions =
        [
            T(1, "CAR", ("assets:bank:a", -5m, "$"), ("expenses:auto:insurance", 5m, "$")),
            T(2, "HOUSE", ("assets:bank:a", -6m, "$"), ("expenses:house:insurance", 6m, "$")),
        ];
        var settings = ExportSettings.Empty with { Accounts = [Bank] };

        var clash = ExportPlanner.Plan(transactions, [], settings, July1, July31);
        var renamed = ExportPlanner.Plan(transactions, [], settings with { Categories = new Dictionary<string, string> { ["expenses:house:insurance"] = "home insurance" } }, July1, July31);

        Assert.Contains("expenses:auto:insurance and expenses:house:insurance would all be the MYM category 'insurance'", clash.Problems.Single().Reason);
        Assert.IsTrue(renamed.CanExport);
        CollectionAssert.AreEqual(new[] { "insurance", "home insurance" }, renamed.Files[0].Entries.Select(e => e.Category).ToArray());
    }

    [TestMethod]
    public void ExpenseNamedTransfer_ClashesWithTransfers()
    {
        LedgerTransaction[] transactions =
        [
            T(1, "MOVE", ("assets:bank:a", -5m, "$"), ("assets:bank:b", 5m, "$")),
            T(2, "FEE", ("assets:bank:a", -1m, "$"), ("expenses:bank:transfer", 1m, "$")),
        ];

        var plan = ExportPlanner.Plan(transactions, [], ExportSettings.Empty with { Accounts = [Bank] }, July1, July31);

        Assert.Contains("(transfers) and expenses:bank:transfer would all be the MYM category 'Transfer'", plan.Problems.Single().Reason);
    }

    [TestMethod]
    public void BadSettings_StopTheExport()
    {
        var transaction = T(1, "SHOP", ("assets:bank:a", -5m, "$"), ("expenses:x", 5m, "$"));
        var twoFiles = ExportSettings.Empty with { Accounts = [Bank, new MymAccount("assets:bank:b", "b", "A", QifAccountType.Bank)] };
        var badStem = ExportSettings.Empty with { Accounts = [Bank with { FileName = "TOOLONGNAME" }] };
        var badCategory = ExportSettings.Empty with { Accounts = [Bank], Categories = new Dictionary<string, string> { ["expenses:x"] = "food:out" } };

        Assert.Contains("No accounts are chosen", PlanOne(transaction, ExportSettings.Empty).Problems.Single().Reason);
        Assert.Contains("would all be written to A.QIF", PlanOne(transaction, twoFiles).Problems.Single().Reason);
        Assert.Contains("The file name 'TOOLONGNAME' must be 1 to 8", PlanOne(transaction, badStem).Problems.Single().Reason);
        Assert.IsTrue(PlanOne(transaction, badCategory).Problems.Any(p => p.Reason.Contains("The chosen category 'food:out' can't be written: it contains a colon")));
    }
}
