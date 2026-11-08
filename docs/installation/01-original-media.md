# Original Managing Your Money 12 Installation Media

## Purpose

This document records the original installation-media layout used while developing SecretMoney.

SecretMoney does not distribute Managing Your Money, its installation media, executables, documentation, artwork, or other original copyrighted files.

Users must supply their own copy of the original software.

## Media Used During Testing

The tested copy of Managing Your Money 12 for DOS was supplied as three raw 3.5-inch floppy disk images:

| Image | Size |
|---|---:|
| disk01.img | 737,280 bytes |
| disk02.img | 737,280 bytes |
| disk03.img | 737,280 bytes |

Each image is exactly 720 KiB.

Inspection of the first disk's boot sector showed a standard DOS FAT12 floppy layout.

Observed values included:

- 512 bytes per sector
- 2 sectors per cluster
- 2 FAT copies
- 112 root-directory entries
- 1,440 total sectors
- 9 sectors per track
- 2 heads
- FAT12 filesystem

The boot sector contained the OEM identifier:

    IBM  3.3

## Verifying an Image

In Windows PowerShell, the first 64 bytes can be inspected without executing anything:

    Get-Content .\disk01.img -Encoding Byte -TotalCount 64 | Format-Hex

The tested disk began with:

    EB 34 90 49 42 4D 20 20 33 2E 33

## Inspecting With 7-Zip

7-Zip 26.05 successfully recognized all three images as FAT12 filesystems.

If 7-Zip is installed in its normal location:

    & "$env:ProgramFiles\7-Zip\7z.exe" l .\disk01.img

## Observed Disk Contents

### Program Disk 1

The first disk contained:

    INSTALL.EXE
    MYMCOMP1.DFL
    README.DOC

Observed sizes:

| File | Size |
|---|---:|
| INSTALL.EXE | 65,863 bytes |
| MYMCOMP1.DFL | 652,288 bytes |
| README.DOC | 9,482 bytes |

### Program Disk 2

The second disk contained:

    MYMCOMP2.DFL

Observed size:

    729,088 bytes

### Program Disk 3

The third disk contained:

    MYMCOMP3.DFL

Observed size:

    392,960 bytes

## Installer Packaging

7-Zip could read the FAT12 floppy images but did not recognize MYMCOMP1.DFL as a supported archive format.

Binary inspection of MYMCOMP1.DFL revealed recognizable embedded filenames, including:

    MYM.EXE
    QL.EXE
    CFMYM.EXE
    HELP.HLP
    ASSET.DBS
    BUDGET.DBS
    IMPORT.DBS
    NETWORTH.DBS
    PORTFOL.DBS
    PRICES.DBS
    RECORDS.DBS
    REMIND.DBS
    REPORT.DBS
    ROI.DBS
    TAX.DBS
    WORK.DBS
    MYM12.PIF
    MYM12.ICO
    SAMPLE.DOC

INSTALL.EXE contained the string:

    PKLITE Copr. 1990-1995 PKWARE Inc. All Rights Reserved

The SecretMoney installation process therefore leaves decompression of the original MYMCOMP*.DFL files to the original installer running inside DOSBox-X rather than attempting to reproduce the installer format.

## Copyright Boundary

The following files are research inputs and must not be committed to the SecretMoney repository:

    disk01.img
    disk02.img
    disk03.img
    MYMCOMP1.DFL
    MYMCOMP2.DFL
    MYMCOMP3.DFL
    INSTALL.EXE
    MYM.EXE

The same rule applies to other original Managing Your Money program files and resources.

SecretMoney contains independently developed documentation, scripts, test fixtures, interoperability research, and conversion tools only.

## Preservation Recommendation

Keep the original disk images unchanged.

Before experimenting with the installation media, hashes may be recorded locally:

    Get-FileHash .\disk*.img -Algorithm SHA256

Hashes may be documented for verification purposes, but the corresponding copyrighted disk images should remain outside the SecretMoney repository.

## AI-Assisted Research

These findings were developed through hands-on testing with assistance from Microsoft Copilot.

Only behavior actually observed during the SecretMoney research process is recorded here as verified behavior.
