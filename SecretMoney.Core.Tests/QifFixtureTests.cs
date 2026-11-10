// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using static SecretMoney.Core.Tests.TestHelper;

namespace SecretMoney.Core.Tests;

/// <summary>
/// Keeps the QIF fixtures in tests/qif exactly as MYM 12 was tested with them
/// (tests/qif/README.md): ASCII only, CRLF line endings including the last line, a
/// !Type header, and every record closed by "^". .gitattributes keeps Git from
/// changing their bytes; this test catches it if something else does.
/// </summary>
[TestClass]
public sealed class QifFixtureTests
{
    private static IEnumerable<string> Fixtures() =>
        Directory.EnumerateFiles(Path.Combine(RepoRoot(), "tests", "qif"), "*.qif");

    [TestMethod]
    public void RoundTwoFixturesArePresent()
    {
        var names = Fixtures().Select(Path.GetFileName).Order(StringComparer.Ordinal).ToArray();

        CollectionAssert.AreEqual(
            new[] { "t1bbank.qif", "t1ccard.qif", "t2long.qif", "t3three.qif", "t4chk.qif", "t4sav.qif" },
            names);
    }

    [TestMethod]
    public void FixturesAreAsciiWithCrlfLineEndings()
    {
        foreach (var file in Fixtures())
        {
            var name = Path.GetFileName(file);
            var bytes = File.ReadAllBytes(file);

            Assert.IsTrue(bytes.All(b => b < 0x80), $"{name} contains a non-ASCII byte.");
            Assert.IsTrue(bytes.Length >= 2 && bytes[^2] == '\r' && bytes[^1] == '\n', $"{name} must end with CRLF.");
            for (var i = 0; i < bytes.Length; i++)
            {
                if (bytes[i] == '\n')
                {
                    Assert.IsTrue(i > 0 && bytes[i - 1] == '\r', $"{name} has a line feed without a carriage return at byte {i}.");
                }
                else if (bytes[i] == '\r')
                {
                    Assert.IsTrue(i + 1 < bytes.Length && bytes[i + 1] == '\n', $"{name} has a carriage return without a line feed at byte {i}.");
                }
            }
        }
    }

    [TestMethod]
    public void FixturesHaveATypeHeaderAndClosedRecords()
    {
        foreach (var file in Fixtures())
        {
            var name = Path.GetFileName(file);
            var lines = File.ReadAllText(file).Split("\r\n");

            Assert.AreEqual("", lines[^1], $"{name} must end with CRLF.");
            Assert.IsTrue(lines[0] is "!Type:Bank" or "!Type:CCard", $"{name} starts with '{lines[0]}'.");
            Assert.AreEqual("^", lines[^2], $"{name}'s last record isn't closed by '^'.");
            Assert.IsGreaterThan(0, lines.Count(l => l == "^"), $"{name} has no records.");
        }
    }
}
