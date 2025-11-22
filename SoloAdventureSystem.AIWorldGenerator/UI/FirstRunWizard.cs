using System;
using Terminal.Gui;

namespace SoloAdventureSystem.ContentGenerator.UI;

public static class FirstRunWizard
{
    /// <summary>
    /// Shows the first-run wizard. Returns true to continue, false to exit.
    /// Caller must have already called Application.Init()!
    /// </summary>
    public static bool ShowIfNeeded()
    {
        // Don't show wizard - just return true to proceed directly to main UI
        // User can select provider in the main UI
        return true;
    }
}
