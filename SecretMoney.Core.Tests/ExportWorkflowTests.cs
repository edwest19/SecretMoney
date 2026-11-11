// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Export;
using SecretMoney.Ledger;
using SecretMoney.Qif;
using static SecretMoney.Core.Tests.TestHelper;

namespace SecretMoney.Core.Tests;

/// <summary>
/// The steps the export window takes, in SecretMoney.Core: finding the period, choosing accounts and
/// categories, running hledger, writing the files and remembering the last date (docs/export-rules.md).
/// </summary>
[TestClass]
public sealed class ExportWorkflowTests
{
    private static IReadOnlyList<LedgerTransaction> Generated => HledgerCsvReader.Read(ReadRepoFile("tests", "secretbeans", "print-generated.csv"));

    private static IReadOnlyList<LedgerTransaction> Opening => HledgerCsvReader.Read(ReadRepoFile("tests", "secretbeans", "print-opening.csv"));

    private static IReadOnlyList<MymAccount> Found =>
        ExportRules.AccountsInGeneratedJournal(ReadRepoFile("tests", "secretbeans", "period", "journal", "generated.journal"));

    private static ExportPlan JulyPlan() =>
        ExportPlanner.Plan(Generated, Opening, ExportSettings.Empty with { Accounts = Found }, new DateOnly(2026, 7, 21), new DateOnly(2026, 7, 31));

