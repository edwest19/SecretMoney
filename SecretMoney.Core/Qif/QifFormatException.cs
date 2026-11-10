// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
namespace SecretMoney.Qif;

/// <summary>Thrown when an entry can't be written as docs/qif-format.md specifies. The message names the entry and the rule.</summary>
public sealed class QifFormatException(string message) : Exception(message);
