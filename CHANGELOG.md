# Changelog

> AI-generated: this file was written by Claude (Anthropic). SecretMoney is a project built with AI-generated code; see [README.md](README.md).

Every release has a heading `## [X.Y.Z] - YYYY-MM-DD`; the release workflow refuses a tag without one and uses the entry as the release notes. The date on the entry below is updated when 0.1.0 is released.

## [0.1.0] - 2026-11-10

Not released yet.

### New
- **The QIF writer.** `docs/qif-format.md` (version 1) specifies exactly what SecretMoney writes, using only what MYM 12 was seen to import: ASCII with CR LF, `!Type:Bank` or `!Type:CCard`, and per entry a date, an amount, a payee (at most 25 characters), an optional memo (40) and a category (20, no colon). `QifWriter` in `SecretMoney.Core` writes it and refuses entries that break a rule; `QifText` cleans any text to printable ASCII. `QifWriterTests` reproduce the round-2 files MYM imported byte for byte. Nothing in the app uses the writer yet.
- **The SecretMoney app**, set up from SecretLead's layout: `SecretMoney.Core` (plain .NET, no Windows dependencies), `SecretMoney.Core.Tests` (MSTest) and `SecretMoney.UI` (WinUI 3, packaged as an MSIX, x64 only). The window only says that exporting isn't built yet; it reads and writes no files.
- **Rule tests.** `RepositoryRulesTests` checks Microsoft packages only, no network API in the app's code, `runFullTrust` as the only permission, no path built under AppData, and no Managing Your Money file types in the repository. `ReleaseRulesTests` checks the version in three places, this changelog, the signing subject, pinned workflow actions and x64. `QifFixtureTests` keeps the QIF test files in `tests/qif` ASCII with CRLF line endings.
- **Build and release workflows**, SecretLead's with SecretMoney's names. The Build workflow runs the tests, builds the app and packages an unsigned MSIX on every pull request, checked by `scripts/Test-Msix.ps1`. The release workflow starts only on a `v*` tag.
- `README.md` is now the spec, and `CLAUDE.md` tells an AI how to work in and evaluate the repository. The MYM 12 installation guide, research, scripts and QIF test files are unchanged.
- Round-2 QIF research with MYM 12 (pull request #1): charge accounts, field lengths, duplicate warnings and transfers, recorded in `docs/research/qif-compatibility.md`.
