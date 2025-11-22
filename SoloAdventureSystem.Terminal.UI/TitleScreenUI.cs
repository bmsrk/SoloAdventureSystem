using Terminal.Gui;
using SoloAdventureSystem.UI.Themes;

namespace SoloAdventureSystem.UI;

/// <summary>
/// Beautiful title screen - simplified with single theme
/// </summary>
public class TitleScreenUI
{
    public Action? OnContinue { get; set; }

    public TitleScreenUI()
    {
    }

    public Window CreateWindow()
    {
        var win = ComponentFactory.CreateWindow("Solo Adventure System");
        win.X = 0;
        win.Y = 0;
        win.Width = Dim.Fill();
        win.Height = Dim.Fill();

        // ASCII Art Title
        var asciiArt = ComponentFactory.CreateTitle(@"
   _____ ____  __    ____     ___    ____  _    ____________   ________  ______  ______
  / ___// __ \/ /   / __ \   /   |  / __ \| |  / / ____/ __ \ /_  __/ / / / __ \/ ____/
  \__ \/ / / / /   / / / /  / /| | / / / /| | / / __/ / / / /  / / / / / / /_/ / __/   
 ___/ / /_/ / /___/ /_/ /  / ___ |/ /_/ / | |/ / /___/ /_/ /  / / / /_/ / _, _/ /___   
/____/\____/_____/\____/  /_/  |_/_____/  |___/_____/\____/  /_/  \____/_/ |_/_____/   
                                                                                         
           _____ __  ______________________  ___
          / ___/\ \/ / ___/_  __/ ____/  |/  /
          \__ \  \  /\__ \ / / / __/ / /|_/ / 
         ___/ /  / /___/ // / / /___/ /  / /  
        /____/  /_//____//_/ /_____/_/  /_/   
                                              
");
        asciiArt.X = Pos.Center();
        asciiArt.Y = 2;
        asciiArt.Height = 12;

        // Tagline
        var tagline = ComponentFactory.CreateAccentLabel("Explore AI-Generated Worlds - Create Epic Adventures - Play Solo RPGs");
        tagline.X = Pos.Center();
        tagline.Y = 14;
        tagline.TextAlignment = TextAlignment.Centered;

        // Version info
        var version = ComponentFactory.CreateMutedLabel("v1.0.0 | Built with .NET 10 & Terminal.Gui");
        version.X = Pos.Center();
        version.Y = 16;
        version.TextAlignment = TextAlignment.Centered;

        // Welcome message
        var welcomeFrame = ComponentFactory.CreateFrame("[ Welcome ]");
        welcomeFrame.X = Pos.Center();
        welcomeFrame.Y = 18;
        welcomeFrame.Width = 60;
        welcomeFrame.Height = 7;

        var welcomeText = ComponentFactory.CreateLabel("Generate procedural worlds powered by AI");
        welcomeText.X = Pos.Center();
        welcomeText.Y = 1;
        welcomeText.TextAlignment = TextAlignment.Centered;

        var welcomeText2 = ComponentFactory.CreateMutedLabel("Embark on solo tabletop RPG adventures");
        welcomeText2.X = Pos.Center();
        welcomeText2.Y = 2;
        welcomeText2.TextAlignment = TextAlignment.Centered;

        var continueBtn = ComponentFactory.CreatePrimaryButton("[ Press Enter to Continue ]");
        continueBtn.X = Pos.Center();
        continueBtn.Y = 4;
        continueBtn.Clicked += () =>
        {
            OnContinue?.Invoke();
        };

        welcomeFrame.Add(welcomeText, welcomeText2, continueBtn);

        // Footer
        var footer = ComponentFactory.CreateHintLabel("Press Enter to continue - Ctrl+C to quit");
        footer.X = Pos.Center();
        footer.Y = Pos.AnchorEnd(1);

        win.Add(asciiArt, tagline, version, welcomeFrame, footer);
        
        return win;
    }
}
