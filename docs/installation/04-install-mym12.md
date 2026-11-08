# Install Managing Your Money 12 in DOSBox-X

## Purpose

This document records the installation procedure successfully used during SecretMoney research to install Managing Your Money 12 for DOS from three user-supplied 720 KiB floppy disk images.

SecretMoney does not contain or distribute the original Managing Your Money software.

## Prerequisites

Before continuing:

- DOSBox-X is installed.
- The user has legally obtained MYM 12 installation media.
- The three tested floppy images are available locally.
- A dedicated writable DOS C: directory has been created.

The tested installation media consisted of:

    disk01.img
    disk02.img
    disk03.img

Each image was exactly:

    737,280 bytes

## Create the DOS C: Directory

The current SecretMoney runtime layout uses OneDrive:

    C:\Users\<user>\OneDrive\MYM\DriveC

Create it from Windows PowerShell if necessary:

    New-Item -ItemType Directory -Path "$env:USERPROFILE\OneDrive\MYM\DriveC" -Force | Out-Null

The directory must remain outside the SecretMoney Git repository.

## Start DOSBox-X

Start DOSBox-X.

A normal session begins at:

    Z:\>

## Mount the Writable C: Drive

At the DOSBox-X prompt:

    MOUNT C "C:\Users\<user>\OneDrive\MYM\DriveC"

The Windows directory is now visible to DOS applications as:

    C:\

## Mount the Installation Floppy Images

The tested MYM installer uses three program disks.

Mount the user-supplied disk images as floppy drive A:.

Example paths will vary depending on where the user stores the original installation media.

The images should be mounted in this order:

    disk01.img
    disk02.img
    disk03.img

Do not copy the original images into the SecretMoney repository.

## Verify Program Disk 1

At the DOSBox-X prompt:

    A:
    DIR

The tested Program Disk 1 contained:

    INSTALL.EXE
    MYMCOMP1.DFL
    README.DOC

Do not continue until Program Disk 1 is visible as drive A:.

## Start the Installer

From drive A:

    INSTALL

The original Managing Your Money installer will start.

Follow the installation prompts.

Install MYM onto the mounted DOS C: drive.

The tested installation created:

    C:\MYM12

On Windows this corresponds to:

    C:\Users\<user>\OneDrive\MYM\DriveC\MYM12

## Changing Program Disks

MYM 12 requests additional program disks during installation.

When the installer requests Program Disk 2, use DOSBox-X to switch floppy drive A: from:

    disk01.img

to:

    disk02.img

Continue the installer.

When the installer requests Program Disk 3, switch drive A: to:

    disk03.img

and continue.

During SecretMoney testing, DOSBox-X's floppy-drive swap functionality was used rather than copying the individual DFL files into a fabricated installation directory.

## Installation Completion

The tested installer completed successfully and displayed a message indicating that the DOS environment should be restarted before running Managing Your Money.

After installation completes, exit or restart DOSBox-X.

## Restart the MYM Environment

Start DOSBox-X again.

Mount the MYM C: drive:

    MOUNT C "C:\Users\<user>\OneDrive\MYM\DriveC"

Then:

    C:
    CD \MYM12

Verify the installed directory:

    DIR

Start Managing Your Money:

    MYM

## Installed Directory

During SecretMoney research, the installed MYM12 directory included program and support files such as:

    MYM.EXE
    CFMYM.EXE
    QL.EXE
    HELP.HLP
    IMPORT.DBS
    SETUP.DAT
    SETUP.DBS

It also contained application resource files such as:

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

These are original MYM files.

They must not be committed to SecretMoney.

## Data Directories

The tested installation created:

    C:\MYM12\CURRENT
    C:\MYM12\ORIGINAL

Both contained MYM database files.

Observed database filenames included:

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
    TAX.DBS
    TCCONN.DBS

These files are MYM runtime data and must remain outside the SecretMoney Git repository.

## Verify the Application

A successful installation should allow MYM 12 to start and display its normal application interface.

The SecretMoney research installation successfully accessed MYM functions including:

- account registers
- payments
- deposits
- categories
- split transactions
- native account-to-account transfers
- QIF import

These observations describe the tested environment only.

## Do Not Commit the Installed Application

Never add the installed MYM12 directory to Git.

In particular, do not commit:

    MYM.EXE
    *.CHP
    *.MYC
    *.HLP
    *.DBS
    *.PIF
    *.ICO

The SecretMoney repository documents how to build the environment from user-supplied installation media without redistributing the software.

## Preserve a Working Installation

Once MYM is running successfully, preserve the working runtime directory separately from the SecretMoney repository.

The current SecretMoney design uses OneDrive for the runtime environment so the MYM files can be synchronized independently of Git.

The next document describes that layout:

    docs\installation\05-onedrive-drives.md

## AI-Assisted Documentation

This installation procedure records a hands-on installation successfully completed during SecretMoney research with assistance from Microsoft Copilot.
