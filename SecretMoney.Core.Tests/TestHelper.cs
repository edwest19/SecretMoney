// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic), following
// TestHelper in edwest19/SecretLead.
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
namespace SecretMoney.Core.Tests;

/// <summary>Shared test helpers.</summary>
internal static class TestHelper
{
    /// <summary>Finds the repository root (the folder containing SecretMoney.slnx) from the test's output folder.</summary>
    public static string RepoRoot()
    {
        for (var dir = new DirectoryInfo(AppContext.BaseDirectory); dir != null; dir = dir.Parent)
        {
            if (File.Exists(Path.Combine(dir.FullName, "SecretMoney.slnx")))
            {
                return dir.FullName;
            }
        }

        throw new InvalidOperationException("Could not find SecretMoney.slnx above " + AppContext.BaseDirectory);
    }

    /// <summary>Reads a file in the repository, given its path parts from the root.</summary>
    public static string ReadRepoFile(params string[] path) =>
        File.ReadAllText(Path.Combine([RepoRoot(), .. path]));

    /// <summary>True for paths inside build output or tool folders, which the rule tests skip.</summary>
    public static bool IsBuildOutput(string root, string file)
    {
        var parts = Path.GetRelativePath(root, file).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        return parts.Any(p => p is "bin" or "obj" or ".git" or ".vs" or "TestResults" or "artifacts");
    }
}

/// <summary>A private temporary folder for one test, deleted afterwards.</summary>
internal sealed class TempFolder : IDisposable
{
    public TempFolder()
    {
        FullPath = Path.Combine(Path.GetTempPath(), "secretmoney-tests-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(FullPath);
    }

    public string FullPath { get; }

    /// <summary>The path of a file or folder named <paramref name="name"/> in this folder.</summary>
    public string PathOf(params string[] name) => Path.Combine([FullPath, .. name]);

    public void Dispose()
    {
        try
        {
            Directory.Delete(FullPath, recursive: true);
        }
        catch (IOException)
        {
            // A leftover temp folder is harmless.
        }
    }
}
