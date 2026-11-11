// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic), following
// RepositoryRulesTests in edwest19/SecretLead.
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Text.RegularExpressions;
using System.Xml.Linq;
using static SecretMoney.Core.Tests.TestHelper;

namespace SecretMoney.Core.Tests;

/// <summary>
/// Enforces the README's rules ("Safety" and "What this repository never contains"): Microsoft
/// packages only, no network API in the app's code, only the runFullTrust permission, no path
/// built under AppData, and no Managing Your Money files. If one of these fails, a change broke
/// a rule; fix the change, don't loosen the test without the owner's approval.
/// </summary>
[TestClass]
public sealed class RepositoryRulesTests
{
    // Every NuGet package the repo may use. All are published by Microsoft.
    private static readonly string[] AllowedPackages =
    [
        "Microsoft.WindowsAppSDK",                // WinUI 3 runtime for SecretMoney.UI
        "Microsoft.Windows.SDK.BuildTools",       // Windows SDK build tools for SecretMoney.UI (MSIX packaging)
        "Microsoft.Windows.SDK.BuildTools.WinApp", // `dotnet run` with package identity for SecretMoney.UI
        "MSTest",                                 // Microsoft's test framework (tests only)
    ];

    // The app's code. The test project is left out because it names these APIs itself.
    private static readonly string[] AppProjectFolders = ["SecretMoney.Core", "SecretMoney.UI"];

    // File types of Managing Your Money's own software, media and data (README "What this
    // repository never contains"). .ico is checked separately: SecretMoney has its own icon.
    private static readonly string[] MymFileTypes = [".img", ".ima", ".dsk", ".7z", ".zip", ".exe", ".chp", ".myc", ".hlp", ".pif", ".dbs"];

    [TestMethod]
    public void OnlyAllowedMicrosoftPackagesAreReferenced()
    {
        var root = RepoRoot();
        var used = new List<string>();

        foreach (var file in BuildFiles(root))
        {
            foreach (var reference in XDocument.Load(file).Descendants().Where(e => e.Name.LocalName == "PackageReference"))
            {
                var id = (string?)reference.Attribute("Include") ?? (string?)reference.Attribute("Update") ?? "";
                used.Add($"{Path.GetRelativePath(root, file)}: {id}");
                Assert.Contains(id, AllowedPackages, $"Package '{id}' in {Path.GetRelativePath(root, file)} is not on the allowed list.");
            }
        }

        Assert.IsNotEmpty(used, "Expected to find the project's package references.");
    }

    [TestMethod]
    public void AppCodeHasNoNetworkAccess()
    {
        // System.Net on its own is allowed (System.Net.WebUtility only encodes text). Its
        // namespaces that can reach a network are not.
        var network = new Regex(
            @"\bSystem\.Net\.(Http|Sockets|WebSockets|NetworkInformation|Mail|Quic|Security)\b" +
            @"|\bHttpClient\b|\bWebClient\b|\bWebRequest\b|\bSockets?\b|\bTcpClient\b|\bUdpClient\b|\bWebSocket\b" +
            @"|\bWebView2?\b|\bWindows\.Web\b|\bWindows\.Networking\b");
        var root = RepoRoot();
        var filesChecked = 0;

        foreach (var file in AppSourceFiles(root, ".cs", ".xaml"))
        {
            filesChecked++;
            var lines = File.ReadAllLines(file);
            for (var i = 0; i < lines.Length; i++)
            {
                Assert.IsFalse(network.IsMatch(lines[i]),
                    $"Network API found in {Path.GetRelativePath(root, file)} line {i + 1}: {lines[i].Trim()}");
            }
        }

        Assert.IsGreaterThan(0, filesChecked, "Expected to scan the app's source files.");
    }

