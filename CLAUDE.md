# CLAUDE.md

> Written by Claude (Anthropic) with Edwin West, 2026-11-10. Guidance for Claude Code, or any AI, working in or evaluating this repository.

## Ground rules

- **`README.md` is the spec.** If code and README disagree, that's a defect in one of them: fix the code, or say the README is stale and propose the edit. Never drift silently.
- **Edwin decides; Claude engineers.** One task is one branch (`feature/…`, `fix/…`, `docs/…`, `release/…`) and one pull request. Merge only after Edwin says "yes, merge", with a merge commit. Never push to `main`, force-push or rewrite history.
- **Ask first** before adding or upgrading a package, changing a file format or what the app stores, changing workflows, signing or permissions, or deleting anything outside the change.
- **Microsoft packages only** (README §1). **No network API** in `SecretMoney.Core` or `SecretMoney.UI`. **Only `runFullTrust`.** The app's data goes only in `ApplicationData.Current.LocalFolder`.
- **No MYM files or personal financial data** in the repository (README §7). Tests use synthetic data.
- **MYM behaviour comes from observed tests** (`docs/research/qif-compatibility.md`), never from what Quicken or other software does. A new MYM behaviour the code relies on needs a test file in `tests/qif` and a recorded result first.
- **Never change the QIF format silently.** `docs/qif-format.md` is locked by `QifWriterTests`. A change needs Edwin's approval, a new version of the spec, and a new import test in MYM.
- **Never change the settings format silently.** `docs/settings-format.md` is locked by `SettingsFileTests`. A change needs Edwin's approval, a new version of the spec, and a plan for settings files already on disk.
- **Bugs are fixed test-first:** a failing test, pushed and shown failing in CI, then the fix.
- **Never weaken, skip or delete a test to make it pass**, or change a known-answer test's expected values.
- **Never work around a refused permission or security check.** Say what was refused and give Edwin the proper fix.
- **No guessing.** Facts come from the code, the logs, a recorded test, or a cited source. Say where every test ran: the cloud Linux container, CI on `windows-latest`, or Edwin's PC.
- **Mark AI-written code:** a header naming Claude in each file Claude writes; an `AI-generated (Claude):` comment on lines Claude changes in files it didn't write; `Co-Authored-By:` at the end of each commit.
- Commands for Edwin are Windows PowerShell, one line each, starting with `cd` to the repository's folder on his PC.
- **The repository is public.** No PC names, Windows user folders or other personal details in files Claude writes; refer to "Edwin's PC".

## Layout

