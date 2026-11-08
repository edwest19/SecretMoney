# OneDrive Runtime Drives

## Purpose

SecretMoney keeps the live Managing Your Money environment separate from the Git repository.

The tested runtime layout stores the MYM working directories under OneDrive and exposes them to DOSBox-X as separate DOS C: and D: drives.

The conceptual layout is:

    OneDrive\MYM\
        DriveC\
        DriveD\

DOSBox-X maps these as:

    C: -> DriveC
    D: -> DriveD

## Keep Runtime Data Outside SecretMoney

The SecretMoney Git repository and MYM runtime files should remain separate.

Example:

    C:\Users\<user>\OneDrive\SecretMoney

and:

    C:\Users\<user>\OneDrive\MYM

The SecretMoney directory contains independently developed source code, scripts, documentation, and tests.

The MYM directory contains the actual DOS runtime environment.

Do not place the live MYM installation inside the SecretMoney Git repository.

## Create DriveC

From Windows PowerShell:

    New-Item -ItemType Directory -Path "$env:USERPROFILE\OneDrive\MYM\DriveC" -Force | Out-Null

DriveC contains the installed MYM environment.

The tested installation placed MYM at:

    DriveC\MYM12

DOSBox-X sees the same location as:

    C:\MYM12

## Create DriveD

From Windows PowerShell:

    New-Item -ItemType Directory -Path "$env:USERPROFILE\OneDrive\MYM\DriveD" -Force | Out-Null

DriveD provides a second DOS-accessible working location.

SecretMoney uses DriveD for purposes such as:

- QIF import files
- exports
- test interchange files
- MYM-accessible backup files

## Mount DriveC in DOSBox-X

At the DOSBox-X Z:\ prompt:

    MOUNT C "C:\Users\<user>\OneDrive\MYM\DriveC"

Verify:

    C:
    DIR

## Mount DriveD in DOSBox-X

At the DOSBox-X prompt:

    MOUNT D "C:\Users\<user>\OneDrive\MYM\DriveD"

Verify:

    D:
    DIR

## Start MYM

Once DriveC is mounted:

    C:
    CD \MYM12
    MYM

## Access QIF Files

A QIF file placed in the Windows directory:

    C:\Users\<user>\OneDrive\MYM\DriveD

is intended to be accessible to MYM through DOS drive D:.

For example, a Windows file:

    C:\Users\<user>\OneDrive\MYM\DriveD\QIFTEST.QIF

appears to DOS applications as:

    D:\QIFTEST.QIF

During testing, short DOS-friendly filenames were convenient.

Examples:

    QIFTEST.QIF
    CHECKTEST.QIF
    CLEARED.QIF
    SPLIT.QIF
    TRANSFER.QIF

## Refreshing DriveD

If a file is added to the Windows DriveD directory while DOSBox-X is already running, it may not immediately appear in the DOS directory listing.

Check with:

    D:
    DIR *.QIF

If the new file is not visible, refresh or remount the DOSBox-X directory drive, or restart DOSBox-X and mount DriveD again.

## OneDrive and Git Serve Different Purposes

The MYM runtime environment and the SecretMoney repository solve different problems.

The runtime environment contains files MYM actually reads and modifies.

The SecretMoney repository contains the tooling and knowledge needed to reproduce and interact with that environment.

Keep these boundaries explicit:

    OneDrive\MYM
        Runtime MYM environment

    OneDrive\SecretMoney
        Git repository

## Live Financial Data

MYM stores active data in its runtime database files.

During testing, MYM used files including:

    BUDGET.DBS
    LITTLE.DBS
    RECORDS.DBS
    REMIND.DBS
    INSURE.DBS

These files must not be committed to SecretMoney.

They may contain private financial information.

## Backups

DriveD can be used as a DOS-visible backup or interchange location.

However, DriveC and DriveD are both part of the same runtime storage hierarchy.

For important financial records, SecretMoney recommends maintaining an additional independent backup appropriate to the user's own backup strategy.

## OneDrive Synchronization

Because the runtime directories are stored under OneDrive, allow synchronization to complete before depending on a newly changed file from another device or location.

Avoid intentionally modifying the same MYM database from multiple systems simultaneously.

MYM is legacy DOS software and SecretMoney does not assume its database files provide modern multi-user or synchronization conflict handling.

## DOS-Friendly Filenames

For files intended to be opened directly by MYM or other DOS programs, prefer simple filenames.

For example:

    BANK.QIF
    CHASE.QIF
    SAVINGS.QIF
    IMPORT.QIF

This avoids unnecessary dependence on long-filename handling in legacy software.

## Planned Automation

SecretMoney may provide scripts that create the runtime directories and simplify the DOSBox-X mount process.

Planned script locations include:

    scripts\powershell\
    scripts\dosbox\

Automation should make the paths configurable rather than permanently assuming one Windows username.

## Repository Boundary

Do not commit:

    DriveC\
    DriveD\
    MYM12\
    *.DBS
    original MYM program files
    real financial QIF files

Synthetic QIF fixtures created specifically for SecretMoney testing may be stored under:

    tests\qif\

## AI-Assisted Documentation

This runtime layout was developed and tested during the SecretMoney project with assistance from Microsoft Copilot.
