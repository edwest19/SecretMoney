# GitHub Copilot Instructions for SecretMoney

## Project Purpose

SecretMoney is an independently developed preservation, installation,
interoperability, research, and financial-data conversion project for
Managing Your Money 12 for DOS.

The project is designed primarily for Windows.

## Copyright Boundary

Never add, reproduce, commit, or recommend committing original Managing Your
Money copyrighted files to this repository.

This includes, but is not limited to:

- *.img original installation disks
- original installation archives
- MYM.EXE
- original executable files
- *.CHP files
- *.MYC files
- original help files
- original manuals
- original artwork
- original installer resources

The user supplies the original software separately.

SecretMoney may document filenames, observed file structures, installation
procedures, interoperability behavior, and independently discovered technical
information.

## Personal Financial Data

Never commit actual personal financial data.

Do not commit live MYM database files.

Do not commit:

- live *.DBS files
- bank statements
- account numbers
- credentials
- secrets
- tokens
- personally identifying financial records

Tests must use clearly synthetic financial data.

Examples:

    TESTPAYEE
    TESTMEMO
    12.34

## Licensing

New SecretMoney source code and original SecretMoney documentation should be
compatible with the MIT License.

Do not represent Managing Your Money itself as MIT licensed.

Do not copy third-party source code into the repository unless its license has
been checked and the inclusion is intentional.

## Platform

Prefer:

- Windows
- PowerShell
- DOSBox-X
- Git
- GitHub
- GitHub Actions where useful

Commands intended for the user should normally be directly pasteable into
PowerShell.

Do not substitute Bash instructions when the task specifically concerns the
Windows workflow.

## Working Directories

The live MYM installation is separate from this repository.

Typical runtime layout:

    C:\Users\<user>\OneDrive\MYM\DriveC
    C:\Users\<user>\OneDrive\MYM\DriveD

SecretMoney must not assume that runtime data is stored inside the repository.

Paths should be configurable rather than permanently hard-coded to one user's
Windows profile whenever practical.

## DOSBox-X

SecretMoney may automate and document DOSBox-X configuration.

The intended conceptual mapping is:

    DOS C: -> MYM DriveC
    DOS D: -> MYM DriveD

Do not mount a user's entire Windows system drive when a dedicated folder will
suffice.

## QIF

QIF is currently the preferred interchange format for importing normalized
financial transactions into MYM 12.

Observed MYM 12 QIF compatibility must be distinguished from general QIF
behavior.

Currently tested successfully:

- date
- payment
- deposit
- payee
- memo
- category
- check number
- split transactions

Cleared/reconciled status has not yet been demonstrated to map visibly as
expected.

The tested QIF account-transfer approach did not reproduce MYM's native linked
transfer behavior.

Do not mark an untested feature as supported.

## Transfers

SecretBeans already performs important transfer and card-payment detection.

Do not independently reinvent that logic inside SecretMoney unless the project
requirements explicitly change.

SecretMoney should preserve transfer metadata supplied by the normalized
SecretForecast data model.

MYM has a native Transfer Money operation that creates corresponding account
entries. QIF transfer compatibility requires further research before an
automatic implementation is considered reliable.

## Financial Calculations

Use decimal-safe financial arithmetic in new code.

Do not use binary floating-point arithmetic for new monetary calculations
merely because the legacy MYM database has been observed using floating-point
representations internally.

## Reverse Engineering

Reverse engineering in this project is for interoperability, preservation,
data migration, documentation, and understanding user-owned data.

Prefer behavioral observation and controlled test data.

Example workflow:

1. Establish a known baseline.
2. Perform exactly one controlled operation.
3. Save and exit MYM.
4. Compare before and after.
5. Document observed changes.
6. Repeat with another controlled variable.

Clearly distinguish:

- verified findings
- hypotheses
- untested behavior

Do not convert a hypothesis into documentation stated as fact.

## Tests

Use deterministic synthetic test records.

Prefer conspicuous identifiers such as:

    TESTPAYEE123
    TESTMEMO456
    CHECKNUMBER-TEST
    SPLIT-TEST
    TRANSFER-TEST

Tests should be independently reproducible.

Keep regression fixtures small.

## Documentation

Documentation is an important project deliverable, not an afterthought.

Commands should be explicit.

Installation instructions should be reproducible from a clean system whenever
possible.

Research documents should record:

- what was tested
- test input
- expected behavior
- observed behavior
- result
- remaining questions

Screenshots may be referenced when they materially document observed behavior.

## Changes

Prefer small, reviewable changes.

Do not perform broad architectural rewrites merely because a different design
appears preferable.

Preserve working behavior unless a change has a clearly documented purpose.

For substantial work:

1. Create or reference a GitHub issue.
2. Implement one coherent change.
3. Add or update tests.
4. Update documentation.
5. Create a pull request.
6. Explain what was verified.

## AI-Assisted Project

SecretMoney is intentionally developed with AI assistance.

Copilot may:

- analyze the repository
- propose implementations
- generate code
- create tests
- improve documentation
- review pull requests
- identify inconsistencies

Copilot must not fabricate test results.

If MYM behavior has not actually been tested, state that it is unverified.

Generated comments should be useful and proportional. Do not add repetitive
"generated by AI" comments throughout source code.

A brief file-level or documentation-level acknowledgment of Copilot assistance
is sufficient where appropriate.

## Primary Rule

Protect the boundary between:

1. independently developed SecretMoney material, and
2. original Managing Your Money software and private financial data.

When uncertain, keep the questionable material out of the repository and
document what the user must supply separately.