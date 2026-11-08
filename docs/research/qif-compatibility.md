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
