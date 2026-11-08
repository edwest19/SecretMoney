# Managing Your Money 12 File Layout Research

## Purpose

This document records the installed file layout observed during SecretMoney research of Managing Your Money 12 for DOS.

The purpose of this research is interoperability, preservation, data migration, and understanding user-owned financial data.

SecretMoney does not distribute the original application files described here.

## Installed Root Directory

The tested installation created:

    C:\MYM12

In the current DOSBox-X/OneDrive configuration, the corresponding Windows directory is:

    C:\Users\<user>\OneDrive\MYM\DriveC\MYM12

## Observed Root Files

The tested installation contained the following executable files:

    MYM.EXE
    CFMYM.EXE
    QL.EXE

Observed help and configuration-related files included:

    HELP.HLP
    MYMIC.HLP
    SETUP.DAT
    SETUP.DBS
    PRINTER.STP
    CUSTOM-1.STP
    CUSTOM-2.STP
    WP.KEY

Windows-related launch resources included:

    MYM12.ICO
    MYM12.PIF

## CHP Resources

The installed root contained:

    0.CHP
    1.CHP
    2.CHP
    3.CHP
    4.CHP
    5.CHP
    6.CHP
    7.CHP
    8.CHP
    9.CHP

These files were present during testing, but their internal format and exact purpose have not been established by SecretMoney research.

Do not describe their internal role as verified without additional testing.

## MYC Resources

The installation also contained:

    0.MYC
    5.MYC
    7.MYC

Their exact purpose has not yet been established.

## Import-Related Resources

Observed files included:

    IMPORT.DBS
    MYMIC.DBS
    MYMICBMT.DBS
    MYMICPMT.DBS

The installed application provides an Import Data function.

SecretMoney has experimentally tested QIF importing, but the specific role of each MYMIC database file has not yet been established.

## Data Directories

Two important directories were created by the installation:

    CURRENT
    ORIGINAL

The tested directory structure resembles:

    MYM12\
        CURRENT\
        ORIGINAL\
        MYM.EXE
        ...

Both CURRENT and ORIGINAL contained similarly named DBS files.

## Observed CURRENT Files

The tested CURRENT directory contained:

    ASSET.DBS
    BUDGET.DBS
    COMPOUND.DBS
    INSURE.DBS
    LITTLE.DBS
    NETWORTH.DBS
    PORTFOL.DBS
    PRICES.DBS
    RECORDS.DBS
    REMIND.DBS
    REPORT.DBS
    ROI.DBS
    SAMPLE.DOC
    TAX.DBS
    TCCONN.DBS

## Observed ORIGINAL Files

The tested ORIGINAL directory contained the same set of database/template filenames:

    ASSET.DBS
    BUDGET.DBS
    COMPOUND.DBS
    INSURE.DBS
    LITTLE.DBS
    NETWORTH.DBS
    PORTFOL.DBS
    PRICES.DBS
    RECORDS.DBS
    REMIND.DBS
    REPORT.DBS
    ROI.DBS
    SAMPLE.DOC
    TAX.DBS
    TCCONN.DBS

## CURRENT vs ORIGINAL

Controlled testing showed that MYM modifies files under CURRENT during normal application activity.

A comparison after test activity identified content changes in:

    BUDGET.DBS
    INSURE.DBS
    LITTLE.DBS
    RECORDS.DBS
    REMIND.DBS

Other tested files remained identical to their ORIGINAL counterparts at that point in the research.

This does not establish that the unchanged databases are never writable. It records only what occurred during the controlled tests performed.

## Observed File Sizes

The initial tested data set included these file sizes:

| File | Observed size |
|---|---:|
| ASSET.DBS | 98,304 bytes |
| BUDGET.DBS | 69,632 bytes |
| COMPOUND.DBS | 12,288 bytes |
| INSURE.DBS | 16,384 bytes |
| LITTLE.DBS | 8,192 bytes initially |
| NETWORTH.DBS | 12,288 bytes |
| PORTFOL.DBS | 8,192 bytes |
| PRICES.DBS | 8,192 bytes |
| RECORDS.DBS | 8,192 bytes |
| REMIND.DBS | 8,192 bytes |
| REPORT.DBS | 57,344 bytes |
| ROI.DBS | 8,192 bytes |
| TAX.DBS | 16,384 bytes |
| TCCONN.DBS | 28,672 bytes |

During subsequent testing, CURRENT\LITTLE.DBS grew from:

    8,192 bytes

to:

    24,576 bytes

This growth occurred while MYM was being exercised with test data.

## Database Header Observation

ASSET.DBS began with bytes observed as:

    00 00 0F FF 00 02 00 17 00 18 00 00 05 54 00 00

LITTLE.DBS began with:

    00 00 0F FF 00 02 00 05 00 06 00 00 00 C6 00 01

These headers do not match the standard dBASE header expected by the initial research hypothesis.

SecretMoney therefore treats the DBS files as an application-specific database format unless future research demonstrates otherwise.

## 4 KiB Alignment Observation

Many observed DBS file sizes are multiples of:

    4096 bytes

Controlled tests also showed changes clustered within 4096-byte regions.

SecretMoney currently records this as evidence consistent with a page-oriented format.

The exact database-page structure has not been completely decoded.

## BUDGET.DBS

The strongest database findings currently concern:

    CURRENT\BUDGET.DBS

Controlled test transactions demonstrated that BUDGET.DBS stores banking transaction information.

Observed plain-text data included:

    TESTPAYEE123
    TESTMEMO456
    AMOUNTTEST999
    AMOUNTMEMO999

Additional BUDGET.DBS findings are documented in:

    docs\research\budget-dbs-findings.md

## Repository Boundary

None of the original installed files described in this document belong in the SecretMoney Git repository.

In particular, do not commit:

    MYM.EXE
    *.CHP
    *.MYC
    *.HLP
    *.DBS
    MYM12\

Real MYM database files can contain private financial information in addition to being original/runtime application data.

## Research Method

SecretMoney uses controlled differential testing:

1. Preserve a baseline.
2. Perform one identifiable operation in MYM.
3. Save and close MYM.
4. Compare the resulting files.
5. Locate deliberately distinctive test values.
6. Record verified behavior.
7. Keep hypotheses clearly separated from findings.

This approach allows the file format to be studied without assuming that filenames or binary structures have a particular meaning.

## AI-Assisted Research

These findings were developed through hands-on controlled testing with assistance from Microsoft Copilot.

Only directly observed behavior is documented as verified.
