// ============================================================================
// Copyright (c) 2026 Edwin West. Licensed under the MIT License (see LICENSE).
// AI-GENERATED CODE: This file was written by Claude (Anthropic), following
// SecretLead.UI/MainWindow.xaml.cs in edwest19/SecretLead.
// SecretMoney is a project built with AI-generated code; see README.md.
// ============================================================================
using Microsoft.UI.Xaml;

namespace SecretMoney;

/// <summary>The application window. A placeholder until the export window is built.</summary>
public sealed partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);

        AppWindow.SetIcon("Assets/AppIcon.ico");
    }
}
