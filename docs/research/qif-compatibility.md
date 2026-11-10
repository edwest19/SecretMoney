# MYM 12 QIF Compatibility Research

## Purpose

This document records QIF import behavior directly tested with Managing Your Money 12 for DOS during SecretMoney development.

The goal is to determine which QIF features can reliably serve as the interchange format between SecretBeans / SecretForecast and MYM 12.

Results in this document describe observed MYM 12 behavior rather than assumptions based on other versions of Quicken or other financial software.

## Current Direction

Based on testing so far, QIF is the preferred interchange format for ordinary banking transactions.

Planned architecture:

    Bank CSV
        |
        v
    SecretBeans
        |
        v
    SecretForecast
        |
        v
    SecretMoney QIF Export
        |
        v
    Managing Your Money 12

SecretBeans remains responsible for its existing normalization and duplicate-transfer/card-payment detection logic.

## QIF Files

The test files were plain ASCII QIF files.

A basic tested banking transaction followed this general form:

    !Type:Bank
    D10/24/2026
    T-12.34
    PTESTPAYEE-QIF
    MTESTMEMO-QIF
    LTest Category
    ^

The test QIF files were placed on the DOSBox-X D: drive and imported through MYM's QIF import function.

## Compatibility Summary

| Feature | Result |
|---|---|
| Banking QIF | PASS |
| Date | PASS |
| Payment / negative amount | PASS |
| Deposit / positive amount | PASS |
| Payee | PASS |
| Memo | PASS |
| Category | PASS |
| Check number | PASS |
| Split transaction | PASS |
| Cleared status using C* | NOT visibly mapped |
| Linked account transfer using tested L[Account] method | FAILED |
| MYM native account transfer | PASS |
<!-- AI-generated (Claude): rows below added for round 2, 2026-11-10 -->
| Charge account using `!Type:CCard` (round 2) | PASS |
| Payee length (round 2) | 25 characters; up to 40 copied into the memo as `Full Name:` |
| Category length (round 2) | At least 20 kept (15 shown in the register); up to 31 copied into the memo as `*Category Full Name*:` |
| Colon in a category (round 2) | One category name, not a subcategory |
| Punctuation in a payee (round 2) | PASS |
| New categories (round 2) | Created without asking |
| Several entries in one file, lower-case categories (round 2) | PASS |
| Same file imported twice (round 2) | MYM warned |
| Transfer as an ordinary entry in both accounts, category `Transfer` (round 2) | PASS for balances; not linked |
| Memo length (round 2) | 40 characters of the QIF `M` field kept |

These results apply only to the specific behavior tested.

## Basic Payment Test

Test input included:

    !Type:Bank
    D10/24/2026
    T-12.34
    PTESTPAYEE-QIF
    MTESTMEMO-QIF
    LTest Category
    ^

MYM successfully imported the transaction as a payment.

Observed mappings included:

    D -> transaction date
    T negative -> payment
    P -> payee
    M -> memo
    L -> category

The imported amount was:

    12.34

## Basic Deposit Test

A positive test transaction used:

    T123.45

MYM imported the transaction as a deposit.

The test payee was:

    TESTDEPOSIT-QIF

The test memo was:

    Test QIF deposit

The observed register arithmetic was consistent with the imported payment and deposit amounts.

## Check Number Test

A separate test used:

    N1234

with:

    PCHECKNUMBER-TEST

MYM imported:

    1234

into the transaction number/check-number field.

Result:

    PASS

## Cleared Status Test

A test transaction included:

    C*

The imported transaction was visible in the MYM register, but the test did not produce a visible checkmark in MYM's cleared-status column.

Current result:

    NOT visibly mapped

This does not prove that MYM ignores all QIF cleared-status values.

It records only that the tested `C*` value did not produce the expected visible cleared indicator.

## Split Transaction Test

A test transaction used:

    !Type:Bank
    D10/24/2026
    T-100.00
    PSPLIT-TEST
    MTesting QIF split transaction
    SUtilities
    ESplit part one
    $60.00
    SGroceries
    ESplit part two
    $40.00
    ^

MYM imported the transaction.

The register identified the transaction as a split.

The transaction detail displayed the two tested categories:

    Utilities
    Groceries

and the corresponding split amounts.

Result:

    PASS

Further testing should verify MYM's exact sign convention for more complex split combinations before production conversion logic is finalized.

## Linked Transfer Test

A second MYM bank account named:

    Savings

was created.

A transfer test used:

    !Type:Bank
    !Option:AllXfr
    D10/24/2026
    T-50.00
    PTRANSFER-TEST
    MTesting transfer from Chase to Savings
    L[Savings]
    ^

