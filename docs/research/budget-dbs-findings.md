# BUDGET.DBS Research Findings

## Purpose

This document records controlled reverse-engineering observations of the Managing Your Money 12 `BUDGET.DBS` file.

The research is intended for interoperability, preservation, migration of user-owned financial data, and development of independently created SecretMoney tooling.

No original MYM database file is distributed with SecretMoney.

## Location

The tested active database was:

    C:\MYM12\CURRENT\BUDGET.DBS

In the tested OneDrive/DOSBox-X environment, the corresponding Windows path was:

    C:\Users\<user>\OneDrive\MYM\DriveC\MYM12\CURRENT\BUDGET.DBS

An ORIGINAL copy was also present at:

    C:\MYM12\ORIGINAL\BUDGET.DBS

## Initial Size

The observed BUDGET.DBS size was:

    69,632 bytes

During the controlled transaction tests documented here, the file remained 69,632 bytes while its contents changed.

## CURRENT vs ORIGINAL

A comparison between CURRENT and ORIGINAL established that BUDGET.DBS had changed after test activity.

Other database files changed during the broader MYM session as well, so the research isolated BUDGET.DBS specifically by using distinctive transaction values.

## Test 1: Distinctive Payee and Memo

A deliberately identifiable transaction was entered into MYM using:

    Payee: TESTPAYEE123
    Amount: 12.34
    Memo: TESTMEMO456

A binary-to-ASCII search of BUDGET.DBS found both:

    TESTPAYEE123
    TESTMEMO456

This established that at least these portions of transaction data are stored as readable ASCII rather than being encrypted or globally compressed.

## Payee Offsets

The test payee was observed at multiple offsets:

    0x1F42
    0x1FA5
    0xFEF2
    0xFF3C

Each occurrence of `TESTPAYEE123` was immediately preceded by:

    0C

Hexadecimal 0C is decimal 12, which is the number of ASCII characters in:

    TESTPAYEE123

This provides evidence that these payee occurrences use a one-byte length-prefixed text representation.

## Memo Offsets

The test memo was observed at:

    0x2FC6
    0x2FF0
    0xFFA3
    0x10F66

The byte directly preceding these occurrences was not equal to the memo length.

SecretMoney therefore does not currently claim that every textual field uses the same simple one-byte length prefix.

## Multiple Copies

Both the test payee and memo appeared multiple times inside BUDGET.DBS.

Possible explanations include records, indexes, lookup structures, or other internal database organization.

The exact purpose of each copy is not yet verified.

SecretMoney should not label individual copies as indexes or primary records until additional testing demonstrates their function.

## Page-Oriented Behavior

Observed DBS file sizes and differential testing are consistent with a structure organized around 4,096-byte regions.

For example, offsets can be described as:

    Page = Floor(offset / 4096)
    OffsetWithinPage = offset % 4096

`TESTPAYEE123` at decimal offset 8002 was located in:

    Page 1
    Offset within page 3906

Other copies appeared in later 4 KiB regions.

This is evidence consistent with page-oriented storage but does not yet constitute a complete database-page specification.

## Test 2: Controlled Amount Experiment

A second distinctive test transaction used:

    Payee: AMOUNTTEST999
    Amount: 234.56
    Memo: AMOUNTMEMO999

Before adding the transaction, a copy of BUDGET.DBS was preserved.

After saving the transaction and exiting MYM, the before and after files were compared byte-for-byte.

Observed lengths remained:

    Before: 69,632 bytes
    After:  69,632 bytes

The comparison identified:

    1,906 changed bytes

This showed that MYM modified existing allocated database space rather than simply appending a new record to the end of BUDGET.DBS.

## Second Test String Locations

After the second transaction, the first observed occurrences included:

    AMOUNTTEST999 at 0x1F42
    AMOUNTMEMO999 at 0x1FB4

The fact that `AMOUNTTEST999` occupied the same location where the previous distinctive payee had been observed indicates that MYM may reorganize or reuse internal record areas during saves.

