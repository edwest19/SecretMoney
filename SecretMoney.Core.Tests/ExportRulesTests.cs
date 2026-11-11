// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Export;

namespace SecretMoney.Core.Tests;

/// <summary>Each rule in docs/export-rules.md on its own.</summary>
[TestClass]
public sealed class ExportRulesTests
{
    [TestMethod]
    [DataRow("expenses:food:groceries", "groceries")]
    [DataRow("income:Salary", "salary")]
    [DataRow("expenses:house:utilities:gas", "gas")]
    [DataRow("expenses:café", "cafe")]
    [DataRow("expenses:abcdefghijklmnopqrstuvwxyz", "abcdefghijklmnopqrst")]
    public void DefaultCategory_IsTheLastPartInLowerCase(string account, string category)
    {
        Assert.AreEqual(category, ExportRules.DefaultCategory(account));
        Assert.IsNull(ExportRules.CategoryProblem(category));
    }

    [TestMethod]
    [DataRow("", "it is empty")]
    [DataRow(" x", "isn't clean QIF text")]
    [DataRow("abcdefghijklmnopqrstu", "it is 21 characters; MYM 12 keeps 20")]
    [DataRow("a:b", "it contains a colon")]
    [DataRow("[Savings]", "it starts with '['")]
    public void CategoryProblem_NamesTheRule(string category, string problem)
    {
        Assert.Contains(problem, ExportRules.CategoryProblem(category)!);
    }

    [TestMethod]
    [DataRow("checking", "CHECKING")]
    [DataRow("my card 2", "MYCARD2")]
    [DataRow("freedom-unlimited", "FREEDOM-")]
    [DataRow("sav_1", "SAV_1")]
    [DataRow("été", "T")]
    public void FileStem_IsUpperCaseDosName(string name, string stem)
    {
        Assert.AreEqual(stem, ExportRules.FileStem(name));
        Assert.IsTrue(ExportRules.IsValidFileStem(stem));
    }

    [TestMethod]
    public void IsValidFileStem_RefusesWhatDosCannotUse()
    {
        Assert.IsFalse(ExportRules.IsValidFileStem(""));
        Assert.IsFalse(ExportRules.IsValidFileStem("NINECHARS"));
        Assert.IsFalse(ExportRules.IsValidFileStem("lower"));
        Assert.IsFalse(ExportRules.IsValidFileStem("A.B"));
    }

    [TestMethod]
    public void TransferAndUncategorisedAccounts_AreRecognised()
    {
        Assert.IsTrue(ExportRules.IsTransferAccount("assets:bank:savings"));
        Assert.IsTrue(ExportRules.IsTransferAccount("liabilities:creditcard:visa"));
        Assert.IsFalse(ExportRules.IsTransferAccount("expenses:assets"));

        Assert.IsTrue(ExportRules.IsUncategorised("expenses:review", "expenses:review"));
        Assert.IsTrue(ExportRules.IsUncategorised("expenses:review:check", "expenses:review"));
        Assert.IsTrue(ExportRules.IsUncategorised("expenses:unknown", "expenses:review"));
        Assert.IsTrue(ExportRules.IsUncategorised("income:unknown", "expenses:review"));
        Assert.IsFalse(ExportRules.IsUncategorised("expenses:reviewers", "expenses:review"));
        Assert.IsFalse(ExportRules.IsUncategorised("expenses:unknowns", "expenses:review"));
    }

    [TestMethod]
    public void PayeeAndMemo_ShortDescriptionHasNoMemo()
    {
        Assert.AreEqual(("ACME GROCERY 123", (string?)null), ExportRules.PayeeAndMemo("ACME GROCERY 123"));
        Assert.AreEqual(("ABCDEFGHIJKLMNOPQRSTUVWXY", (string?)null), ExportRules.PayeeAndMemo("ABCDEFGHIJKLMNOPQRSTUVWXY"));
    }

    [TestMethod]
    public void PayeeAndMemo_LongDescriptionKeepsFortyInTheMemo()
    {
        Assert.AreEqual(
            ("Online Transfer to SAV ..", "Online Transfer to SAV ...3456 transacti"),
            ExportRules.PayeeAndMemo("Online Transfer to SAV ...3456 transaction#: 10293847566 07/21"));
        Assert.AreEqual(("ABCDEFGHIJKLMNOPQRSTUVWXY", "ABCDEFGHIJKLMNOPQRSTUVWXYZ"), ExportRules.PayeeAndMemo("ABCDEFGHIJKLMNOPQRSTUVWXYZ"));
    }
}
