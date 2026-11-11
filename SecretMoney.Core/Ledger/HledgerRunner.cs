// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Diagnostics;
using System.Text;

namespace SecretMoney.Ledger;

/// <summary>Thrown when hledger can't be run or reports an error. The message includes what hledger said.</summary>
public sealed class HledgerException(string message) : Exception(message);

/// <summary>
/// Runs the hledger the user chose (docs/secretbeans-input.md, "What hledger is asked to do"). Each
/// argument is passed separately, so paths with spaces need no quoting. hledger writes its CSV to a
/// file with <c>-o</c>, as it did when its output was recorded on Edwin's PC; the file is read as UTF-8
/// and deleted.
/// </summary>
public static class HledgerRunner
{
    /// <summary>The arguments for <c>hledger check</c> on a journal.</summary>
    public static IReadOnlyList<string> CheckArguments(string journal) => ["check", "-f", journal];

    /// <summary>The arguments for printing a journal as CSV into <paramref name="output"/>.</summary>
    public static IReadOnlyList<string> PrintArguments(string journal, string output) => ["print", "-x", "-O", "csv", "-f", journal, "-o", output];

    /// <summary>Runs <c>hledger check</c>; throws <see cref="HledgerException"/> with hledger's message if the journal fails it.</summary>
    public static async Task CheckAsync(string hledgerExe, string journal, CancellationToken cancellationToken = default)
    {
        await RunAsync(hledgerExe, CheckArguments(journal), cancellationToken);
    }

    /// <summary>
    /// Prints <paramref name="journal"/> as CSV, through a file in <paramref name="tempFolder"/> that is
    /// deleted afterwards, and returns the CSV text.
    /// </summary>
    public static async Task<string> PrintCsvAsync(string hledgerExe, string journal, string tempFolder, CancellationToken cancellationToken = default)
    {
        var output = Path.Combine(tempFolder, $"secretmoney-{Guid.NewGuid():N}.csv");
        try
        {
            await RunAsync(hledgerExe, PrintArguments(journal, output), cancellationToken);
            if (!File.Exists(output))
            {
                throw new HledgerException($"hledger finished but wrote no CSV for {journal}.");
            }

            return await File.ReadAllTextAsync(output, Encoding.UTF8, cancellationToken);
        }
        finally
        {
            if (File.Exists(output))
            {
                File.Delete(output);
            }
        }
    }

    private static async Task RunAsync(string hledgerExe, IReadOnlyList<string> arguments, CancellationToken cancellationToken)
    {
        if (!File.Exists(hledgerExe))
        {
            throw new HledgerException($"hledger.exe isn't at {hledgerExe}. Choose where it is.");
        }

        var start = new ProcessStartInfo(hledgerExe)
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };
        foreach (var argument in arguments)
        {
            start.ArgumentList.Add(argument);
        }

        using var process = Process.Start(start) ?? throw new HledgerException($"{hledgerExe} didn't start.");
        var output = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var error = process.StandardError.ReadToEndAsync(cancellationToken);
        await process.WaitForExitAsync(cancellationToken);
        var said = ((await error).Trim() + Environment.NewLine + (await output).Trim()).Trim();

        if (process.ExitCode != 0)
        {
            throw new HledgerException($"hledger {arguments[0]} failed (exit code {process.ExitCode}): {said}");
        }
    }
}
