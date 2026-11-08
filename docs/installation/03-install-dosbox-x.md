# Install DOSBox-X

## Purpose

SecretMoney uses DOSBox-X to provide the DOS environment required to install and run Managing Your Money 12 on modern Windows.

The original MYM software is supplied separately by the user and is not included in SecretMoney.

## Install DOSBox-X with WinGet

Search for the DOSBox-X package:

    winget search DOSBox-X

During SecretMoney testing, the package ID used was:

    joncampbell123.DOSBox-X

Install the package:

    winget install --id joncampbell123.DOSBox-X --exact --accept-package-agreements --accept-source-agreements

After installation, verify the installed package:

    winget list DOSBox-X

## Starting DOSBox-X

Start DOSBox-X from the Windows Start menu.

A normal DOSBox-X session begins at its internal Z: drive:

    Z:\>

The Z: drive is used by DOSBox-X itself.

SecretMoney mounts dedicated Windows directories as DOS C: and D: drives rather than exposing the entire Windows system drive.

## Runtime Directory Layout

The tested SecretMoney environment uses:

    C:\Users\<user>\OneDrive\MYM\DriveC
    C:\Users\<user>\OneDrive\MYM\DriveD

These directories remain outside the SecretMoney Git repository.

DriveC contains the MYM installation and active data.

DriveD is available for backups, imports, exports, QIF files, and other MYM-accessible working data.

## Mount the DOS C: Drive

At the DOSBox-X Z:\ prompt:

    MOUNT C "C:\Users\<user>\OneDrive\MYM\DriveC"

Then:

    C:

DOSBox-X will now see the contents of the Windows DriveC directory as DOS drive C:.

## Mount the DOS D: Drive

At the DOSBox-X prompt:

    MOUNT D "C:\Users\<user>\OneDrive\MYM\DriveD"

Then:

    D:

Files placed in the Windows DriveD directory become accessible to DOS applications through DOS drive D:.

## Directory Refresh

When a new file is added to a mounted Windows directory while DOSBox-X is already running, the DOS directory view may need to be refreshed.

If a newly created QIF or other file does not immediately appear in:

    DIR

refresh or remount the directory drive, or restart DOSBox-X.

After restarting, mount C: and D: again.

## Mounting Floppy Images

Managing Your Money 12 was installed from user-supplied floppy disk images.

The tested media consisted of:

    disk01.img
    disk02.img
    disk03.img

DOSBox-X can mount floppy images as DOS drive A:.

The images remain outside the SecretMoney repository.

## Multiple Installation Disks

MYM 12 installation spans three floppy images.

The original installer requests the next program disk when required.

During installation, switch drive A: to the requested image before continuing.

DOSBox-X provides floppy-image switching facilities for multi-disk installations.

## Verify Drive A:

After mounting Program Disk 1:

    A:
    DIR

The tested first disk displayed:

    INSTALL.EXE
    MYMCOMP1.DFL
    README.DOC

This confirms that DOSBox-X can read the installation media before INSTALL.EXE is started.

## Installing MYM

The next installation step is:

    A:
    INSTALL

The complete procedure is documented in:

    docs\installation\04-install-mym12.md

## Safety Boundary

Do not mount the entire Windows system drive as the DOS C: drive.

Use a dedicated directory such as:

    C:\Users\<user>\OneDrive\MYM\DriveC

This limits the DOS environment to the files intended for MYM.

## Git Boundary

The DOSBox-X runtime directories are not part of the SecretMoney source tree.

Do not commit:

    DriveC\
    DriveD\
    MYM12\
    *.DBS
    *.IMG
    original MYM executables or resources

SecretMoney contains only independently developed automation, documentation, test data, and interoperability research.

## AI-Assisted Documentation

This procedure was developed from the working SecretMoney installation with assistance from Microsoft Copilot.
