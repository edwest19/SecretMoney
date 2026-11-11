// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text;
using SecretMoney.Export;
using SecretMoney.Qif;
using static SecretMoney.Core.Tests.TestHelper;

namespace SecretMoney.Core.Tests;

/// <summary>
/// Known-answer tests that lock docs/settings-format.md, version 1. Never change an expected value here
/// to make a test pass: a format change needs the owner's approval, a new version of the spec, and a plan
/// for settings files already on disk.
/// </summary>
[TestClass]
public sealed class SettingsFileTests
{
    // The example in docs/settings-format.md, exactly.
    private const string Example =
        "{\n" +
        "  \"formatVersion\": 1,\n" +
        "  \"hledgerPath\": \"C:\\\\Tools\\\\hledger.exe\",\n" +
        "  \"reviewAccount\": \"expenses:review\",\n" +
        "  \"accounts\": [\n" +
        "    {\n" +
        "      \"hledgerAccount\": \"assets:bank:checking\",\n" +
        "      \"secretBeansName\": \"checking\",\n" +
        "      \"fileName\": \"CHECKING\",\n" +
        "      \"type\": \"Bank\",\n" +
        "      \"lastExported\": \"2026-07-31\"\n" +
        "    },\n" +
        "    {\n" +
        "      \"hledgerAccount\": \"liabilities:creditcard:visa\",\n" +
        "      \"secretBeansName\": \"visa\",\n" +
        "      \"fileName\": \"VISA\",\n" +
        "      \"type\": \"CCard\",\n" +
        "      \"lastExported\": null\n" +
        "    }\n" +
        "  ],\n" +
        "  \"categories\": {\n" +
        "    \"expenses:food:groceries\": \"Groceries\",\n" +
        "    \"expenses:utilities:gas\": \"Gas \\u0026 electric\"\n" +
        "  }\n" +
        "}\n";

    private static readonly ExportSettings ExampleSettings = new(
        @"C:\Tools\hledger.exe",
        "expenses:review",
        [
            new MymAccount("assets:bank:checking", "checking", "CHECKING", QifAccountType.Bank, new DateOnly(2026, 7, 31)),
            new MymAccount("liabilities:creditcard:visa", "visa", "VISA", QifAccountType.CCard),
        ],
        new Dictionary<string, string> { ["expenses:utilities:gas"] = "Gas & electric", ["expenses:food:groceries"] = "Groceries" });

    [TestMethod]
    public void Example_KnownAnswer()
    {
        var text = SettingsFile.Write(ExampleSettings);

        Assert.AreEqual(Example, text);
        Assert.HasCount(607, Encoding.UTF8.GetBytes(text));
    }

    [TestMethod]
    public void SpecDocument_ShowsTheSameExample()
    {
        var lines = ReadRepoFile("docs", "settings-format.md").Split('\n').Select(l => l.TrimEnd('\r')).ToList();
        var start = lines.IndexOf("```json", lines.IndexOf("## Example")) + 1;
        var end = lines.IndexOf("```", start);

        Assert.AreEqual(Example, string.Concat(lines.Skip(start).Take(end - start).Select(l => l + "\n")));
        Assert.Contains($"version {SettingsFile.FormatVersion}", lines[0]);
    }

    [TestMethod]
    public void Example_ReadsBackTheSame()
    {
        var settings = SettingsFile.Read(Example);

        Assert.AreEqual(@"C:\Tools\hledger.exe", settings.HledgerPath);
        Assert.AreEqual("expenses:review", settings.ReviewAccount);
        CollectionAssert.AreEqual(ExampleSettings.Accounts.ToArray(), settings.Accounts.ToArray());
        Assert.AreEqual("Gas & electric", settings.Categories["expenses:utilities:gas"]);
        Assert.AreEqual(Example, SettingsFile.Write(settings));
    }

    [TestMethod]
    public void EmptySettings_KnownAnswer()
    {
        const string empty = "{\n  \"formatVersion\": 1,\n  \"hledgerPath\": null,\n  \"reviewAccount\": \"expenses:review\",\n  \"accounts\": [],\n  \"categories\": {}\n}\n";

        Assert.AreEqual(empty, SettingsFile.Write(ExportSettings.Empty));
        Assert.HasCount(0, SettingsFile.Read(empty).Accounts);
    }

    [TestMethod]
    [DataRow(0, "isn't valid JSON")]
    [DataRow(1, "is format version 2; this SecretMoney reads version 1")]
    [DataRow(2, "has 'colour', which the format doesn't have")]
    [DataRow(3, "has no 'categories'")]
    [DataRow(4, "'type' is 'Savings'; it must be 'Bank' or 'CCard'")]
    [DataRow(5, "'lastExported' is '07/31/2026'; it must be YYYY-MM-DD or null")]
    [DataRow(6, "assets:bank:checking is listed twice")]
    [DataRow(7, "'reviewAccount' must be a non-empty string")]
    [DataRow(8, "The category for expenses:x must be a non-empty string")]
    [DataRow(9, "Account 1 'fileName' must be a non-empty string")]
    [DataRow(10, "has 'formatVersion' twice")]
    public void BrokenFiles_AreRefused(int which, string message)
    {
        var text = which switch
        {
            0 => "{ not json",
            1 => Example.Replace("\"formatVersion\": 1", "\"formatVersion\": 2"),
            2 => Example.Replace("\"reviewAccount\"", "\"colour\": \"red\",\n  \"reviewAccount\""),
            3 => Example[..Example.IndexOf(",\n  \"categories\"")] + "\n}\n",
            4 => Example.Replace("\"type\": \"Bank\"", "\"type\": \"Savings\""),
            5 => Example.Replace("\"2026-07-31\"", "\"07/31/2026\""),
            6 => Example.Replace("liabilities:creditcard:visa", "assets:bank:checking"),
            7 => Example.Replace("\"reviewAccount\": \"expenses:review\"", "\"reviewAccount\": \"\""),
            8 => Example.Replace("\"expenses:utilities:gas\": \"Gas \\u0026 electric\"", "\"expenses:x\": 5"),
            9 => Example.Replace("\"fileName\": \"CHECKING\"", "\"fileName\": null"),
            _ => Example.Replace("\"formatVersion\": 1,", "\"formatVersion\": 1,\n  \"formatVersion\": 1,"),
        };

        var error = Assert.ThrowsExactly<SettingsFormatException>(() => SettingsFile.Read(text));
        Assert.Contains(message, error.Message);
    }
}