- `SecretMoney.Core/` — everything that isn't the window. Plain .NET 10, no Windows dependencies.
  - `Qif/` — `QifWriter` (writes `docs/qif-format.md` version 1), `QifText` (cleaning and fitting text), `QifTransaction`, `QifAccountType`, `MymLimits` (25 / 40 / 20, from round 2), `QifFormatException`.
  - `Ledger/` — `SecretBeansPeriod` (finds the newest period, as SecretBeans' README does), `HledgerRunner` (runs `hledger check` and `hledger print -x -O csv`; the only code that starts a program), `HledgerException`, `HledgerCsvReader` (reads that CSV, `docs/secretbeans-input.md`), `LedgerAccounts` (one account's entries; totals), `LedgerTransaction`, `LedgerPosting`, `AccountEntry`, `LedgerFormatException`.
  - `Export/` — `ExportPlanner` (applies `docs/export-rules.md`; returns the files, their balances and anything that stops the export), `ExportRules` (each rule on its own; finds the accounts in `generated.journal`; the accounts and categories the window offers), `ExportFiles` (writes a plan's files into a folder), `SettingsFile` (`docs/settings-format.md` version 1), `ExportSettings` (and `AfterExport`, the last export dates), `MymAccount`, `AccountChoice`, `CategoryChoice`, `ExportPlan`, `ExportFile`, `ExportProblem`, `SettingsFormatException`.
- `SecretMoney.Core.Tests/` — MSTest tests:
  - `RepositoryRulesTests` — Microsoft packages only, no network API, no program started but hledger, only `runFullTrust`, no path built under AppData, no MYM file types
  - `ReleaseRulesTests` — publisher, version in three places, changelog, signing subject, pinned actions, package name, x64, solution file
  - `QifFixtureTests` — `tests/qif/*.qif` are ASCII, CRLF, with a `!Type` header and closed records
  - `QifWriterTests` — known-answer tests for `docs/qif-format.md`, including byte-for-byte copies of the round-2 files MYM imported
  - `QifTextTests` — the text rules, step by step
  - `SecretBeansReaderTests` — reads `tests/secretbeans/print-*.csv`, hledger 1.51.2's real output for the synthetic period
  - `HledgerCsvReaderTests` — the CSV rules and every refusal, on hand-made input
  - `ExportPlannerTests` — the export rules on the test period (known-answer files and balances) and every rule that stops an export
  - `ExportRulesTests` — each rule on its own
  - `SettingsFileTests` — known-answer tests for `docs/settings-format.md` and every refusal
  - `ExportWorkflowTests` — the window's steps in Core: finding the period, the hledger commands, the accounts and categories offered, writing the files, the last export dates
  - `TestHelper` — finds the repository root (the folder with `SecretMoney.slnx`); `TempFolder`, a test's own temporary folder
- `SecretMoney.UI/` — the WinUI 3 app, packaged as MSIX (x64). `App.xaml(.cs)`, `MainWindow.xaml(.cs)` (the export window; it only connects controls to Core, and keeps `settings.json` in `ApplicationData.Current.LocalFolder`), `Rows.cs` (`AccountRow`, `CategoryRow`: the window's list rows), `Package.appxmanifest`, `app.manifest`, `Assets/` (SecretLead's icons), `Properties/launchSettings.json`.
- `docs/installation/` — installing MYM 12 in DOSBox-X: `INSTALL.md` and `01`–`05`. Written with Copilot.
- `docs/research/` — `qif-compatibility.md` (what MYM 12 does with QIF), `mym-file-layout.md`, `budget-dbs-findings.md`.
- `tests/qif/` — synthetic QIF test files and their `README.md` (questions and results).
- `scripts/Test-Msix.ps1` — checks a packaged MSIX (used by both workflows).
- `scripts/powershell/` — `Install-SecretMoney.ps1` (creates the OneDrive MYM folders), `New-Qif.ps1` (writes a one-transaction QIF). Written with Copilot; not part of the app.
- `scripts/dosbox/Mount-SecretMoney.bat` — DOSBox-X mounts. Written with Copilot; not part of the app.
- `.github/workflows/build.yml` — every pull request and push to `main`: tests, Windows build, unsigned MSIX packaged and checked.
- `.github/workflows/release.yml` — on a `v*` tag: check, test, package, sign after approval, publish.
- `.github/Copilot-Instructions.md` — instructions for GitHub Copilot (Edwin's).
- `Directory.Build.props` (the version), `global.json` (.NET 10 SDK), `SecretMoney.slnx`, `CHANGELOG.md`, `.gitattributes` (keeps QIF bytes exact), `.gitignore`, `LICENSE`.

## Build and test

- Tests, any OS: `dotnet test SecretMoney.Core.Tests/SecretMoney.Core.Tests.csproj`
- Everything including the window, Windows only: `dotnet build SecretMoney.slnx -c Release`
- In a cloud Linux session, only the tests run. The Windows build and MSIX check run in CI.

## How to evaluate this repository

1. Run the tests and report the result and where they ran.
2. Check every README statement against the code and tests, using the claims map below.
3. Check `docs/research/qif-compatibility.md` claims against the files in `tests/qif` and their recorded results.
4. Report each finding as **statement, evidence (file and line), verdict**. Verdicts: verified; partly verified; documentation only; implemented but undocumented; contradiction; unable to verify.

## README claims map

| README | Code | Tests |
|---|---|---|
| §0, Using SecretMoney: the export window | `SecretMoney.UI/MainWindow.xaml(.cs)`, `Rows.cs` | `ExportWorkflowTests` (the Core steps it calls); the window itself — read the code |
| §1 Microsoft packages only | every `.csproj` | `RepositoryRulesTests.OnlyAllowedMicrosoftPackagesAreReferenced` |
| §1 no network calls | `SecretMoney.Core`, `SecretMoney.UI` | `RepositoryRulesTests.AppCodeHasNoNetworkAccess` |
| §1, §3 one other program: hledger | `SecretMoney.Core/Ledger/HledgerRunner.cs` | `RepositoryRulesTests.AppCode_StartsNoProgramButHledger`, `ExportWorkflowTests.HledgerArguments_AreTheRecordedCommands` |
| §1 only `runFullTrust` | `SecretMoney.UI/Package.appxmanifest` | `RepositoryRulesTests.AppManifest_DeclaresOnlyRunFullTrust`; `scripts/Test-Msix.ps1` in CI |
| §1, §2 data only in the package data folder | `SecretMoney.UI`, `SecretMoney.Core` | `RepositoryRulesTests.AppCode_BuildsNoPathUnderAppDataItself` |
| §2 what the app reads and writes | `SecretMoney.UI/MainWindow.xaml.cs`, `HledgerRunner`, `ExportFiles` | `ExportWorkflowTests`; the window — read the code |
| §5 scripts | `scripts/powershell/*.ps1`, `scripts/dosbox/*.bat` | — (read the scripts) |
| §6 MYM 12 QIF behaviour | `docs/research/qif-compatibility.md`, `tests/qif/` | `QifFixtureTests` (file bytes only; the results were observed in MYM) |
| §1, §6 what SecretMoney writes | `docs/qif-format.md`, `SecretMoney.Core/Qif/` | `QifWriterTests`, `QifTextTests` |
| §0 export rules | `docs/export-rules.md`, `SecretMoney.Core/Export/` | `ExportPlannerTests`, `ExportRulesTests`, `ExportWorkflowTests` |
| §2 the settings file | `docs/settings-format.md`, `SecretMoney.Core/Export/SettingsFile.cs` | `SettingsFileTests` |
| §0 reading SecretBeans through hledger | `docs/secretbeans-input.md`, `SecretMoney.Core/Ledger/`, `tests/secretbeans/` | `SecretBeansReaderTests`, `HledgerCsvReaderTests`, `ExportWorkflowTests` |
| §7 no MYM files | `.gitignore` | `RepositoryRulesTests.Repository_ContainsNoManagingYourMoneyFiles` |
| Install: self-contained MSIX | `SecretMoney.UI/SecretMoney.UI.csproj`, workflows | `scripts/Test-Msix.ps1` in CI |
| Releases: version in three places | `Directory.Build.props`, `Package.appxmanifest`, `CHANGELOG.md` | `ReleaseRulesTests.ManifestVersion_IsTheProjectVersionPlusZero`, `Changelog_HasAnEntryForTheProjectVersion` |
| Releases: signer, account, profile | `.github/workflows/release.yml` | `ReleaseRulesTests.ManifestPublisher_IsTheNameReleasesAreSignedAs`, `Readme_ReleaseSection_MatchesTheWorkflow` |
| Releases: signing subject | README, this file, `release.yml` | `ReleaseRulesTests.SigningSubject_IsStatedTheSameEverywhere_ForTheReleaseEnvironment` |
| Releases: pinned actions, x64 | `.github/workflows/*.yml`, `SecretMoney.UI.csproj` | `ReleaseRulesTests.WorkflowActions_ArePinnedToExactCommits`, `App_IsBuiltAndPackagedForX64Only` |

## Releases

- Only through `.github/workflows/release.yml`, on a pushed `vX.Y.Z` or `vX.Y.Z-rc.N` tag, with Edwin's approval of the sign job. Never sign or publish by hand. Edwin pushes tags.
- The version lives in `Directory.Build.props` (`X.Y.Z`), `SecretMoney.UI/Package.appxmanifest` (`X.Y.Z.0`) and `CHANGELOG.md` (`## [X.Y.Z] - YYYY-MM-DD`).
- Signing: Azure Artifact Signing, account `edwest19`, profile `CodeSigning-Apps`, publisher `CN=Edwin West, O=Edwin West, L=Huntington, S=ny, C=US`. The Azure app registration `SecretMoney-GitHubActions-Signing` trusts one immutable OIDC subject, `repo:edwest19@5809459/SecretMoney@1449898539:environment:release`. Renaming the repository, its owner or the `release` environment breaks signing until the federated credential is updated.

## Known gaps

- The export window has been built by CI but not run on Windows, and SecretMoney hasn't been used with real data (README §10). Not tried: hledger, started from the packaged app, writing its CSV into `ApplicationData.Current.TemporaryFolder`.
- A transaction dated on or before an account's `lastExported` that reaches SecretBeans in a later download is never exported (`docs/export-rules.md`, "Dates").
- No Backup for the settings file: Edwin chose that uninstalling deletes everything (2026-11-10).
- `scripts/powershell/Install-SecretMoney.ps1` line 107 has a truncated message.
- `.github/Copilot-Instructions.md` still names SecretForecast.
