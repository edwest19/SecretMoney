// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using SecretMoney.Qif;

namespace SecretMoney.Core.Tests;

/// <summary>The text rules in docs/qif-format.md, "Text", step by step.</summary>
[TestClass]
public sealed class QifTextTests
{
    [TestMethod]
    [DataRow("Café", "Cafe")]                                  // 1, 2: accents dropped
    [DataRow("Wait…", "Wait...")]                         // 1: ellipsis decomposes
    [DataRow("a b", "a b")]                               // 1: non-breaking space
    [DataRow("Ａｍａｚｏｎ", "Amazon")] // 1: full-width letters
    [DataRow("Joe’s “Diner”", "Joe's \"Diner\"")] // 3: curly quotes
    [DataRow("x – y — z", "x - y - z")]              // 3: dashes
    [DataRow("A\tB\r\nC", "A B C")]                            // 4: control characters
    [DataRow("ok\u0007", "ok")]                                // 4, 6: a control character at the end
    [DataRow("€5", "?5")]                                 // 5: euro sign
    [DataRow("\U0001F600ok", "?ok")]                           // 5: one ? for a character outside the BMP
    [DataRow("  X   Y  ", "X Y")]                              // 6: spaces
    [DataRow("T2 PUNCT #*&/'.,-() TEST", "T2 PUNCT #*&/'.,-() TEST")] // printable ASCII is kept
    public void Clean_FollowsTheSpec(string input, string expected)
    {
        Assert.AreEqual(expected, QifText.Clean(input));
        Assert.IsTrue(QifText.IsClean(expected));
    }

    [TestMethod]
    public void Clean_OfNothing_IsEmpty()
    {
        Assert.AreEqual("", QifText.Clean(null));
        Assert.AreEqual("", QifText.Clean(""));
        Assert.AreEqual("", QifText.Clean(" \t "));
    }

    [TestMethod]
    public void IsClean_IsFalseForTextCleanWouldChange()
    {
        Assert.IsFalse(QifText.IsClean(" lead"));
        Assert.IsFalse(QifText.IsClean("trail "));
        Assert.IsFalse(QifText.IsClean("two  spaces"));
        Assert.IsFalse(QifText.IsClean("Café"));
    }

    [TestMethod]
    [DataRow("AMAZON MKTPLACE PMTS AMZN COM BILL", 25, "AMAZON MKTPLACE PMTS AMZN")]
    [DataRow("ABCDEFGHIJ KLMNOP", 11, "ABCDEFGHIJ")]           // the space left at the cut is removed
    [DataRow("Short", 25, "Short")]
    [DataRow("  Café   au lait ", 10, "Cafe au la")]       // cleaned first, then cut
    public void Fit_CleansThenCuts(string input, int max, string expected)
    {
        Assert.AreEqual(expected, QifText.Fit(input, max));
    }

    [TestMethod]
    public void Fit_NeedsAPositiveLength()
    {
        Assert.ThrowsExactly<ArgumentOutOfRangeException>(() => QifText.Fit("x", 0));
    }
}
