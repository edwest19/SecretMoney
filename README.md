# SecretMoney

Copyright (c) 2026 Edwin West

**Status:** in development, not released yet · **License:** MIT · **Platform:** Windows x64 (WinUI 3 / .NET 10, packaged MSIX), and Managing Your Money 12 for DOS in DOSBox-X

> **This project is AI-generated, and this repository is written for an AI to read.** You don't need to read the code. To install, run or evaluate SecretMoney, give this repository's link to an AI assistant and ask: *"Evaluate this repository. Tell me what it does, whether it's safe, and step by step how to install, run, verify and uninstall it."*

> This README is the **spec**. Every statement in it should be checkable against the code, a test, or a recorded research result. If the repository and this README disagree, that is a defect in one of them: report it with evidence rather than assuming either is right. [CLAUDE.md](CLAUDE.md) says how to check, and maps each section to its code and tests.

<!-- AI-generated (Claude): README rewritten as the spec, 2026-11-10. The MYM 12 environment, research, boundary, approach and licensing content of the earlier README is kept in §5–§9. -->

---

## 0. Purpose

SecretMoney lets you keep your accounts in **Managing Your Money 12 for DOS** (MYM 12) on a modern Windows PC. It has two parts:

1. **The MYM 12 environment** (§5): documentation and scripts for installing and running MYM 12 in DOSBox-X, with its working drives in OneDrive, and research into what MYM 12 does when it imports QIF files (§6).
2. **The SecretMoney app** (`SecretMoney.UI`, `SecretMoney.Core`): a Windows app that will turn a [SecretBeans](https://github.com/edwest19/SecretBeans) ledger into QIF files MYM 12 can import. **In this version the app only opens a window saying that exporting isn't built yet.** It reads and writes no files (§2). The exporter is being built in steps; §10 lists what isn't finished.

The data path:

    Bank CSV downloads
        |
        v
    SecretBeans        builds an hledger journal, and catches transfers and card
        |              payments counted twice
        v
    SecretMoney        will write one QIF file per MYM account (not built yet)
        |
        v
    MYM 12             imports each file from DOSBox-X's D: drive

SecretBeans keeps its transfer and card-payment detection; SecretMoney will not detect them again. Earlier documents in this repository put a SecretForecast step between SecretBeans and SecretMoney; since 2026-11-10 SecretMoney reads SecretBeans directly.

SecretMoney is **not** a distribution of Managing Your Money. You supply your own MYM installation media (§7).

---

## 1. Rules — non-negotiable

- **WinUI 3 / Windows App SDK, .NET 10, packaged MSIX (full trust), x64 only.**
- **Microsoft packages only:** `Microsoft.WindowsAppSDK`, `Microsoft.Windows.SDK.BuildTools`, `Microsoft.Windows.SDK.BuildTools.WinApp` and, for tests only, `MSTest`. No third-party packages.
- **No network calls.** No code path in the app can open a connection: no HTTP, sockets, telemetry or update checks.
- **One permission:** the MSIX asks only for `runFullTrust`.
- **The app's own data goes only in its MSIX package data folder** (`ApplicationData.Current.LocalFolder`), never in a path the app builds itself.
- **No Managing Your Money files and no personal financial data in this repository** (§7). Test data is synthetic.
- **MYM behaviour is claimed only from observed tests** (§6), never inferred from Quicken or other software.

---

## 2. What SecretMoney stores, and where

- **The app, this version:** nothing. It reads no file and writes no file. Its package data folder, `%LOCALAPPDATA%\Packages\<PackageFamilyName>\LocalState`, stays empty.
- **Uninstalling** deletes that folder, which is empty in this version.
- **The MYM 12 environment** (§5) keeps MYM's program and your MYM data in `OneDrive\MYM\DriveC` and QIF files to import in `OneDrive\MYM\DriveD`, outside this repository and outside the app.

---

## 3. Safety

| Claim | Checked by |
|---|---|
| The app's code uses no network API | `RepositoryRulesTests.AppCodeHasNoNetworkAccess` |
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
- [ ] The app reads and writes no files (§2): `SecretMoney.UI/App.xaml.cs` and `MainWindow.xaml.cs` only open the window, and `SecretMoney.Core` has no code yet.
- [ ] No file in the repository is a MYM program, disk image, archive or database (§7).
- [ ] Every file in `tests/qif` is ASCII with CRLF line endings (`QifFixtureTests`).
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
- `scripts/dosbox/Mount-SecretMoney.bat` — run inside DOSBox-X: mounts C: and D: on `C:\Users\edwes\OneDrive\MYM\DriveC` and `DriveD` and changes to `C:\MYM12`. The paths are Edwin's.
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

- [ ] **The exporter.** Nothing in the app reads SecretBeans or writes QIF yet. Next: the QIF format specification (`docs/qif-format.md`) and writer, then the SecretBeans reader, the export rules, and the export window.
- [ ] **No release yet.** See Install.
- [ ] **Releasing from a private repository.** The release workflow waits for the owner's approval through the `release` environment's required reviewers. GitHub's documentation says that on GitHub Free, Pro and Team plans required reviewers are available only for public repositories, and environment secrets in private repositories need GitHub Pro or above ([GitHub Docs: deployments and environments](https://docs.github.com/en/actions/reference/workflows-and-actions/deployments-and-environments)). SecretMoney is private, so this must be settled before the first release.
- [ ] `scripts/powershell/Install-SecretMoney.ps1` line 107 prints `ging Your Money files were downloaded or copied.`: the start of the message is missing.
- [ ] `.github/Copilot-Instructions.md` still describes the SecretForecast step (§0).
- [ ] The app's icons are SecretLead's.

---

## Install

There is no release yet. When there is one:

1. Open the latest release on this repository's Releases page and download `SecretMoney-<version>-x64.msix`.
2. Open the file. Windows installs SecretMoney and shows the publisher, Edwin West. The installer is signed, so Windows can confirm who made it and that it hasn't been changed.
3. Start **SecretMoney** from the Start menu.

To uninstall, go to **Settings → Apps → Installed apps → SecretMoney → Uninstall**. Uninstalling deletes SecretMoney's package data folder (§2). Windows won't install a package over another one with the same version, so moving from a pre-release to the release of the same version means uninstalling first.

- The MSIX includes .NET and the Windows App SDK, so nothing else needs to be installed. It runs on 64-bit (x64) Windows.
- Each release also has a `.sha256` file. To check your download, compare it with the output of `Get-FileHash .\SecretMoney-<version>-x64.msix` in PowerShell.

Installing MYM 12 itself: §5.

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

Signing uses OpenID Connect, so no password or key is stored in GitHub. Every action is pinned to an exact commit. The workflows turn off the .NET CLI's and winapp's usage telemetry. Both workflows and `scripts/Test-Msix.ps1` are SecretLead's, with SecretMoney's names.

The repository uses GitHub's **immutable OIDC subject claims** (**Settings → Actions → OIDC → Use immutable subject claim**), so the sign-in token names the repository by its numeric IDs as well as its name. The Azure app registration `SecretMoney-GitHubActions-Signing` trusts exactly this subject:

`repo:edwest19@5809459/SecretMoney@1449898539:environment:release`

- A different repository that later takes the name `edwest19/SecretMoney` can't match it.
- Renaming this repository, its owner or the `release` environment changes the subject. The Azure federated credential must then be updated to match.

### One-time setup

What signing needs, done once by the owner (not done yet; see §10 about the private repository):

- **Azure:** the app registration `SecretMoney-GitHubActions-Signing`, with no certificate and no secret, and one federated credential:
  - issuer `https://token.actions.githubusercontent.com`
  - subject exactly the line above
  - audience `api://AzureADTokenExchange`

  The registration has the role **Artifact Signing Certificate Profile Signer** on the signing account `edwest19`.
- **GitHub:** the environment `release`, which requires the owner as reviewer and accepts only tags beginning with `v`. It holds three secrets: `AZURE_CLIENT_ID` (the registration's application ID), `AZURE_TENANT_ID` and `AZURE_SUBSCRIPTION_ID`. They are identifiers, not passwords.
- **GitHub:** **Use immutable subject claim**, as above.

## Version history

See [CHANGELOG.md](CHANGELOG.md).