This test did not reproduce MYM's native linked-transfer behavior.

The expected linked transfer did not appear as the native corresponding pair of account transactions.

Current result:

    FAILED

Do not generate production MYM transfers using this tested QIF representation unless later research demonstrates the correct MYM 12 behavior.

## Native MYM Transfer

MYM's own Transfer Money screen was tested separately.

A native transfer of:

    77.77

was entered from:

    Chase

to:

    Savings

MYM created corresponding transaction activity for both accounts.

The observed exported transaction summary included:

    Receive, Chase, 77.77
    Spend, Savings, 77.77

This demonstrates that MYM itself supports linked account-to-account transfer behavior even though the tested QIF transfer encoding did not reproduce it.

## Transfer Design Consequence

SecretMoney must distinguish between:

1. MYM native transfer capability.
2. QIF transfer behavior actually accepted by MYM 12.

These are not currently demonstrated to be equivalent.

SecretBeans already performs transfer/card-payment matching, so SecretMoney should preserve that information rather than independently attempting to rediscover matched transfers.

Possible future approaches include:

- further QIF transfer compatibility research
- per-account QIF generation
- investigation of MYM's Quicken Banking Data import
- user-assisted conversion of matched transfers
- a future interoperability mechanism if safely demonstrated

No approach should be marked supported until tested.

<!-- AI-generated (Claude): this whole section was added for round 2, 2026-11-10 -->
## Round 2 (2026-11-10)

Round 2 tested what the SecretBeans-to-QIF exporter depends on. The test files and the
questions asked are in `tests\qif\` and `tests\qif\README.md`. Edwin imported them into a
throwaway MYM 12 data set on his PC, into accounts `Chase` and `Savings` (from round 1)
and a new account `Card`. His answers are recorded below as given; character counts were
computed from the text he reported.

### Charge account (`t1ccard.qif`)

    !Type:CCard
    D11/02/2026
    T-31.31
    PT1 CARD CHARGE TEST
    ...
    D11/05/2026
    T200.00
    PT1 CARD PAYMENT TEST

- Credit-card account type: answer "No, Charge". MYM 12 has no type named credit card; it has
  a Charge type.
- `!Type:CCard` imported into `Card`.
- `T-31.31` showed as a charge (amount owed went up); `T200.00` as a payment (amount owed went
  down).
- `t1bbank.qif` (the `!Type:Bank` fallback) was not needed and not tested.

Result: PASS.

### Field lengths (`t2long.qif`)

The long values carry position markers: each 10-character block ends with its own position.

| Field | Written | Kept by MYM | Characters |
|---|---|---|---|
| Payee | `PAYEE-0010` ... `PAYEE-0080` (80) | `PAYEE-0010PAYEE-0020PAYEE` | 25 |
| Memo | `MEMO--0010` ... `MEMO--0080` (80) | `MEMO--0010MEMO--0020MEMO--0030MEMO--0040` | 40 |
| Category | `CATEG-0010` ... `CATEG-0040` (40) | `CATEG-0010CATEG-0020` in the category list; `CATEG-0010CATEG` in the register | at least 20 |
| Category with colon | `testfood:testgroceries` | one category; `testfood:testgr` in the register | 15 shown in the register |

A re-check the same day, from Edwin's screenshots of the entries, showed what
MYM does with the part that does not fit:

- **Long payee.** The memo of the -1.01 entry, opened with F2 (View/Edit Memo), holds two lines:

      Full Name: PAYEE-0010PAYEE-0020PAYEE-0030PAYEE-0040
      MEMO--0010MEMO--0020MEMO--0030MEMO--0040

  MYM added a `Full Name:` line holding the first 40 characters of the payee, and kept the
  first 40 characters of the QIF `M` field on the next line. The memo editor's title,
  `Memo for: PAYEE-0010PAYEE-0020PAYEE`, has room for more and still shows 25 characters.
- **Long category.** The register shows the -2.02 entry's category as `CATEG-0010CATEG`
  (15 characters). Its memo holds:

      *Category Full Name*:
           CATEG-0010CATEG-0020CATEG-0030C

  MYM added a `*Category Full Name*:` line holding the first 31 characters of the category.
- **The category is not cut to 15.** A second screenshot, View or Edit Category Allocation for
  the same entry, lists it as `CATEG-0010CATEG-0020` (20 characters). That list's category
  column is 20 characters wide, as wide as `CATEG-0010CATEG-0020`, so MYM keeps at least 20
  characters and the register's 15 is a display limit (Edwin: "I do not think it chopped off
  category. only displays 15."). Whether MYM keeps more than 20 has not been established.
  MYM's own categories in that list include `Compuserve/Prodigy` (18 characters).
- The memo field on the transaction screen shows only the start of the memo's first line. The
  first answer to the memo question, `PAYEE-0010PAYEE-0020PAYEE-0030PAYEE-004`, matches the
  end of the 50 characters visible there: `Full Name: ` plus 39 characters.
- Not checked: whether MYM adds these memo lines when the payee or category fits, and what it
  did with the memo of the colon category entry.

A colon is not a subcategory separator in MYM 12: `testfood:testgroceries` became one
category, shown in the register as `testfood:testgr`, the same 15 characters as the long
category.
- `T2 PUNCT #*&/'.,-() TEST` showed exactly as written.
- New categories were created without asking.

