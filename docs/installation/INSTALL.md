# SecretMoney Installation Guide

SecretMoney documents a reproducible environment for installing and running Managing Your Money 12 for DOS on modern Windows.

## Important

SecretMoney does not distribute Managing Your Money.

You must provide your own installation media. Do not commit original disk images, executables, application resources, manuals, financial databases, or other copyrighted Managing Your Money files to this repository.

## Tested Environment

This installation procedure has been tested with:

- Windows
- PowerShell
- DOSBox-X
- Managing Your Money 12 for DOS
- Three 720 KB floppy disk images
- OneDrive for the MYM working directories

## 1. Verify the Installation Media

The tested MYM 12 installation consists of three 720 KB floppy disk images.

Expected image size:

    737280 bytes

The disk images use a DOS FAT12 filesystem.

SecretMoney does not provide these images.

## 2. Install 7-Zip

7-Zip is useful for inspecting the floppy images without executing their contents.

Install with winget:

    winget install 7zip.7zip

If 7z.exe is not immediately available in PATH, it can normally be invoked with:

    & "$env:ProgramFiles\7-Zip\7z.exe"

To inspect an image:

    & "$env:ProgramFiles\7-Zip\7z.exe" l .\disk01.img

The tested MYM 12 media contained:

Disk 1:

    INSTALL.EXE
    MYMCOMP1.DFL
    README.DOC

Disk 2:

    MYMCOMP2.DFL

Disk 3:

    MYMCOMP3.DFL

These original files must not be added to SecretMoney.

## 3. Install DOSBox-X

Find the available package:

    winget search DOSBox-X

Install DOSBox-X:

    winget install --id joncampbell123.DOSBox-X --exact

Verify:

    winget list DOSBox-X

## 4. Create the MYM Working Directories

The tested OneDrive layout is:

    C:\Users\<user>\OneDrive\MYM\DriveC
    C:\Users\<user>\OneDrive\MYM\DriveD

DriveC is used as the DOS C: drive.

DriveD is used for backups, imports, exports, and interchange files.

These directories are runtime data and should remain outside the SecretMoney Git repository.

## 5. Mount Drive C

At the DOSBox-X Z:\ prompt:

    MOUNT C "C:\Users\<user>\OneDrive\MYM\DriveC"

## 6. Mount the Installation Images

Mount the user-supplied floppy images as DOS drive A:.

The original MYM installation requires multiple program disks.

Use DOSBox-X disk-image mounting and switch to the next floppy image when the MYM installer requests the next program disk.

## 7. Run the MYM Installer

From DOSBox-X:

    A:
    INSTALL

Install Managing Your Money onto DOS drive C:.

The tested installation created:

    C:\MYM12

Inside Windows, that corresponds to:

    C:\Users\<user>\OneDrive\MYM\DriveC\MYM12

Follow the original installer prompts and change floppy images whenever requested.

## 8. Restart DOSBox-X

After installation, restart DOSBox-X if requested by the MYM installer.

Mount the working directories again:

    MOUNT C "C:\Users\<user>\OneDrive\MYM\DriveC"
    MOUNT D "C:\Users\<user>\OneDrive\MYM\DriveD"

Then:

    C:
    CD \MYM12
    MYM

## 9. Verify MYM

A successful installation should start Managing Your Money 12 and allow access to its normal menus and account functions.

Do not use real financial data until the installation is working reliably and the MYM data directory is backed up.

## 10. QIF Interchange

SecretMoney research has successfully tested MYM 12 QIF imports for:

- dates
- payments
- deposits
- payees
- memos
- categories
- check numbers
- split transactions

Other QIF behavior remains subject to testing.

See the SecretMoney QIF compatibility documentation for current findings.

## Runtime Data

Never commit the live MYM runtime directory to Git.

In particular, keep these outside the repository:

    DriveC
    DriveD
    MYM12
    *.DBS
    *.IMG
    *.EXE

The SecretMoney repository contains only independently created installation scripts, documentation, tests, interoperability research, and related tooling.

## AI-Assisted Documentation

SecretMoney is intentionally developed with assistance from Microsoft Copilot and GitHub Copilot.

This installation procedure was developed and verified through hands-on testing of the user's own installation environment.
