# SecretMoney pre-release checklist

> Written by Claude (Anthropic) with Edwin West, 2026-11-11. SecretMoney is a project built with AI-generated code; see [README.md](../README.md).

The owner works through this list with each pre-release (`vX.Y.Z-rc.N`) on a Windows PC before pushing the release tag `vX.Y.Z` on the same commit (README, "Releases"). It installs the signed build and exports the synthetic period in `tests/secretbeans`, whose files and balances are known answers in `ExportPlannerTests` (`WholeJuly_IsStoppedByTheCheckAwaitingReview`, `FromJuly21_KnownAnswer`). It uses no real data and no MYM data; real data is a separate test (README §10).

The commands are Windows PowerShell, one line each, for 0.1.0-rc.1. For a later pre-release, change the version and tag in them. Each item is pass or fail; for a failure, note what the window or PowerShell showed.

## 1. Before the tag

- [ ] The repository on the PC is on `main`, up to date, and its tests pass: as many as the Build workflow ran on `main` (157 for 0.1.0-rc.1). The commands assume the clone is in `source\repos\SecretMoney` under the user's folder.

  ```powershell
  cd "$env:USERPROFILE\source\repos\SecretMoney"; git switch main; git pull; dotnet test SecretMoney.Core.Tests/SecretMoney.Core.Tests.csproj
  ```

- [ ] The `release` environment requires the owner's approval, accepts only `v*` tags, and holds the three Azure secrets:

  ```powershell
  cd "$env:USERPROFILE\source\repos\SecretMoney"; gh api repos/edwest19/SecretMoney/environments/release --jq ".protection_rules[].type"; gh api repos/edwest19/SecretMoney/environments/release/deployment-branch-policies --jq ".branch_policies[].name"; gh secret list --env release -R edwest19/SecretMoney
  ```

  Expect `required_reviewers` (and `branch_policy`), `v*`, and `AZURE_CLIENT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_TENANT_ID`.

## 2. Tag, approve, download

- [ ] Push the tag on the commit to release: `main` after the release pull request is merged. `git log` shows that merge.

  ```powershell
  cd "$env:USERPROFILE\source\repos\SecretMoney"; git switch main; git pull; git log -1 --oneline; git tag -a v0.1.0-rc.1 -m "SecretMoney 0.1.0-rc.1"; git push origin v0.1.0-rc.1
  ```

- [ ] In GitHub: **Actions → Release → Review deployments → `release` → Approve and deploy.** The run finishes green and the Releases page shows **SecretMoney 0.1.0-rc.1**, marked **Pre-release**, with `SecretMoney-0.1.0-x64.msix` and `SecretMoney-0.1.0-x64.msix.sha256`.
- [ ] Download both files:

  ```powershell
  gh release download v0.1.0-rc.1 -R edwest19/SecretMoney -D "$env:USERPROFILE\Downloads\SecretMoney-0.1.0-rc.1"
  ```

- [ ] The hash matches the `.sha256` file. Expect `hash matches`:

  ```powershell
  cd "$env:USERPROFILE\Downloads\SecretMoney-0.1.0-rc.1"; if ((Get-FileHash .\SecretMoney-0.1.0-x64.msix).Hash -eq (Get-Content .\SecretMoney-0.1.0-x64.msix.sha256).Split(' ')[0]) { 'hash matches' } else { 'HASH DIFFERS' }
  ```

- [ ] The signature is valid, made by Edwin West, and timestamped. Expect `Status : Valid`, a signer subject of `CN=Edwin West, O=Edwin West, L=Huntington, S=ny, C=US`, and a timestamp certificate:

  ```powershell
  cd "$env:USERPROFILE\Downloads\SecretMoney-0.1.0-rc.1"; Get-AuthenticodeSignature .\SecretMoney-0.1.0-x64.msix | Format-List Status, SignerCertificate, TimeStamperCertificate
  ```

## 3. Install

- [ ] Open `SecretMoney-0.1.0-x64.msix`. The installer shows SecretMoney, publisher Edwin West, and installs it.
- [ ] The installed package is version 0.1.0.0 from Edwin West:

  ```powershell
  Get-AppxPackage -Name 129542DC-DC08-4A7E-BD35-99912FB37B80 | Format-List Name, Version, Publisher, PackageFamilyName
  ```