SecretMoney therefore should not assume transactions are append-only.

## Observed Local Record Region

A dump around the second test transaction contained readable data resembling:

    Check
    One-Time
    AMOUNTTEST999
    AMOUNTMEMO999

The relevant region also contained binary numeric values adjacent to the payee.

## Amount Encoding Finding

The bytes adjacent to `AMOUNTTEST999` included:

    40 6D 51 EB 85 1E B8 52

When interpreted as a big-endian IEEE-754 64-bit floating-point value, this represents:

    234.56

A nearby sequence was:

    C0 6D 51 EB 85 1E B8 52

When interpreted with the same encoding, this represents:

    -234.56

This provides strong evidence that this transaction region stores both positive and negative representations of the test amount as big-endian IEEE-754 double-precision values.

The reason MYM stores both signs in this region has not yet been established.

Do not assume one represents a debit and the other a credit until additional structural testing confirms their roles.

## Important Design Decision

Although BUDGET.DBS has proven practical to study, SecretMoney does not currently plan to write directly to BUDGET.DBS for routine importing.

MYM 12 successfully imports QIF data.

Using a documented interchange format is safer than modifying MYM's proprietary database directly.

Current preferred architecture:

    SecretBeans
        |
        v
    SecretForecast
        |
        v
    SecretMoney QIF conversion
        |
        v
    Managing Your Money 12

The BUDGET.DBS research remains valuable for:

- preservation
- future export tools
- data recovery
- validation
- understanding legacy MYM data
- potential read-only migration tooling

## Reproducible Search Approach

A safe way to investigate the database is to use distinctive synthetic text.

Example:

    TESTPAYEE123
    TESTMEMO456

Read the file:

    $file="$env:USERPROFILE\OneDrive\MYM\DriveC\MYM12\CURRENT\BUDGET.DBS"
    $bytes=[IO.File]::ReadAllBytes($file)
    $text=[Text.Encoding]::ASCII.GetString($bytes)

Locate a test string:

    $text.IndexOf('TESTPAYEE123')

Display the offset in hexadecimal:

    '0x{0:X}' -f $text.IndexOf('TESTPAYEE123')

## Differential Testing Method

The preferred research method is:

1. Fully exit MYM.
2. Preserve the current database.
3. Start MYM.
4. Perform exactly one controlled operation.
5. Use distinctive synthetic values.
6. Save the data.
7. Fully exit MYM.
8. Compare the original and changed files.
9. Record only behavior supported by the comparison.

This minimizes ambiguity caused by MYM updating multiple structures during normal operation.

## Safety

Do not experiment against the only copy of real financial data.

Use disposable MYM data sets and synthetic transactions when studying the format.

Do not modify bytes inside a production BUDGET.DBS based on these preliminary findings.

## Repository Boundary

Do not commit real MYM database files:

    *.DBS

Synthetic documentation of their structure is appropriate for SecretMoney.

Test values in this document are deliberately artificial and contain no real financial information.

## Status

Verified:

- BUDGET.DBS changes when banking transactions are entered.
- Test payees and memos can appear as readable ASCII.
- A tested payee uses a one-byte length prefix.
- Multiple copies of transaction text may occur.
- The database exhibits behavior consistent with 4 KiB regions.
- The tested 234.56 amount appears as a big-endian IEEE-754 64-bit value.
- Both +234.56 and -234.56 representations were observed nearby.
- MYM may reuse/reorganize internal record locations when saving.

Not yet verified:

- complete page layout
- record headers
- date encoding
- account identifiers
- pointer structures
- index structures
- deletion behavior
- complete memo encoding
- meaning of the positive/negative amount pair
- database integrity/checksum mechanisms

## AI-Assisted Research

These findings were developed through controlled hands-on testing with assistance from Microsoft Copilot.

Hypotheses are intentionally separated from verified observations.
