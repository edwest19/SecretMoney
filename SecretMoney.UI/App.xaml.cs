// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic), following
// SecretLead.UI/App.xaml.cs in edwest19/SecretLead.
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using Microsoft.UI.Xaml;

namespace SecretMoney;

/// <summary>The application: opens the one window.</summary>
public partial class App : Application
{
    /// <summary>The main window, for anything that needs it (file pickers, dialogs).</summary>
    public static Window Window { get; private set; } = null!;

    public App()
    {
        InitializeComponent();
    }

    protected override void OnLaunched(Microsoft.UI.Xaml.LaunchActivatedEventArgs args)
    {
        Window = new MainWindow();
        Window.Activate();
    }
}
