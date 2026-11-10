# SecretBeans test data

> Written by Claude (Anthropic) with Edwin West, 2026-11-10.

A synthetic SecretBeans period, for building and testing SecretMoney's SecretBeans reader. Every
name and amount is made up.

`period/journal/` holds the four files SecretBeans writes into a period's `journal` folder, in the
shapes its `SbBuild.ps1` and `SbJournal.ps1` write them (UTF-8, CR LF):

| File | Holds |
|---|---|
| `main.journal` | `include`s the other three |
| `accounts.journal` | an `account` line for every account used |
| `opening.journal` | opening balances for `checking` ($14,884.32), `savings` ($2,000.00) and `visa` (-$1,250.00), dated 2026-07-02 |
| `generated.journal` | nine transactions from three accounts' CSVs, each block headed `; ---- name (account) ----` |

`generated.journal` covers what the reader has to handle: purchases, a deposit, a check sent to
`expenses:review:check`, a transfer from `checking` to `savings` recorded once (by `checking`), a
card payment recorded once (by `visa`), an amount over 1,000, and a description with accented
letters, a comma and double quotes.

## hledger's output

The `print-*.csv` files are what hledger 1.51.2 printed for these journals on Edwin's PC, with
`hledger print -x -O csv -f <journal> -o <file>`. SecretMoney reads SecretBeans through this
output (README §0), so the reader's tests read these files rather than a guess at hledger's format.

| File | From |
|---|---|
| `print-main.csv` | `period/journal/main.journal` |
| `print-generated.csv` | `period/journal/generated.journal` |
| `print-opening.csv` | `period/journal/opening.journal` |