### Several entries, and importing twice (`t3three.qif`)

- All three entries imported. Categories `groceries`, `utilities` and `salary` stayed in lower
  case. `Chase` went up by 1,359.50 (-52.40 - 88.10 + 1,500.00).
- Importing the same file a second time: MYM warned. What happens if the import goes ahead
  anyway was not recorded.

### Transfer as an ordinary entry (`t4chk.qif`, `t4sav.qif`)

    t4chk.qif into Chase:   T-75.00  LTransfer
    t4sav.qif into Savings: T75.00   LTransfer

- `Chase` went down 75.00 and `Savings` up 75.00. Category `Transfer` was accepted.
- Category report: `Chase` shows `Transfer` on the income side as -$75; `Savings` shows
  `Transfer` on the income side as $75. Across the two accounts that nets to 0.
- MYM does not link the two entries; they are two independent register entries. Why MYM put
  `Transfer` on the income side has not been established.

Result: PASS for balances. The entries are not linked as MYM's own Transfer Money screen
links them.

### Consequences for the exporter

- Charge accounts are written as `!Type:CCard`, with the card's own amount: a charge negative,
  a payment positive. That is the sign the card account's posting already has in hledger.
- The exporter cuts payees to 25 characters and categories to 20 itself, so every character
  it writes is one MYM has been seen to keep, and it reports any two hledger categories that
  become the same 20-character name. The register shows the first 15.
- When a bank description is longer than 25 characters, the exporter writes its first 40
  characters as the memo, so the longer text MYM would otherwise copy there is kept.
- Category names carry no colon.
- Transfers and card payments are written as an ordinary entry in both accounts, category
  `Transfer`.
- MYM warns when the same file is imported twice; the exporter still exports only dates after
  the previous export.

## Account-Oriented QIF

A future test should investigate whether MYM supports multi-account QIF structures.

If not, a straightforward SecretMoney export design is:

    Chase.qif
    Savings.qif
    CreditCard.qif

with each file imported into its corresponding MYM account.

This remains a proposed design rather than a verified requirement.

## Investment QIF

MYM exposes a separate Quicken Investment Data import option, and QIF itself also supports investment transaction types.

SecretMoney has not yet completed controlled investment-import compatibility testing.

Investment support should therefore remain:

    UNVERIFIED

Planned test cases should include:

- Buy
- Sell
- Dividend
- Reinvested dividend
- quantity
- price
- commissions
- security identifiers

## Synthetic Test Data

SecretMoney QIF regression fixtures should contain only synthetic data.

Examples used during research included:

    TESTPAYEE-QIF
    TESTDEPOSIT-QIF
    CHECKNUMBER-TEST
    CLEARED-TEST
    SPLIT-TEST
    TRANSFER-TEST

No real financial records should be committed as tests.

## Planned Test Directory

Synthetic QIF fixtures belong under:

    tests\qif\

Potential fixtures include:

    banking.qif
    check-number.qif
    cleared.qif
    split.qif
    transfer.qif

A failed compatibility test may still be useful as a regression/research fixture as long as its expected result is documented clearly.

## Implementation Principle

SecretMoney should generate QIF from a normalized internal transaction representation rather than directly translating arbitrary bank CSV columns.

Preferred structure:

    Source CSV
        |
        v
    SecretBeans normalization
        |
        v
    Canonical transaction
        |
        +--> hledger
        |
        +--> QIF
        |
        +--> future formats

This keeps bank-specific parsing separate from MYM-specific export behavior.

## Safety

Never test importer development against the only copy of real financial data.

Use a disposable MYM data set until the resulting QIF behavior is verified.

Retain backups before importing large transaction sets.

## Status

QIF is currently considered suitable for continued SecretMoney banking-import development because MYM 12 successfully imported the primary transaction fields and split transactions tested so far.

Transfers and investments require additional research.

## AI-Assisted Research

These compatibility findings were developed from hands-on MYM 12 testing performed with assistance from Microsoft Copilot.

Only observed behavior is marked as verified.
