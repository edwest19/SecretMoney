# Reading SecretBeans

> Written by Claude (Anthropic) with Edwin West, 2026-11-10, from the code in `SecretMoney.Core/Ledger` and hledger's output in [tests/secretbeans](../tests/secretbeans). SecretMoney is a project built with AI-generated code; see [README.md](../README.md).

SecretMoney reads a SecretBeans period through **hledger**, the program SecretBeans itself uses, rather than parsing the journal files itself. hledger prints the journal as CSV; `HledgerCsvReader` reads that CSV; `LedgerAccounts` turns it into each account's entries. This is not a format SecretMoney writes: it is hledger's, as hledger 1.51.2 printed it, and the tests read hledger's real output.

## What hledger is asked to print

```text
hledger print -x -O csv -f <journal>
```

`-x` makes every amount explicit, so a posting written without one (`equity:opening balances` in `opening.journal`) gets its amount.

A SecretBeans period's `journal` folder has four files (SecretBeans' README, "What a run produces"): `main.journal` includes the other three; `accounts.journal` declares accounts; `opening.journal` holds the opening balances typed into SecretBeans; `generated.journal` holds every transaction from the bank and card CSVs. The export (not built yet) will read entries from `generated.journal` and opening balances from `opening.journal`, so the two never mix.

hledger has to be found. On Edwin's PC it is not on the PATH, so the app will need to be told where `hledger.exe` is.

## The CSV hledger prints

Observed from hledger 1.51.2 on Edwin's PC, 2026-11-10 (`tests/secretbeans/print-*.csv`):

- UTF-8 with no byte-order mark, CR LF after every row, every field in double quotes, a quote inside a field doubled.
- A header row of 14 columns: `txnidx`, `date`, `date2`, `status`, `code`, `description`, `comment`, `account`, `amount`, `commodity`, `credit`, `debit`, `posting-status`, `posting-comment`.
- One row per posting. A transaction's rows are together. Transactions are in date order.
- `txnidx` is the transaction's place in the journal files, not in the printed order: `STOP & SHOP 0042`, eighth in `generated.journal`, is printed third.
- `date` is `YYYY-MM-DD`.
- `amount` is a plain number: digits, `.` and two decimals, `-` in front when negative, and **no thousands separator**, even where the journal writes `$14,884.32`. The commodity is in its own column, `commodity` (`$`).

## What the reader uses and checks

`HledgerCsvReader.Read` uses six columns, found by name in any order: `txnidx`, `date`, `description`, `account`, `amount`, `commodity`. It ignores the rest. It reads CR LF or LF, skips a byte-order mark if there is one, and refuses (`LedgerFormatException`, saying where and why):

- an empty CSV, or a header missing one of the six columns
- a row with a different number of fields from the header, or a quoted field never closed
- a `txnidx` that isn't a positive whole number, or one whose rows are split up
- a date that isn't `YYYY-MM-DD`, or a transaction whose rows disagree on date or description
- an amount that isn't a plain number (a thousands separator or a currency sign is refused)
- an empty account
- a transaction whose postings don't add up to zero in each commodity

## One account's entries

`LedgerAccounts.EntriesFor(transactions, account)` gives one entry for each transaction with a posting to exactly that account: the whole name, matched exactly, with no subaccounts. The entry's amount is that account's postings added together; its other postings are the rest of the transaction; the order is hledger's.

SecretBeans records a transfer between two of your accounts, or a card payment, **once**: in the block of the account that owns it (SecretBeans `docs/TRANSFERS.md`). Seen from either account here, it is an entry in both. In the test data, the transfer from `checking` to `savings` is recorded by `checking` and the card payment by `visa`; `checking` has 7 entries, `savings` 2 and `visa` 2.

`LedgerAccounts.Total(transactions, account)` adds up an account's postings. Over `opening.journal`, that is the account's opening balance.

## Tested with

hledger 1.51.2 (`hledger 1.51.2-ge59e1837a-20260108, windows-x86_64`), on Edwin's PC. Other versions have not been tested. If a version prints different columns or number formats, the reader refuses its output rather than misreading it.
