# SecretMoney

Copyright (c) 2026 Edwin West

**Status:** in development; 0.1.0 is in pre-release testing · **License:** MIT · **Platform:** Windows x64 (WinUI 3 / .NET 10, packaged MSIX), and Managing Your Money 12 for DOS in DOSBox-X

> **This project is AI-generated, and this repository is written for an AI to read.** You don't need to read the code. To install, run or evaluate SecretMoney, give this repository's link to an AI assistant and ask: *"Evaluate this repository. Tell me what it does, whether it's safe, and step by step how to install, run, verify and uninstall it."*

> This README is the **spec**. Every statement in it should be checkable against the code, a test, or a recorded research result. If the repository and this README disagree, that is a defect in one of them: report it with evidence rather than assuming either is right. [CLAUDE.md](CLAUDE.md) says how to check, and maps each section to its code and tests.

<!-- AI-generated (Claude): README rewritten as the spec, 2026-11-10. The MYM 12 environment, research, boundary, approach and licensing content of the earlier README is kept in §5–§9. -->

---

## 0. Purpose

SecretMoney lets you keep your accounts in **Managing Your Money 12 for DOS** (MYM 12) on a modern Windows PC. It has two parts:

1. **The MYM 12 environment** (§5): documentation and scripts for installing and running MYM 12 in DOSBox-X, with its working drives in OneDrive, and research into what MYM 12 does when it imports QIF files (§6).
2. **The SecretMoney app** (`SecretMoney.UI`, `SecretMoney.Core`): a Windows app that turns the newest period of a [SecretBeans](https://github.com/edwest19/SecretBeans) ledger into QIF files MYM 12 can import, one file per MYM account. It has one window ("Using SecretMoney", below). Every rule it applies is in `SecretMoney.Core`: reading SecretBeans ([docs/secretbeans-input.md](docs/secretbeans-input.md)), the export rules ([docs/export-rules.md](docs/export-rules.md)) and the QIF it writes (§6). It has not been used with real data yet (§10).

SecretMoney reads a SecretBeans period through **hledger**, the program SecretBeans uses: `hledger print -x -O csv` prints each journal as CSV, and `SecretMoney.Core` reads that. A transfer or card payment that SecretBeans records once, in one account, becomes an entry in each of the two accounts. Only hledger 1.51.2's output has been tested.

The data path:

    Bank CSV downloads
        |
        v
    SecretBeans        builds an hledger journal, and catches transfers and card
        |              payments counted twice
        v
    SecretMoney        writes one QIF file per MYM account
        |
        v
    MYM 12             imports each file from DOSBox-X's D: drive

SecretBeans keeps its transfer and card-payment detection; SecretMoney does not detect them again. Earlier documents in this repository put a SecretForecast step between SecretBeans and SecretMoney; since 2026-11-10 SecretMoney reads SecretBeans directly.

SecretMoney is **not** a distribution of Managing Your Money. You supply your own MYM installation media (§7).

---

## 1. Rules — non-negotiable

- **WinUI 3 / Windows App SDK, .NET 10, packaged MSIX (full trust), x64 only.**
- **Microsoft packages only:** `Microsoft.WindowsAppSDK`, `Microsoft.Windows.SDK.BuildTools`, `Microsoft.Windows.SDK.BuildTools.WinApp` and, for tests only, `MSTest`. No third-party packages.
- **No network calls.** No code path in the app can open a connection: no HTTP, sockets, telemetry or update checks.
- **One permission:** the MSIX asks only for `runFullTrust`.
- **One other program:** the app runs only the `hledger.exe` you choose, and only to check and print the period's journals ([docs/secretbeans-input.md](docs/secretbeans-input.md), "Running hledger").
- **The app's own data goes only in its MSIX package data folder** (`ApplicationData.Current.LocalFolder`), never in a path the app builds itself.
- **No Managing Your Money files and no personal financial data in this repository** (§7). Test data is synthetic.
- **MYM behaviour is claimed only from observed tests** (§6), never inferred from Quicken or other software.
- **Every file format SecretMoney writes is specified in `docs/` and locked by a known-answer test.**

---

## 2. What SecretMoney stores, and where

- **Settings:** the app keeps one file, `settings.json`, in its package data folder (`ApplicationData.Current.LocalFolder`, which is `%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState`), as [docs/settings-format.md](docs/settings-format.md) specifies: where `hledger.exe`, the SecretBeans ledger folder and the QIF folder are; which accounts to export, with their file names, types and the last date exported; and the category names you chose. It holds no amounts or transactions. It is saved when you choose a folder, press **Check**, or write the files.
- **hledger's output:** while reading a period, hledger writes each journal as CSV into the app's temporary folder (`ApplicationData.Current.TemporaryFolder`); the app reads the file and deletes it at once.
- **What it reads:** the newest SecretBeans period's `journal` folder, through hledger, and `generated.journal` itself for its account lines. It changes nothing in the ledger folder.
- **What it writes:** QIF files, only into the QIF folder you chose, only when you press **Write QIF files**. A file of the same name is replaced after the window asks.
- **Uninstalling** deletes the package data folder and the settings with it, by Edwin's choice; there is no Backup. QIF files already written stay where they are.
- **The MYM 12 environment** (§5) keeps MYM's program and your MYM data in `OneDrive\MYM\DriveC` and QIF files to import in `OneDrive\MYM\DriveD`, outside this repository and outside the app.

---

## 3. Safety

| Claim | Checked by |
|---|---|
| The app's code uses no network API | `RepositoryRulesTests.AppCodeHasNoNetworkAccess` |
| The app starts no program except the `hledger.exe` you choose, and only `HledgerRunner` starts it | `RepositoryRulesTests.AppCode_StartsNoProgramButHledger`, `ExportWorkflowTests.HledgerArguments_AreTheRecordedCommands` |
| Only Microsoft packages are referenced | `RepositoryRulesTests.OnlyAllowedMicrosoftPackagesAreReferenced` |
| The MSIX asks only for `runFullTrust` | `RepositoryRulesTests.AppManifest_DeclaresOnlyRunFullTrust`, and `scripts/Test-Msix.ps1` on every packaged MSIX |
| The app builds no path under AppData | `RepositoryRulesTests.AppCode_BuildsNoPathUnderAppDataItself` |
| The MSIX is self-contained: it needs no other package | `scripts/Test-Msix.ps1` |
| The repository holds no MYM program, image, archive or database files | `RepositoryRulesTests.Repository_ContainsNoManagingYourMoneyFiles` |
| Releases are signed as Edwin West | `.github/workflows/release.yml`, `ReleaseRulesTests` |

The PowerShell scripts in `scripts/powershell` and the DOSBox-X script in `scripts/dosbox` are not part of the app (§5).

---

## 4. Verification checklist (pass/fail)

- [ ] `dotnet test SecretMoney.Core.Tests/SecretMoney.Core.Tests.csproj` passes.
- [ ] No `.cs` or `.xaml` file in `SecretMoney.Core` or `SecretMoney.UI` uses a network API.
- [ ] Every `PackageReference` is one of the four packages in §1.
- [ ] `SecretMoney.UI/Package.appxmanifest` declares only `runFullTrust`.
- [ ] The app reads and writes only what §2 lists: `settings.json` in `ApplicationData.Current.LocalFolder` and hledger's CSV in `ApplicationData.Current.TemporaryFolder` (`SecretMoney.UI/MainWindow.xaml.cs`, `HledgerRunner`), and QIF files in the folder chosen (`ExportFiles`).
- [ ] Only `HledgerRunner` starts a program, and only `hledger check` and `hledger print` (`RepositoryRulesTests`, `ExportWorkflowTests`).
- [ ] No file in the repository is a MYM program, disk image, archive or database (§7).
- [ ] Every file in `tests/qif` is ASCII with CRLF line endings (`QifFixtureTests`).
- [ ] `QifWriter` writes exactly what `docs/qif-format.md` version 1 specifies, and reproduces byte for byte the round-2 files MYM 12 imported (`QifWriterTests`, `QifTextTests`).
- [ ] `HledgerCsvReader` reads hledger 1.51.2's real output for the test period in `tests/secretbeans`, and each account's entries include the transfers and card payments recorded by the other account (`SecretBeansReaderTests`, `HledgerCsvReaderTests`).
- [ ] `ExportPlanner` follows `docs/export-rules.md`: for the test period it writes the known-answer files and balances, and it stops the export for each rule listed there (`ExportPlannerTests`, `ExportRulesTests`).
- [ ] `SettingsFile` writes exactly what `docs/settings-format.md` version 1 specifies and refuses anything else (`SettingsFileTests`).
- [ ] The window's steps behave as the docs say: the newest period is found as SecretBeans finds it; the accounts and categories offered; the files written, and only when nothing stops the export; the last export dates, which never move back (`ExportWorkflowTests`).
- [ ] The version is the same in `Directory.Build.props`, `SecretMoney.UI/Package.appxmanifest` and `CHANGELOG.md` (`ReleaseRulesTests`).
- [ ] The signing subject is the same in this README, `CLAUDE.md` and `release.yml` (`ReleaseRulesTests`).
- [ ] Every workflow action is pinned to a full commit SHA (`ReleaseRulesTests`).

---

## 5. The MYM 12 environment

Tested with Windows, PowerShell, DOSBox-X, MYM 12 for DOS from three 720 KB floppy images, and OneDrive. Step by step: [docs/installation/INSTALL.md](docs/installation/INSTALL.md), with details in `docs/installation/01-original-media.md` to `05-onedrive-drives.md`.

Host layout:

    C:\Users\<user>\OneDrive\MYM\
        DriveC\        DOS C: - MYM's program (C:\MYM12) and your MYM data
        DriveD\        DOS D: - QIF files to import

The MYM program and your MYM data stay outside this repository.

Scripts (not part of the app; run by hand from PowerShell or DOSBox-X):

- `scripts/powershell/Install-SecretMoney.ps1` — checks for winget, 7-Zip and DOSBox-X without installing anything, creates `OneDrive\MYM`, `DriveC` and `DriveD` if missing (supports `-WhatIf`), and prints the DOSBox-X mount commands. It downloads, copies and installs nothing.
- `scripts/dosbox/Mount-SecretMoney.bat` — run inside DOSBox-X: mounts C: and D: on the `OneDrive\MYM\DriveC` and `DriveD` folders and changes to `C:\MYM12`. The paths are hard-coded to Edwin's Windows user folder; edit them for another user.
- `scripts/powershell/New-Qif.ps1` — writes a one-transaction banking QIF file (ASCII, CRLF), by default `OneDrive\MYM\DriveD\IMPORT.QIF`, for testing imports by hand.

---

## 6. MYM 12 QIF research

Results of importing hand-made QIF files into MYM 12 are recorded in [docs/research/qif-compatibility.md](docs/research/qif-compatibility.md). The test files are in [tests/qif](tests/qif). In short:

- **Works:** dates, payments, deposits, payees, memos, categories, check numbers, split transactions, several entries in one file, and `!Type:CCard` into a Charge account (a charge negative, a payment positive).
- **Field lengths:** MYM keeps 25 characters of a payee, 40 of a memo, and at least 20 of a category (its register shows 15). It copies the rest of a long payee or category into the memo.
- **Categories:** created without asking; a colon is part of the name, not a subcategory.
- **Importing the same file twice:** MYM warns.
- **Transfers:** a QIF transfer to `L[Savings]` did not make MYM's linked transfer. An ordinary entry in each account under category `Transfer` keeps both balances right, but the two entries aren't linked.
- **Not tested:** cleared status (one test with `C*` showed no checkmark), investments.

MYM's file layout and its `BUDGET.DBS` database are described in `docs/research/mym-file-layout.md` and `docs/research/budget-dbs-findings.md`. SecretMoney does not write to MYM's databases; it uses QIF.

**What SecretMoney writes** is specified in [docs/qif-format.md](docs/qif-format.md), version 1, using only what MYM 12 was seen to import: ASCII with CR LF; `!Type:Bank` or `!Type:CCard`; per entry a date, an amount in whole cents, a payee of at most 25 characters, an optional memo of at most 40, and a category of at most 20 with no colon. It never writes transfers to `[Account]`, splits, check numbers or cleared status. `QifWriter` in `SecretMoney.Core` writes it and refuses an entry that breaks a rule; `QifWriterTests` locks it, including byte-for-byte copies of the round-2 files MYM imported.

---

## 7. What this repository never contains

- Managing Your Money disk images, installation archives, `MYM.EXE` or other original executables, program resources (`*.CHP`, `*.MYC`, `*.HLP`), documentation or artwork.
- MYM databases (`*.DBS`), bank statements, account numbers, credentials, or any personal financial data. QIF files are ignored by `.gitignore` except the synthetic test files in `tests/qif`.

`.gitignore` excludes these file types, and `RepositoryRulesTests.Repository_ContainsNoManagingYourMoneyFiles` fails if one is present. The one `.ico` file allowed is SecretMoney's own app icon.

---

## 8. How work is done here

SecretMoney is evidence-driven:

1. Test one MYM behaviour, with synthetic data, in a throwaway MYM data set.
2. Record exactly what happened.
3. Keep a reproducible test file.
4. Document the result.
5. Build only what has been verified.
6. Test what was built against MYM.

### AI-generated project

SecretMoney is written with AI. The MYM 12 environment, the installation guide, the first research and the PowerShell scripts were written with GitHub Copilot and Microsoft Copilot. Since 2026-11-10 the app, its tests and workflows, and later research are written by Claude (Anthropic). Edwin West decides what SecretMoney does, tests it, and approves every merge and release. AI-written code is marked:

- a file Claude wrote has a header naming Claude
- a line Claude changed in a file it didn't write has an `AI-generated (Claude):` comment
- each commit Claude makes ends with a `Co-Authored-By:` line naming it

---

## 9. Licensing

SecretMoney's own code and documentation are under the MIT License (see [LICENSE](LICENSE)). The license covers only material made for SecretMoney. It grants no rights to Managing Your Money or any other third-party software, documentation, trademarks, artwork or data.

---

## 10. Status and known limitations

Not finished:

- [ ] **Not used with real data yet.** The export window is built by CI but hasn't been run on Windows. Next: export a short period from Edwin's real SecretBeans ledger, import it into a throwaway MYM data set, and compare the balances.
- [ ] **Not tried yet:** hledger, started from the packaged app, writing its CSV into the app's temporary folder.
- [ ] **Late transactions.** A transaction dated on or before an account's last export date that reaches SecretBeans in a later download is never exported (`docs/export-rules.md`, "Dates"). Choose a last date your downloads are complete for.
- [ ] **0.1.0 is not released yet.** It is tested as pre-releases (`v0.1.0-rc.N`) with [docs/release-checklist.md](docs/release-checklist.md) before the release tag is pushed.
- [ ] `scripts/powershell/Install-SecretMoney.ps1` line 107 prints `ging Your Money files were downloaded or copied.`: the start of the message is missing.
- [ ] `.github/Copilot-Instructions.md` still describes the SecretForecast step (§0).
- [ ] The app's icons are SecretLead's.

---

## Install

Until 0.1.0 is released, the only builds are pre-releases, marked **Pre-release** on the Releases page and meant for testing.

1. Open this repository's [Releases page](https://github.com/edwest19/SecretMoney/releases) and download `SecretMoney-<version>-x64.msix` and `SecretMoney-<version>-x64.msix.sha256` from the newest release.
2. Open the file. Windows installs SecretMoney and shows the publisher, Edwin West. The installer is signed, so Windows can confirm who made it and that it hasn't been changed.
3. Start **SecretMoney** from the Start menu.

To uninstall, go to **Settings → Apps → Installed apps → SecretMoney → Uninstall**. Uninstalling deletes SecretMoney's package data folder (§2). Windows won't install a package over another one with the same version, so moving from a pre-release to the release of the same version means uninstalling first.

- The MSIX includes .NET and the Windows App SDK, so nothing else needs to be installed. It runs on 64-bit (x64) Windows.
- Each release also has a `.sha256` file. To check your download, compare it with the output of `Get-FileHash .\SecretMoney-<version>-x64.msix` in PowerShell.
- To check the signature, run `Get-AuthenticodeSignature .\SecretMoney-<version>-x64.msix | Format-List Status, SignerCertificate, TimeStamperCertificate` in PowerShell. `Status` should be `Valid`, the signer `CN=Edwin West, O=Edwin West, L=Huntington, S=ny, C=US`, and there should be a timestamp. The release workflow makes the same check before publishing.

Installing MYM 12 itself: §5.

## Using SecretMoney

1. In SecretBeans, build the period until its report shows nothing uncategorised or awaiting review.
2. Start SecretMoney. In **1. Where things are**, choose `hledger.exe`, the SecretBeans ledger folder (the one holding the period folders) and the QIF folder (`OneDrive\MYM\DriveD`, MYM's D: drive). SecretMoney remembers them.
3. **2. Read SecretBeans** runs `hledger check` on the newest period, then reads `generated.journal` and `opening.journal` through hledger, and shows the period's name and how many transactions it has.
4. **3. Accounts:** tick the accounts to export, and change a file name (up to 8 characters) or type (Bank or Charge) if you need to.
5. **4. Category names:** change any MYM category name (up to 20 characters, no colon). Transfers between your accounts are always `Transfer`.
6. **5. Dates:** the first start date is used only for accounts never exported; it starts as the period's first transaction date. The last date starts as the period's last transaction date.
7. **6. Check and write:** **Check** lists anything that stops the export, or each file with its entries, dates and the balance before and after. **Write QIF files** writes them, asks before replacing a file, and records each account's last export date.
8. In MYM: create an account never exported before with the **balance before** as its opening balance, import each file into its account from D:, and check that MYM shows the **balance after**.

## Building and testing

- Windows, the .NET 10 SDK, and Git. `global.json` selects a .NET 10 SDK.
- Run the tests (any OS):

  ```
  dotnet test SecretMoney.Core.Tests/SecretMoney.Core.Tests.csproj
  ```

- Build everything, including the WinUI app (Windows only):

  ```
  dotnet build SecretMoney.slnx -c Release
  ```

- `SecretMoney.UI` is packaged as an MSIX (full trust). Every pull request packages it, unsigned, with Microsoft's Windows App Development CLI (`winapp pack`), and `scripts/Test-Msix.ps1` checks the package: publisher, version, self-contained (no other packages needed), and no permission except `runFullTrust`.

## Releases

Releases are built, signed and published by `.github/workflows/release.yml`, only when a version tag is pushed.

1. Set the version in **three places**. `ReleaseRulesTests` fails if they disagree.
   - `Directory.Build.props`: `X.Y.Z`
   - `SecretMoney.UI/Package.appxmanifest`: `X.Y.Z.0`
   - `CHANGELOG.md`: a heading `## [X.Y.Z] - YYYY-MM-DD`, whose text becomes the release notes
2. Push a tag `vX.Y.Z`, or `vX.Y.Z-rc.N` for a pre-release.
3. The workflow then:
   - checks the tag against the version
   - runs the tests
   - packages the MSIX and checks it with `scripts/Test-Msix.ps1`
   - **waits for the owner's approval** in the GitHub environment `release`
   - signs the MSIX with Azure Artifact Signing (account `edwest19`, profile `CodeSigning-Apps`, East US)
   - checks that the signature is valid, timestamped and made by `CN=Edwin West, O=Edwin West, L=Huntington, S=ny, C=US`
   - publishes the GitHub Release with the MSIX and its `.sha256`
4. The owner installs the pre-release and works through [docs/release-checklist.md](docs/release-checklist.md). If it passes, the owner pushes `vX.Y.Z` on the **same commit**. If not, the fix goes through a pull request and the next pre-release is `-rc.N+1`.

Signing uses OpenID Connect, so no password or key is stored in GitHub. Every action is pinned to an exact commit. The workflows turn off the .NET CLI's and winapp's usage telemetry. Both workflows and `scripts/Test-Msix.ps1` are SecretLead's, with SecretMoney's names.

The repository uses GitHub's **immutable OIDC subject claims** (**Settings → Actions → OIDC → Use immutable subject claim**), so the sign-in token names the repository by its numeric IDs as well as its name. The Azure app registration `SecretMoney-GitHubActions-Signing` trusts exactly this subject:

`repo:edwest19@5809459/SecretMoney@1449898539:environment:release`

- A different repository that later takes the name `edwest19/SecretMoney` can't match it.
- Renaming this repository, its owner or the `release` environment changes the subject. The Azure federated credential must then be updated to match.

### One-time setup

What signing needs, done once by the owner (done 2026-11-10; first used by the sign job of `v0.1.0-rc.1`):

- **Azure:** the app registration `SecretMoney-GitHubActions-Signing`, with no certificate and no secret, and one federated credential:
  - issuer `https://token.actions.githubusercontent.com`
  - subject exactly the line above
  - audience `api://AzureADTokenExchange`

  The registration has the role **Artifact Signing Certificate Profile Signer** on the signing account `edwest19`.
- **GitHub:** the environment `release`, which requires the owner as reviewer and accepts only tags beginning with `v`. It holds three secrets: `AZURE_CLIENT_ID` (the registration's application ID), `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID`. They are identifiers, not passwords.
- **GitHub:** **Use immutable subject claim**, as above.

## Version history

See [CHANGELOG.md](CHANGELOG.md).
