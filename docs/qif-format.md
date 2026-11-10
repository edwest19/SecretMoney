# SecretMoney QIF format, version 1

> Written by Claude (Anthropic) with Edwin West, 2026-11-10, from the code in `SecretMoney.Core/Qif` and the MYM 12 research in [research/qif-compatibility.md](research/qif-compatibility.md). SecretMoney is a project built with AI-generated code; see [README.md](../README.md).

This is exactly what SecretMoney writes into a QIF file for Managing Your Money 12 for DOS. It uses only what MYM 12 was seen to import (the "Evidence" column), and nothing it wasn't. `QifWriter` writes it; the known-answer tests in `SecretMoney.Core.Tests/QifWriterTests.cs` lock it. **Changing it needs the owner's approval, a new version of this document, and a new import test in MYM.** Never change a known-answer test's expected values to make it pass.

One file holds the entries of one MYM account. Which entries, and what goes in each field, is decided by the export rules, not by this format.

## Bytes

- **ASCII only:** every byte is a printable ASCII character, 0x20 to 0x7E, or part of a line ending. No byte-order mark.
- **Line endings:** CR LF (0x0D 0x0A) after every line, including the last.
- **Evidence:** every QIF file imported in rounds 1 and 2 was ASCII with CR LF (`scripts/powershell/New-Qif.ps1`, `tests/qif`).

## Header

The first line, once per file:

| Account in MYM | Header | Evidence |
|---|---|---|
| Bank | `!Type:Bank` | rounds 1 and 2 |
| Charge (credit card) | `!Type:CCard` | round 2, `t1ccard.qif` |

## Record

Each entry is these lines, in this order, then `^` on a line of its own:

| Line | Holds | Rules | Evidence |
|---|---|---|---|
| `D` | Date | `MM/dd/yyyy`, two-digit month and day, four-digit year | rounds 1 and 2 |
| `T` | Amount | The account's own amount. Digits, a `.` and exactly two decimals; `-` in front when negative; no `+`, no thousands separator. Not zero. Negative is a payment (Bank) or a charge (Charge); positive is a deposit or a payment to the card | rounds 1 and 2; `T1500.00` and `T-31.31` in round 2 |
| `P` | Payee | Required. Text (below), 1 to 25 characters | round 2: MYM keeps 25 |
| `M` | Memo | Optional; no line when there is none. Text, 1 to 40 characters | round 2: MYM keeps 40; `t3three.qif` imported without memos |
| `L` | Category | Required. Text, 1 to 20 characters, no `:`, not starting with `[` | round 2: at least 20 kept; a colon is kept as part of the name; `L[Savings]` did not make a transfer in round 1 |
| `^` | End of entry | Exactly `^` | rounds 1 and 2 |

Entries follow each other with nothing between them. The writer keeps the order it is given.

## Text

Payee, memo and category hold **clean text**: printable ASCII on one line, with single spaces and no space at either end. `QifText.Clean` turns any text into clean text, in this order:

1. Unicode compatibility decomposition (NFKD): `é` becomes `e` plus an accent, `…` becomes `...`, a non-breaking space becomes a space.
2. Accents and other combining marks are dropped.
3. Curly quotes become `'` or `"`; hyphens and dashes (U+2010 to U+2015, U+2212) become `-`.
4. Control characters, including tabs and line breaks, become a space.
5. Any other character outside printable ASCII becomes `?`, one per character.
6. Runs of spaces become one space; spaces at either end are removed.

`QifText.Fit(text, n)` cleans the text, cuts it to at most `n` characters, and removes a space left at the end of the cut.

## Never written

These were not shown to work in MYM 12, or were shown not to, so version 1 never writes them:

- `L[Account]` transfers (round 1: did not make MYM's linked transfer) and `!Option:AllXfr`
- splits (`S`, `E`, `$`): imported in round 1, but the sign of each split amount is not settled
- check numbers (`N`): imported in round 1, but SecretBeans journals don't carry them
- cleared status (`C`): round 1 showed no checkmark
- `!Account` blocks, several accounts in one file, investment types (`!Type:Invst`)
- an entry with no category, or a file with no entries

`QifWriter` refuses an entry that breaks any rule above (`QifFormatException`, naming the entry and the rule). It never changes an entry to make it fit; fitting is the export rules' job.

## Example

Two entries for a bank account. `QifWriterTests` checks that the writer produces exactly these lines, each followed by CR LF:

```text
!Type:Bank
D11/03/2026
T-52.40
PEXAMPLE GROCERY
Lgroceries
^
D11/15/2026
T1500.00
PEXAMPLE PAYROLL
MPay for 1-15 November
Lsalary
^
```

`QifWriterTests` also checks that the writer reproduces, byte for byte, the round-2 files MYM 12 imported: `t1ccard.qif`, `t1bbank.qif`, `t3three.qif`, `t4chk.qif` and `t4sav.qif`. `t2long.qif` is refused, because its 80-character payee is longer than MYM keeps.
