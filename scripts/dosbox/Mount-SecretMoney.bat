@ECHO OFF
REM SecretMoney DOSBox-X mount script
REM Developed with Microsoft Copilot assistance.
REM
REM This script only mounts existing Windows directories.
REM It does not download, copy, install, or modify Managing Your Money.

MOUNT C "C:\Users\edwes\OneDrive\MYM\DriveC"
MOUNT D "C:\Users\edwes\OneDrive\MYM\DriveD"

C:
CD \MYM12

ECHO.
ECHO SecretMoney drives mounted.
ECHO C: = OneDrive\MYM\DriveC
ECHO D: = OneDrive\MYM\DriveD
ECHO.
ECHO To start Managing Your Money, type:
ECHO MYM
ECHO.
