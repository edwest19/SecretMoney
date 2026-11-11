# SecretMoney settings file, version 1

> Written by Claude (Anthropic) with Edwin West, 2026-11-10, from the code in `SecretMoney.Core/Export/SettingsFile.cs`. SecretMoney is a project built with AI-generated code; see [README.md](../README.md).

SecretMoney keeps everything it remembers in one settings file, `settings.json`, in its MSIX package data folder (README §2). **Uninstalling SecretMoney deletes it** (Edwin's choice, 2026-11-10). `SettingsFile` reads and writes it; the known-answer tests in `SecretMoney.Core.Tests/SettingsFileTests.cs` lock it. **Changing it needs the owner's approval**, a new version of this document, and a plan for settings files already on disk. Never change a known-answer test's expected values to make it pass.

Version 1 gained `ledgerFolder` and `qifFolder` on 2026-11-10, with Edwin's approval, before SecretMoney had written any settings file.

## Bytes

- UTF-8 JSON, no byte-order mark.
- Written indented by two spaces, LF line endings, ending with LF, fields in the order below.
- Written with .NET's default JSON escaping: characters outside ASCII and the characters `"`, `&`, `'`, `+`, `<`, `>` and `` ` `` are written as `\u` escapes (`&` is `\u0026`, `é` is `\u00E9`), and a backslash as `\\`. Read with any valid JSON escaping.

## Fields

Every field is required; a field the format doesn't have, or a field given twice, is refused.

| Field | Holds |
|---|---|
| `formatVersion` | `1` |
| `hledgerPath` | Where `hledger.exe` is, or `null` if not chosen yet |
| `ledgerFolder` | SecretBeans' ledger root, the folder holding its period folders, or `null` |
| `qifFolder` | The folder QIF files are written to (DOSBox-X's D: drive), or `null` |
| `reviewAccount` | SecretBeans' review account, normally `expenses:review`. Non-empty |
| `accounts` | The accounts to export, in the order shown; may be empty |
| `categories` | MYM category names chosen in place of the defaults: hledger account → name. Written in ordinal order of the account; may be empty |

Each entry of `accounts`, every field required:

| Field | Holds |
|---|---|
| `hledgerAccount` | The account's full hledger name. Non-empty; no account listed twice |
| `secretBeansName` | Its short name in SecretBeans. Non-empty |
| `fileName` | The QIF file's name without `.QIF`. Non-empty (the export rules say what it may contain) |
| `type` | `Bank` or `CCard` (a MYM Charge account) |
| `lastExported` | The last date already exported, `YYYY-MM-DD`, or `null` if never exported |

Each value of `categories` is a non-empty string. Whether it can be written to MYM is checked when exporting (docs/export-rules.md), not when reading.

`SettingsFile.Read` refuses (`SettingsFormatException`, saying why) anything else: text that isn't JSON, another `formatVersion`, a field of the wrong kind, a `type` other than `Bank` or `CCard`, or a date not in `YYYY-MM-DD`.

## Example

Two accounts, one already exported to 2026-07-31, and two chosen category names. `SettingsFileTests` checks that `SettingsFile.Write` produces exactly this (673 bytes) and reads it back the same:

```json
{
  "formatVersion": 1,
  "hledgerPath": "C:\\Tools\\hledger.exe",
  "ledgerFolder": "C:\\Ledger",
  "qifFolder": "C:\\MYM\\DriveD",
  "reviewAccount": "expenses:review",
  "accounts": [
    {
      "hledgerAccount": "assets:bank:checking",
      "secretBeansName": "checking",
      "fileName": "CHECKING",
      "type": "Bank",
      "lastExported": "2026-07-31"
    },
    {
      "hledgerAccount": "liabilities:creditcard:visa",
      "secretBeansName": "visa",
      "fileName": "VISA",
      "type": "CCard",
      "lastExported": null
    }
  ],
  "categories": {
    "expenses:food:groceries": "Groceries",
    "expenses:utilities:gas": "Gas \u0026 electric"
  }
}
```

With nothing chosen yet, the file is:

```text
{
  "formatVersion": 1,
  "hledgerPath": null,
  "ledgerFolder": null,
  "qifFolder": null,
  "reviewAccount": "expenses:review",
  "accounts": [],
  "categories": {}
}
```