- [ ] **SecretMoney** is in the Start menu (with SecretLead's icons, a known gap) and opens the export window, which begins "Turns the newest SecretBeans period into one QIF file per Managing Your Money account."

## 4. The synthetic period

- [ ] Copy the synthetic period into a test ledger folder, under a period name SecretBeans would use, and make an empty QIF folder:

  ```powershell
  cd "$env:USERPROFILE\source\repos\SecretMoney"; New-Item -ItemType Directory -Force "$env:TEMP\SecretMoney-rc\ledger", "$env:TEMP\SecretMoney-rc\qif" | Out-Null; Copy-Item -Recurse tests\secretbeans\period "$env:TEMP\SecretMoney-rc\ledger\2026-07 Test"; "$env:TEMP\SecretMoney-rc\ledger"; "$env:TEMP\SecretMoney-rc\qif"
  ```

  It prints the two folders' paths, to paste into the folder pickers.

- [ ] Find `hledger.exe`. It needn't be on the PATH, and on Edwin's PC it isn't (`docs/secretbeans-input.md`): the window asks for the file. This searches the user's folder, OneDrive included, and Program Files, and can take a minute:

  ```powershell
  Get-ChildItem -Path $env:USERPROFILE, $env:ProgramFiles -Filter hledger.exe -Recurse -ErrorAction SilentlyContinue | Select-Object -ExpandProperty FullName
  ```

  In the picker, paste the path into the **File name** box: folders under AppData are hidden.

- [ ] **1. Where things are:** choose `hledger.exe`, the ledger folder (`...\SecretMoney-rc\ledger`) and the QIF folder (`...\SecretMoney-rc\qif`). Each path shows in the window.
- [ ] **2. Read the newest period:** shows `Period: 2026-07 Test` and "Read 9 transactions, 2026-07-03 to 2026-07-31. hledger check passed." This is the first run of hledger started from the packaged app.
- [ ] hledger's CSV files were deleted from the app's temporary folder. Expect no output:

  ```powershell
  Get-ChildItem "$env:LOCALAPPDATA\Packages\$((Get-AppxPackage -Name 129542DC-DC08-4A7E-BD35-99912FB37B80).PackageFamilyName)\TempState"
  ```

- [ ] **3. Accounts:** three rows, all ticked, all "never exported": `assets:bank:checking` CHECKING Bank, `assets:bank:savings` SAVINGS Bank, `liabilities:creditcard:visa` VISA Charge.
- [ ] **4. Category names:** five rows, unchanged: `expenses:food:dining` dining, `expenses:food:groceries` groceries, `expenses:utilities:gas` gas, `income:interest` interest, `income:salary` salary.
- [ ] **5. Dates:** 2026-07-03 and 2026-07-31.
- [ ] **Check** with those dates is stopped: "Nothing can be written yet: 1 problem(s)." and a line for `2026-07-20 CHECK 501 (assets:bank:checking)` saying it is in `expenses:review:check`. **Write QIF files** stays unavailable.

## 5. Export and compare

- [ ] Change **First export starts on** to 2026-07-21 and press **Check**. Expect:
  - `CHECKING.QIF` 3 entries, 2026-07-21 to 2026-07-31, balance before 16,168.82, after 13,562.57
  - `SAVINGS.QIF` 2 entries, 2026-07-21 to 2026-07-31, balance before 2,000.00, after 4,401.23
  - `VISA.QIF` 1 entries, 2026-07-21 to 2026-07-31, balance before -1,281.31, after -1,081.31
  - "Ready to write to" the QIF folder
- [ ] **Write QIF files:** "Wrote 3 file(s)", and each account now shows "exported to 2026-07-31".
- [ ] The files are byte for byte the known answers in `ExportPlannerTests.FromJuly21_KnownAnswer`:

  ```powershell
  Get-FileHash "$env:TEMP\SecretMoney-rc\qif\*.QIF" | Format-Table Hash, Path
  ```

  | File | SHA-256 |
  |---|---|
  | `CHECKING.QIF` (243 bytes) | `E2C200D9A1FA0A2E04367245AC19C62DFD0C1A93E29185F14F111696617919CA` |
  | `SAVINGS.QIF` (173 bytes) | `7A9B2F1B8ED719AC9829D9751B5F2722E3CE586DF968EBD8DFCECA5C555A4010` |
  | `VISA.QIF` (75 bytes) | `6C361F12AA0851D683A3CB7F8FE7607FC0ED7F243FC5CFD8571D994E4FD30ECD` |

- [ ] **Check** again: each file says "already exported to 2026-07-31", and "There are no entries to write."

## 6. What the app keeps

- [ ] Close SecretMoney and start it again. The three paths are still shown. **Read the newest period** shows the three accounts "exported to 2026-07-31".
- [ ] `settings.json` holds the three paths and the three accounts with `"lastExported": "2026-07-31"`, and no amounts or transactions (`docs/settings-format.md`):

  ```powershell
  Get-Content "$env:LOCALAPPDATA\Packages\$((Get-AppxPackage -Name 129542DC-DC08-4A7E-BD35-99912FB37B80).PackageFamilyName)\LocalState\settings.json"
  ```

- [ ] Untick VISA and press **Check**, then **Read the newest period**: VISA is unticked and "never exported".
- [ ] Tick VISA, set **First export starts on** to 2026-07-21 and press **Check**: `VISA.QIF` 1 entries, balance before -1,281.31, after -1,081.31; the other two "already exported to 2026-07-31".
- [ ] **Write QIF files** asks "Replace files?" for `VISA.QIF`. **Cancel** writes nothing. **Write QIF files** again and **Replace**: "Wrote 1 file(s)", and `VISA.QIF`'s hash is unchanged.

## 7. Uninstall

- [ ] **Settings → Apps → Installed apps → SecretMoney → Uninstall.**
- [ ] The package data folder is gone (expect no output from the first part) and the three QIF files are still there:

  ```powershell
  Get-ChildItem "$env:LOCALAPPDATA\Packages" -Filter "129542DC-DC08-4A7E-BD35-99912FB37B80_*"; Get-ChildItem "$env:TEMP\SecretMoney-rc\qif"
  ```

- [ ] Remove the test folders:

  ```powershell
  Remove-Item -Recurse "$env:TEMP\SecretMoney-rc"
  ```

Uninstalling also clears the last export dates, so the real-data test starts from a fresh install. A release of the same version can't be installed over a pre-release, so uninstall the pre-release before installing the release.
