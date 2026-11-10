# QIF test fixtures

> Written by Claude (Anthropic) with Edwin West, 2026-11-10. The `.qif` files here have no
> header because QIF has no comment syntax; this file is their header.

Synthetic QIF files for testing what Managing Your Money 12 for DOS does on import. Every
value is made up. Results are recorded in
[docs/research/qif-compatibility.md](../../docs/research/qif-compatibility.md).

All files are ASCII with CRLF line endings and a final CRLF. `.gitattributes` stops Git from
converting them. File names are lower-case 8.3 so DOS sees them unchanged (DOS shows them in
upper case).

## Round 2 (2026-11-10)

Round 2 answers the questions the SecretBeans-to-QIF exporter depends on. The exporter's
planned choices are: one QIF file per MYM account; transfers and card payments written as an
ordinary entry in **both** accounts under one category named `Transfer` (because `L[Savings]`
failed in round 1); categories taken from the last part of the hledger account name, in lower
case (`expenses:food:groceries` becomes `groceries`).

| File | Import into | Tests |
|---|---|---|
| `t1ccard.qif` | credit-card account `Card` | `!Type:CCard`; a charge `T-31.31` and a payment `T200.00` |
| `t1bbank.qif` | credit-card account `Card` | Only if `t1ccard.qif` is refused: the same two entries as `!Type:Bank` |
| `t2long.qif` | `Chase` | 80-character payee and memo, 40-character category, a category with a colon, punctuation in a payee |
| `t3three.qif` | `Chase`, then a second time | three entries in one file, lower-case categories, an amount over 1,000; then re-import for duplicates |
| `t4chk.qif` | `Chase` | transfer as an ordinary entry, `T-75.00`, category `Transfer` |
| `t4sav.qif` | `Savings` | the other side, `T75.00`, category `Transfer` |

The long values carry position markers: each 10-character block ends with its own position.
`PAYEE-0030` as the last visible block means 30 characters were kept.

### Questions to answer

1. Does MYM offer a credit-card account type? If not, which account types does it offer?
2. Is `t1ccard.qif` accepted into `Card`?
3. Does 31.31 show as a charge (amount owed goes up) and 200.00 as a payment (amount owed goes down)?
4. Only if 2 is no: is `t1bbank.qif` accepted into `Card`, and how do the two amounts show?
5. `t2long.qif`, payee: the last marker visible, and any extra characters after it.
6. `t2long.qif`, memo: the same.
7. `t2long.qif`, long category: kept whole, cut short (last marker), or refused?
8. `t2long.qif`, `testfood:testgroceries`: one category, a category and subcategory, or refused?
9. `t2long.qif`, `T2 PUNCT #*&/'.,-() TEST`: payee shown exactly?
10. Did MYM ask before creating new categories, or create them silently?
11. `t3three.qif`: all three imported? Categories kept in lower case? `Chase` balance up by 1,359.50?
12. `t3three.qif` imported a second time: warned, skipped, or three more entries added?
13. `t4chk.qif` and `t4sav.qif`: `Chase` down 75.00, `Savings` up 75.00, category `Transfer` accepted?
14. In a category report, does `Transfer` show 75.00 out and 75.00 in, or nets to 0, or something else?

### Results

Imported by Edwin on his PC, 2026-11-10. Details in
[docs/research/qif-compatibility.md](../../docs/research/qif-compatibility.md#round-2-2026-11-10).

| # | Answer |
|---|---|
| 1 | No credit-card type; MYM has a Charge type |
| 2 | Yes |
| 3 | Yes |
| 4 | Not needed |
| 5 | `PAYEE-0010PAYEE-0020PAYEE`: 25 characters |
| 6 | 40 characters of the memo kept; MYM also added `Full Name:` with the first 40 characters of the payee |
| 7 | Register shows `CATEG-0010CATEG` (15); category list shows `CATEG-0010CATEG-0020` (20, the column's width); MYM added `*Category Full Name*:` with the first 31 to the memo |
| 8 | One category, `testfood:testgr` |
| 9 | Yes |
| 10 | Created without asking |
| 11 | Yes, yes, yes |
| 12 | Warned |
| 13 | Yes |
| 14 | Both accounts show `Transfer` on the income side: `Chase` -$75, `Savings` $75 |