    private static void Touch(string path)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, "");
    }

    [TestMethod]
    public void FindLatest_PicksTheLastPeriodWithAJournal_AsSecretBeansDoes()
    {
        using var root = new TempFolder();
        Touch(root.PathOf("2026-09 September", "journal", "main.journal"));
        Touch(root.PathOf("2026-10 October", "journal", "main.journal"));
        Directory.CreateDirectory(root.PathOf("2026-11 November", "journal")); // being built: no main.journal yet
        Touch(root.PathOf("notes", "journal", "main.journal"));               // not a period name
        Touch(root.PathOf("2026-12", "journal", "main.journal"));             // no space after the month

        Assert.AreEqual(root.PathOf("2026-10 October"), SecretBeansPeriod.FindLatest(root.FullPath));
        Assert.AreEqual(root.PathOf("2026-10 October", "journal", "generated.journal"), SecretBeansPeriod.Journal(root.PathOf("2026-10 October"), "generated.journal"));
    }

    [TestMethod]
    public void FindLatest_IsNullWhenThereIsNoPeriod()
    {
        using var root = new TempFolder();

        Assert.IsNull(SecretBeansPeriod.FindLatest(root.FullPath));
        Assert.IsNull(SecretBeansPeriod.FindLatest(root.PathOf("missing")));
    }

    [TestMethod]
    public void HledgerArguments_AreTheRecordedCommands()
    {
        CollectionAssert.AreEqual(new[] { "check", "-f", @"C:\Ledger\2026-07 July\journal\main.journal" },
            HledgerRunner.CheckArguments(@"C:\Ledger\2026-07 July\journal\main.journal").ToArray());
        CollectionAssert.AreEqual(new[] { "print", "-x", "-O", "csv", "-f", "g.journal", "-o", "out.csv" },
            HledgerRunner.PrintArguments("g.journal", "out.csv").ToArray());
    }

    [TestMethod]
    public async Task Hledger_ThatIsNotThere_IsReportedByPath()
    {
        using var folder = new TempFolder();
        var missing = folder.PathOf("hledger.exe");

        var error = await Assert.ThrowsExactlyAsync<HledgerException>(() => HledgerRunner.PrintCsvAsync(missing, "g.journal", folder.FullPath));
        Assert.Contains($"hledger.exe isn't at {missing}", error.Message);
        Assert.HasCount(0, Directory.GetFiles(folder.FullPath));
    }

    [TestMethod]
    public void AccountChoices_FirstTime_OffersEveryAccountFound()
    {
        var choices = ExportRules.AccountChoices(Found, []);

        CollectionAssert.AreEqual(Found.ToArray(), choices.Select(c => c.Account).ToArray());
        Assert.IsTrue(choices.All(c => c.Chosen));
    }

    [TestMethod]
    public void AccountChoices_KeepWhatWasChosen_AndListAccountsNoLongerFound()
    {
        var chosen = new[]
        {
            Found[2] with { FileName = "CARD", LastExported = new DateOnly(2026, 7, 31) },
            new MymAccount("assets:bank:old", "old", "OLD", QifAccountType.Bank),
        };

        var choices = ExportRules.AccountChoices(Found, chosen);

        CollectionAssert.AreEqual(
            new[] { ("CHECKING", false), ("SAVINGS", false), ("CARD", true), ("OLD", true) },
            choices.Select(c => (c.Account.FileName, c.Chosen)).ToArray());
        Assert.AreEqual(new DateOnly(2026, 7, 31), choices[2].Account.LastExported);
    }

    [TestMethod]
    public void CategoryChoices_ListTheCategoriesTheChosenAccountsUse()
    {
        var settings = ExportSettings.Empty with
        {
            Accounts = Found,
            Categories = new Dictionary<string, string> { ["expenses:food:dining"] = "Eating out" },
        };

        var choices = ExportRules.CategoryChoices(Generated, settings);

        // Transfers, the check awaiting review and the other side of a transfer are not categories.
        CollectionAssert.AreEqual(
            new[]
            {
                new CategoryChoice("expenses:food:dining", "dining", "Eating out"),
                new CategoryChoice("expenses:food:groceries", "groceries", "groceries"),
                new CategoryChoice("expenses:utilities:gas", "gas", "gas"),
                new CategoryChoice("income:interest", "interest", "interest"),
                new CategoryChoice("income:salary", "salary", "salary"),
            },
            choices.ToArray());

        var kept = ExportRules.ChosenCategories(choices);
        Assert.HasCount(1, kept);
        Assert.AreEqual("Eating out", kept["expenses:food:dining"]);
    }

    [TestMethod]
    public void Write_WritesTheFilesWithEntries_AndReplacesOldOnes()
    {
        using var folder = new TempFolder();
        var plan = JulyPlan();
        File.WriteAllText(folder.PathOf("VISA.QIF"), "last month's file");

        Assert.HasCount(1, ExportFiles.Existing(plan, folder.FullPath));
        var written = ExportFiles.Write(plan, folder.FullPath);

        CollectionAssert.AreEqual(new[] { "CHECKING.QIF", "SAVINGS.QIF", "VISA.QIF" }, written.Select(Path.GetFileName).ToArray());
        foreach (var file in plan.Files)
        {
            CollectionAssert.AreEqual(file.ToBytes(), File.ReadAllBytes(folder.PathOf(file.FileName)));
        }
    }

    [TestMethod]
    public void Write_RefusesAPlanThatCannotBeExported()
    {
        using var folder = new TempFolder();
        var stopped = ExportPlanner.Plan(Generated, Opening, ExportSettings.Empty with { Accounts = Found }, new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31));

        var error = Assert.ThrowsExactly<InvalidOperationException>(() => ExportFiles.Write(stopped, folder.FullPath));
        Assert.Contains("1 problem(s) stop it", error.Message);
        Assert.HasCount(0, Directory.GetFiles(folder.FullPath));
        Assert.ThrowsExactly<DirectoryNotFoundException>(() => ExportFiles.Write(JulyPlan(), folder.PathOf("missing")));
    }

    [TestMethod]
    public void AfterExport_EveryAccountInThePlanIsExportedToItsLastDate()
    {
        var settings = ExportSettings.Empty with { Accounts = [.. Found, new MymAccount("assets:bank:old", "old", "OLD", QifAccountType.Bank)] };
        var plan = ExportPlanner.Plan(Generated, Opening, settings with { Accounts = Found }, new DateOnly(2026, 7, 21), new DateOnly(2026, 7, 31));

        var after = settings.AfterExport(plan);

        CollectionAssert.AreEqual(
            new DateOnly?[] { new DateOnly(2026, 7, 31), new DateOnly(2026, 7, 31), new DateOnly(2026, 7, 31), null },
            after.Accounts.Select(a => a.LastExported).ToArray());
    }

    [TestMethod]
    public void AfterExport_NeverMovesALastExportDateBack()
    {
        // VISA was exported to 2026-07-31; CHECKING and SAVINGS are exported for the first time, to 2026-07-25.
        var settings = ExportSettings.Empty with { Accounts = [Found[0], Found[1], Found[2] with { LastExported = new DateOnly(2026, 7, 31) }] };
        var plan = ExportPlanner.Plan(Generated, Opening, settings, new DateOnly(2026, 7, 21), new DateOnly(2026, 7, 25));
        Assert.IsTrue(plan.CanExport);
        Assert.HasCount(0, plan.Files[2].Entries);

        var after = settings.AfterExport(plan);

        CollectionAssert.AreEqual(
            new DateOnly?[] { new DateOnly(2026, 7, 25), new DateOnly(2026, 7, 25), new DateOnly(2026, 7, 31) },
            after.Accounts.Select(a => a.LastExported).ToArray());
    }
}