    [TestMethod]
    public void AppCode_StartsNoProgramButHledger()
    {
        // README "Safety": the only other program the app runs is the hledger.exe the user chose, and
        // only HledgerRunner runs it, with `check` and `print` (docs/secretbeans-input.md, "Running hledger").
        var starts = new Regex(@"\bProcess(StartInfo)?\b|\bSystem\.Diagnostics\b|\bLauncher\b|\bShellExecute\b|\bCreateProcess\b");
        var root = RepoRoot();

        var found = AppSourceFiles(root, ".cs", ".xaml")
            .Where(file => starts.IsMatch(File.ReadAllText(file)))
            .Select(file => Path.GetRelativePath(root, file).Replace('\\', '/'))
            .ToArray();

        CollectionAssert.AreEqual(new[] { "SecretMoney.Core/Ledger/HledgerRunner.cs" }, found);
    }

    [TestMethod]
    public void AppManifest_DeclaresOnlyRunFullTrust()
    {
        // No network, device or AI capabilities. runFullTrust is required for a WinUI 3 desktop app
        // packaged as full trust (README "Safety").
        var capabilities = XDocument.Parse(ReadRepoFile("SecretMoney.UI", "Package.appxmanifest")).Descendants()
            .Where(e => e.Name.LocalName is "Capability" or "DeviceCapability")
            .Select(e => (string?)e.Attribute("Name") ?? "")
            .ToArray();

        CollectionAssert.AreEqual(new[] { "runFullTrust" }, capabilities);
    }

    [TestMethod]
    public void AppCode_BuildsNoPathUnderAppDataItself()
    {
        // README "What SecretMoney stores": anything the app keeps goes in its MSIX package data
        // folder (ApplicationData.Current.LocalFolder), never in a path it builds itself.
        var root = RepoRoot();
        var ownPath = new Regex(@"\bLocalApplicationData\b|\bApplicationData\.Roaming\b|\bGetFolderPath\b|%LOCALAPPDATA%|\bLOCALAPPDATA\b|%APPDATA%");

        foreach (var file in AppSourceFiles(root, ".cs"))
        {
            var code = string.Join("\n", File.ReadAllLines(file).Where(line => !line.TrimStart().StartsWith("//", StringComparison.Ordinal)));
            Assert.IsFalse(ownPath.IsMatch(code), $"{Path.GetRelativePath(root, file)} builds a path under AppData itself.");
        }
    }

    [TestMethod]
    public void Repository_ContainsNoManagingYourMoneyFiles()
    {
        // README "What this repository never contains": no MYM program files, disk images,
        // archives or databases. Build output folders are skipped.
        var root = RepoRoot();
        var filesChecked = 0;

        foreach (var file in Directory.EnumerateFiles(root, "*", SearchOption.AllDirectories).Where(f => !IsBuildOutput(root, f)))
        {
            filesChecked++;
            var relative = Path.GetRelativePath(root, file).Replace('\\', '/');
            var extension = Path.GetExtension(file).ToLowerInvariant();

            Assert.DoesNotContain(extension, MymFileTypes, $"{relative} has a file type the repository must never contain.");
            if (extension == ".ico")
            {
                Assert.AreEqual("SecretMoney.UI/Assets/AppIcon.ico", relative, "The only .ico file allowed is SecretMoney's own app icon.");
            }
        }

        Assert.IsGreaterThan(0, filesChecked, "Expected to scan the repository's files.");
    }

    private static IEnumerable<string> AppSourceFiles(string root, params string[] extensions) =>
        AppProjectFolders
            .SelectMany(folder => Directory.EnumerateFiles(Path.Combine(root, folder), "*.*", SearchOption.AllDirectories))
            .Where(f => extensions.Any(e => f.EndsWith(e, StringComparison.Ordinal)))
            .Where(f => !IsBuildOutput(root, f));

    private static IEnumerable<string> BuildFiles(string root) =>
        Directory.EnumerateFiles(root, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".csproj", StringComparison.Ordinal) || f.EndsWith(".props", StringComparison.Ordinal) || f.EndsWith(".targets", StringComparison.Ordinal))
            .Where(f => !IsBuildOutput(root, f));
}
