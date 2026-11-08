# Extracting the MYM 12 Floppy Images

## Purpose

This document records the verified procedure used to inspect and extract user-supplied Managing Your Money 12 floppy disk images on Windows.

Extraction is useful for research and verification. It is not required to redistribute any original MYM files.

Original Managing Your Money files must remain outside the SecretMoney repository.

## Prerequisites

The tested installation media consisted of:

    disk01.img
    disk02.img
    disk03.img

Each image was:

    737,280 bytes

The images were standard DOS FAT12 720 KiB floppy images.

## Install 7-Zip

Install 7-Zip with winget:

    winget install 7zip.7zip

If the `7z` command is not immediately available in the current PowerShell PATH, use the executable directly:

    & "$env:ProgramFiles\7-Zip\7z.exe"

To temporarily add the normal 7-Zip installation directory to the current PowerShell session:

    $env:Path += ";$env:ProgramFiles\7-Zip"

## Inspect an Image Without Extracting It

Change to the directory containing the user-supplied disk images.

List the contents of Disk 1:

    & "$env:ProgramFiles\7-Zip\7z.exe" l .\disk01.img

During SecretMoney testing, 7-Zip reported:

    Type = FAT
    Physical Size = 737280
    File System = FAT12
    Cluster Size = 1024
    Sector Size = 512

The tested Disk 1 contained:

    MYMCOMP1.DFL
    README.DOC
    INSTALL.EXE

Inspect the remaining images:

    & "$env:ProgramFiles\7-Zip\7z.exe" l .\disk02.img
    & "$env:ProgramFiles\7-Zip\7z.exe" l .\disk03.img

The tested Disk 2 contained:

    MYMCOMP2.DFL

The tested Disk 3 contained:

    MYMCOMP3.DFL

## Create a Local Extraction Directory

Create a local working directory outside the SecretMoney repository:

    New-Item -ItemType Directory -Path .\extracted -Force | Out-Null

The extracted original files are copyrighted application files and must not be copied into SecretMoney.

## Extract Disk 1

With 7-Zip 26.05, the output-directory switch was passed as one quoted argument:

    & "$env:ProgramFiles\7-Zip\7z.exe" x .\disk01.img "-oextracted\disk01" -y

A successful extraction reported:

    Everything is Ok

Disk 1 produced:

    INSTALL.EXE
    MYMCOMP1.DFL
    README.DOC

## Extract Disk 2

Run:

    & "$env:ProgramFiles\7-Zip\7z.exe" x .\disk02.img "-oextracted\disk02" -y

## Extract Disk 3

Run:

    & "$env:ProgramFiles\7-Zip\7z.exe" x .\disk03.img "-oextracted\disk03" -y

## Verify the Extraction

List all extracted directories:

    Get-ChildItem .\extracted -Recurse

The resulting working structure should resemble:

    extracted\
        disk01\
            INSTALL.EXE
            MYMCOMP1.DFL
            README.DOC

        disk02\
            MYMCOMP2.DFL

        disk03\
            MYMCOMP3.DFL

## Preserve File Hashes

Hashes can be recorded before performing additional research:

    Get-FileHash .\disk*.img -Algorithm SHA256

Hashes of extracted files can also be generated:

    Get-ChildItem .\extracted -File -Recurse | Get-FileHash -Algorithm SHA256

This provides a way to detect accidental modification of the local research copies.

## MYMCOMP DFL Files

7-Zip successfully extracted the DFL files from the FAT12 images but did not recognize `MYMCOMP1.DFL` itself as a supported archive.

The following command returned an archive-recognition error during testing:

    & "$env:ProgramFiles\7-Zip\7z.exe" l .\extracted\disk01\MYMCOMP1.DFL

SecretMoney therefore does not depend on 7-Zip to unpack the MYMCOMP DFL containers.

The original MYM installer is used for that operation inside DOSBox-X.

## Inspecting Binary Headers

Binary data can be inspected without executing a file.

For example:

    Get-Content .\extracted\disk01\MYMCOMP1.DFL -Encoding Byte -TotalCount 128 | Format-Hex

During testing, readable filenames were visible within the DFL data, including `MYM.EXE`.

This observation is documented for research purposes only.

## Do Not Run INSTALL.EXE Directly From Windows

SecretMoney's tested installation procedure runs the original DOS installer inside DOSBox-X.

Do not depend on modern Windows to execute the original DOS installer directly.

The next installation document describes setting up DOSBox-X.

## Repository Boundary

Do not commit the extraction directory.

Do not commit:

    *.img
    *.dfl
    INSTALL.EXE
    MYM.EXE
    original MYM resources

A recommended SecretMoney `.gitignore` should exclude original installation media and runtime MYM files.

The extraction directory exists only as a local research and installation workspace.

## AI-Assisted Research

This procedure was documented from hands-on installation testing performed with assistance from Microsoft Copilot.
