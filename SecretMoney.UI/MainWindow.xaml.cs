// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic).
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using System.Collections.ObjectModel;
using System.Globalization;
using System.Text;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using SecretMoney.Export;
using SecretMoney.Ledger;
using Windows.Storage;
using Windows.Storage.Pickers;

namespace SecretMoney;

/// <summary>
/// The export window (README "Using SecretMoney"). It only connects controls to SecretMoney.Core:
/// finding the period, running hledger, the export rules and writing the files all live there.
/// </summary>
public sealed partial class MainWindow : Window
{
    // README §2: the one settings file, in the MSIX package data folder; uninstalling deletes it.
    private readonly string _settingsPath = Path.Combine(ApplicationData.Current.LocalFolder.Path, "settings.json");
    private readonly ObservableCollection<AccountRow> _accounts = [];
    private readonly ObservableCollection<CategoryRow> _categories = [];
    private ExportSettings _settings = ExportSettings.Empty;
    private IReadOnlyList<MymAccount> _found = [];
    private IReadOnlyList<LedgerTransaction> _generated = [];
    private IReadOnlyList<LedgerTransaction> _opening = [];
    private ExportPlan? _plan;

    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.SetIcon("Assets/AppIcon.ico");

        AccountList.ItemsSource = _accounts;
        CategoryList.ItemsSource = _categories;
        LoadSettings();
        ShowFolders();
    }

    private void LoadSettings()
    {
        if (!File.Exists(_settingsPath))
        {
            return;
        }

        try
        {
            _settings = SettingsFile.Read(File.ReadAllText(_settingsPath, Encoding.UTF8));
        }
        catch (Exception ex) when (ex is SettingsFormatException or IOException)
        {
            Say($"The settings couldn't be read, so SecretMoney starts with none: {ex.Message}");
        }
    }

    private void SaveSettings() => File.WriteAllText(_settingsPath, SettingsFile.Write(_settings));

    private void ShowFolders()
    {
        HledgerPathText.Text = _settings.HledgerPath ?? "Not chosen";
        LedgerFolderText.Text = _settings.LedgerFolder ?? "Not chosen";
        QifFolderText.Text = _settings.QifFolder ?? "Not chosen";
    }

    private void Say(string text) => ResultText.Text = text;

    private async void ChooseHledger_Click(object sender, RoutedEventArgs e)
    {
        var picker = new FileOpenPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
        picker.FileTypeFilter.Add(".exe");
        var file = await picker.PickSingleFileAsync();
        if (file is not null)
        {
            _settings = _settings with { HledgerPath = file.Path };
            SaveSettings();
            ShowFolders();
        }
    }

    private async void ChooseLedgerFolder_Click(object sender, RoutedEventArgs e)
    {
        if (await PickFolderAsync() is string folder)
        {
            _settings = _settings with { LedgerFolder = folder };
            SaveSettings();
            ShowFolders();
        }
    }

    private async void ChooseQifFolder_Click(object sender, RoutedEventArgs e)
    {
        if (await PickFolderAsync() is string folder)
        {
            _settings = _settings with { QifFolder = folder };
            SaveSettings();
            ShowFolders();
        }
    }

    private async Task<string?> PickFolderAsync()
    {
        var picker = new FolderPicker();
        WinRT.Interop.InitializeWithWindow.Initialize(picker, WinRT.Interop.WindowNative.GetWindowHandle(this));
        picker.FileTypeFilter.Add("*");
        var folder = await picker.PickSingleFolderAsync();
        return folder?.Path;
    }

    private async void Read_Click(object sender, RoutedEventArgs e)
    {
        if (_settings.HledgerPath is not string hledger || _settings.LedgerFolder is not string ledger)
        {
            Say("Choose where hledger.exe and the SecretBeans ledger folder are first.");
            return;
        }

        SetBusy(true);
        try
        {
            var period = SecretBeansPeriod.FindLatest(ledger)
                ?? throw new InvalidOperationException($"{ledger} has no SecretBeans period with journal\\main.journal.");
            PeriodText.Text = $"Period: {Path.GetFileName(period)}";

            var temp = ApplicationData.Current.TemporaryFolder.Path;
            await HledgerRunner.CheckAsync(hledger, SecretBeansPeriod.Journal(period, "main.journal"));
            _generated = HledgerCsvReader.Read(await HledgerRunner.PrintCsvAsync(hledger, SecretBeansPeriod.Journal(period, "generated.journal"), temp));
            _opening = HledgerCsvReader.Read(await HledgerRunner.PrintCsvAsync(hledger, SecretBeansPeriod.Journal(period, "opening.journal"), temp));
            _found = ExportRules.AccountsInGeneratedJournal(await File.ReadAllTextAsync(SecretBeansPeriod.Journal(period, "generated.journal")));

            ShowAccounts();
            ShowCategories();

            if (_generated.Count > 0)
            {
                var first = _generated.Min(t => t.Date);
                var last = _generated.Max(t => t.Date);
                StartDate.Date ??= ToOffset(first);
                EndDate.Date = ToOffset(last);
                Say($"Read {_generated.Count} transactions, {first:yyyy-MM-dd} to {last:yyyy-MM-dd}. hledger check passed. Choose the accounts, categories and dates, then Check.");
            }
            else
            {
                Say("The period has no transactions.");
            }
        }
        catch (Exception ex)
        {
            _generated = [];
            Say($"Couldn't read SecretBeans: {ex.Message}");
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void ShowAccounts()
    {
        _accounts.Clear();
        foreach (var choice in ExportRules.AccountChoices(_found, _settings.Accounts))
        {
            _accounts.Add(new AccountRow(choice));
        }
    }

    private void ShowCategories()
    {
        _categories.Clear();
        foreach (var choice in ExportRules.CategoryChoices(_generated, _settings with { Accounts = ChosenAccounts() }))
        {
            _categories.Add(new CategoryRow(choice));
        }
    }

    private MymAccount[] ChosenAccounts() => _accounts.Where(r => r.Chosen == true).Select(r => r.ToAccount()).ToArray();

    /// <summary>Takes the account and category rows into the settings and saves them.</summary>
    private void CollectSettings()
    {
        var categories = new Dictionary<string, string>(_settings.Categories, StringComparer.Ordinal);
        foreach (var row in _categories)
        {
            categories.Remove(row.HledgerAccount);
        }

        foreach (var (account, name) in ExportRules.ChosenCategories(_categories.Select(r => r.ToChoice())))
        {
            categories[account] = name;
        }

        _settings = _settings with { Accounts = ChosenAccounts(), Categories = categories };
        SaveSettings();
    }

    private void Check_Click(object sender, RoutedEventArgs e)
    {
        _plan = null;
        WriteButton.IsEnabled = false;
        if (_generated.Count == 0)
        {
            Say("Read SecretBeans first (step 2).");
            return;
        }

        try
        {
            CollectSettings();
            ShowCategories();

            if (EndDate.Date is not DateTimeOffset end)
            {
                Say("Choose the last date to export (step 5).");
                return;
            }

            var needsStart = _settings.Accounts.Any(a => a.LastExported is null);
            if (needsStart && StartDate.Date is null)
            {
                Say("Choose the date the first export starts on (step 5).");
                return;
            }

            var start = StartDate.Date is DateTimeOffset s ? DateOnly.FromDateTime(s.Date) : DateOnly.MinValue;
            _plan = ExportPlanner.Plan(_generated, _opening, _settings, start, DateOnly.FromDateTime(end.Date));
            Say(Describe(_plan));
            WriteButton.IsEnabled = _plan.CanExport && _settings.QifFolder is not null;
        }
        catch (Exception ex)
        {
            Say($"Couldn't check the export: {ex.Message}");
        }
    }

    private string Describe(ExportPlan plan)
    {
        var text = new StringBuilder();
        if (plan.Problems.Count > 0)
        {
            text.AppendLine($"Nothing can be written yet: {plan.Problems.Count} problem(s).");
            foreach (var p in plan.Problems)
            {
                var where = p.Date is DateOnly d ? $"{d:yyyy-MM-dd} {p.Description} ({p.Account}): " : p.Account.Length > 0 ? $"{p.Account}: " : "";
                text.AppendLine($"- {where}{p.Reason}");
            }

            return text.ToString();
        }

        foreach (var f in plan.Files)
        {
            if (f.From > f.To)
            {
                text.AppendLine($"{f.FileName,-13}already exported to {f.Account.LastExported:yyyy-MM-dd}   balance {Money(f.BalanceBefore)}");
                continue;
            }

            var entries = f.Entries.Count == 0 ? "no entries" : $"{f.Entries.Count} entries";
            text.AppendLine($"{f.FileName,-13}{entries,-12} {f.From:yyyy-MM-dd} to {f.To:yyyy-MM-dd}   balance before {Money(f.BalanceBefore)}, after {Money(f.BalanceAfter)}");
        }

        text.AppendLine();
        if (plan.Files.Any(f => f.Account.LastExported is null))
        {
            text.AppendLine("For an account never exported, create it in MYM with the balance before as its opening balance.");
        }

        text.AppendLine(!plan.CanExport ? "There are no entries to write."
            : _settings.QifFolder is null ? "Choose the QIF folder (step 1) to write the files."
            : $"Ready to write to {_settings.QifFolder}.");
        return text.ToString();
    }

    private async void Write_Click(object sender, RoutedEventArgs e)
    {
        if (_plan is not { CanExport: true } plan || _settings.QifFolder is not string folder)
        {
            return;
        }

        try
        {
            var existing = ExportFiles.Existing(plan, folder);
            if (existing.Count > 0)
            {
                var dialog = new ContentDialog
                {
                    Title = "Replace files?",
                    Content = $"{string.Join(", ", existing)} already in {folder}. Replace them?",
                    PrimaryButtonText = "Replace",
                    CloseButtonText = "Cancel",
                    DefaultButton = ContentDialogButton.Close,
                    XamlRoot = Content.XamlRoot,
                };
                if (await dialog.ShowAsync() != ContentDialogResult.Primary)
                {
                    return;
                }
            }

            var written = ExportFiles.Write(plan, folder);
            _settings = _settings.AfterExport(plan);
            SaveSettings();
            ShowAccounts();
            _plan = null;
            WriteButton.IsEnabled = false;

            var text = new StringBuilder($"Wrote {written.Count} file(s) to {folder}. In MYM, import each into its account:");
            text.AppendLine();
            foreach (var f in ExportFiles.ToWrite(plan))
            {
                text.AppendLine($"- {f.FileName}: {f.Entries.Count} entries ({f.Account.HledgerAccount}); MYM should then show {Money(f.BalanceAfter)}.");
            }

            Say(text.ToString());
        }
        catch (Exception ex)
        {
            Say($"Couldn't write the files: {ex.Message}");
        }
    }

    private void SetBusy(bool busy)
    {
        ReadButton.IsEnabled = !busy;
        CheckButton.IsEnabled = !busy;
        if (busy)
        {
            WriteButton.IsEnabled = false;
            Say("Running hledger…");
        }
    }

    private static DateTimeOffset ToOffset(DateOnly date) => new(date.ToDateTime(TimeOnly.MinValue));

    private static string Money(decimal amount) => amount.ToString("N2", CultureInfo.CurrentCulture);
}
