# SecretMoney export rules

> Written by Claude (Anthropic) with Edwin West, 2026-11-10, from the code in `SecretMoney.Core/Export` and Edwin's decisions of 2026-11-10. SecretMoney is a project built with AI-generated code; see [README.md](../README.md).

These rules turn a SecretBeans period (read as [secretbeans-input.md](secretbeans-input.md) describes) into one QIF file per MYM account (written as [qif-format.md](qif-format.md) specifies). `ExportPlanner.Plan` applies them and returns the files with anything that stops the export; the app writes the files only when nothing does. Tested by `ExportPlannerTests`, `ExportRulesTests` and `ExportWorkflowTests`, on the synthetic period in `tests/secretbeans`.

## Accounts

- **Which accounts.** SecretBeans writes a line before each account's block in `generated.journal`: `; ---- name (account) ----`. Those accounts, in that order, are the ones offered for export (`ExportRules.AccountChoices`). The first time, all of them are ticked. After that, the accounts in the settings are ticked, with their file names, types and last export dates; an account in the settings that the period no longer has is listed after the others, still ticked. Unticking an account removes it from the settings, with its last export date.
- **File name.** The SecretBeans name in upper case, keeping only A–Z, 0–9, `_` and `-`, cut to 8 characters, then `.QIF`: `checking` → `CHECKING.QIF`. It can be changed in the settings; it must stay 1 to 8 of those characters, and no two accounts may share one (compared ignoring case).
- **Type.** An account starting `liabilities:` is a Charge account (`!Type:CCard`); any other is a bank account (`!Type:Bank`). It can be changed in the settings.

## Dates

- **First export.** You choose the start date. Each account's entries start on that date.
- **After that.** An account's entries start the day after its `lastExported` date.
- **End.** You choose the last date; entries up to and including it are written.
- **Balances.** For each account the export gives the balance at the end of the day before its first entry date, and at the end of the last date. The first is the opening balance to type when creating the account in MYM, or what MYM should already show; the second is what MYM should show after the import. A balance is the account's opening balance in `opening.journal` plus every entry in `generated.journal` up to that day.
- After the files are written, `lastExported` becomes the last date for every account in the export, including one with no entries in its dates, unless it was already later: it never moves back (`ExportSettings.AfterExport`). An account already exported past the last date has nothing to write.
- **Choose a last date your downloads are complete for.** The next export starts the day after it, so a transaction dated on or before it that only reaches SecretBeans in a later download is never exported. MYM's balance would then differ from the one SecretMoney shows.

## Entries

One entry for each transaction with a posting to the account (secretbeans-input.md, "One account's entries"), in hledger's order:

| QIF | From |
|---|---|
| `D` | The transaction's date |
| `T` | The account's own amount. For a Charge account a charge is negative and a payment positive, as hledger already has it |
| `P` | The description, cleaned, first 25 characters |
| `M` | When the cleaned description is longer than 25 characters, its first 40; otherwise none |
| `L` | `Transfer` when the other posting is to an `assets:` or `liabilities:` account; otherwise the name chosen in the settings for the other account, or by default the other account's last part in lower case, cleaned, first 20 characters: `expenses:food:groceries` → `groceries` |

**Choosing category names.** The window lists every hledger account the chosen accounts' entries use as a category, in name order, with its name now (`ExportRules.CategoryChoices`): not transfers, not anything uncategorised or `equity:`, and not entries with more than one other posting. The settings keep only names that differ from the default (`ExportRules.ChosenCategories`), and keep names chosen for accounts this period doesn't use.

A transfer between two of your accounts, or a card payment, is recorded once by SecretBeans but is an entry in both accounts, both under `Transfer`. MYM keeps both balances right but does not link the two entries (research, round 2).

## What stops an export

The export writes nothing while any of these is true. Each is listed with its account, date and description:

- An entry whose other posting is to SecretBeans' review account (`expenses:review` or under it) or to any account with a part named `unknown`: categorise it in SecretBeans and build again.
- An entry whose other posting is to an `equity:` account: give it a real account in SecretBeans' rules.
- An entry with more than one other posting (a split), an amount in anything but `$`, an amount of zero for the account, or an empty description.
- Two hledger accounts that would become the same MYM category (compared ignoring case), including an expense account whose name would be `Transfer`: choose a different name for one of them.
- A category name, chosen or default, that `qif-format.md` doesn't allow.
- No accounts chosen, a file name that isn't 1 to 8 of A–Z, 0–9, `_`, `-`, two accounts with the same file name, or an account listed twice.

An export with no entries in its dates writes nothing either.

## Writing the files

`ExportFiles.Write` writes each file that has entries into the QIF folder, as `qif-format.md` specifies, and writes no file for an account with none. It refuses a plan with anything that stops it, or with no entries, and a folder that doesn't exist. A file of the same name is replaced; the window lists those first (`ExportFiles.Existing`) and asks before replacing them. Then the window sets `lastExported` (Dates, above) and saves the settings.
