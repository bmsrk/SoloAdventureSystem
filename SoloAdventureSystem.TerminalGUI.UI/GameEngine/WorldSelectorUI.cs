using Terminal.Gui;

namespace SoloAdventureSystem.TerminalGUI.GameEngine;

/// <summary>
/// World selection menu - loads worlds from content folder
/// </summary>
public class WorldSelectorUI
{
    private readonly string _worldsPath;

    public WorldSelectorUI(string worldsPath)
    {
        _worldsPath = worldsPath;
    }

    public string? SelectWorld()
    {
        Application.Init();
        var top = Application.Top;

        string? selectedWorld = null;

        // Cyberpunk colors
        var cyberCyan = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Cyan, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Cyan),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan)
        };

        var cyberMagenta = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta)
        };

        Colors.Base = cyberCyan;

        var win = new Window("? SELECT WORLD ?")
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = cyberCyan
        };

        // ASCII Art Header
        var header = new Label(@"
?????????????????????????????????????????????????????????????????????????
?  ???????? ??????? ???      ???????      ?????? ??????? ???   ???     ?
?  ????????????????????     ?????????    ???????????????????   ???     ?
?  ???????????   ??????     ???   ???    ???????????  ??????   ???     ?
?  ???????????   ??????     ???   ???    ???????????  ??????? ????     ?
?  ??????????????????????????????????    ???  ??????????? ???????      ?
?  ???????? ??????? ???????? ???????     ???  ??????????   ?????       ?
?                     ? WORLD SELECTOR ?                                ?
?????????????????????????????????????????????????????????????????????????")
        {
            X = Pos.Center(),
            Y = 0,
            ColorScheme = cyberMagenta,
            TextAlignment = TextAlignment.Centered
        };
        win.Add(header);

        // Show search path for debugging
        var pathLabel = new Label($"Searching: {_worldsPath}")
        {
            X = 1,
            Y = 9,
            ColorScheme = cyberCyan
        };
        win.Add(pathLabel);

        var infoLabel = new Label(">>> SELECT A WORLD TO EXPLORE:")
        {
            X = 1,
            Y = 10,
            ColorScheme = cyberCyan
        };
        win.Add(infoLabel);

        // Find all world files
        var worldFiles = Directory.Exists(_worldsPath)
            ? Directory.GetFiles(_worldsPath, "*.zip")
            : Array.Empty<string>();

        if (worldFiles.Length == 0)
        {
            var debugMsg = Directory.Exists(_worldsPath)
                ? $"Directory exists but no .zip files found in:\n{_worldsPath}"
                : $"Directory does not exist:\n{_worldsPath}";

            var noWorldsLabel = new Label(debugMsg)
            {
                X = 1,
                Y = 12,
                Width = Dim.Fill(2),
                Height = 3,
                ColorScheme = cyberMagenta
            };
            win.Add(noWorldsLabel);

            var helpLabel = new Label("Generate worlds using: SoloAdventureSystem.AIWorldGenerator")
            {
                X = 1,
                Y = 16,
                ColorScheme = cyberCyan
            };
            win.Add(helpLabel);

            var exitButton = new Button("[ EXIT ]")
            {
                X = Pos.Center(),
                Y = 18,
                ColorScheme = cyberCyan
            };
            exitButton.Clicked += () => Application.RequestStop();
            win.Add(exitButton);
        }
        else
        {
            var countLabel = new Label($"Found {worldFiles.Length} world(s)")
            {
                X = Pos.Right(infoLabel) + 2,
                Y = 10,
                ColorScheme = cyberMagenta
            };
            win.Add(countLabel);

            var worldList = new ListView()
            {
                X = 1,
                Y = 11,
                Width = Dim.Fill(1),
                Height = Dim.Fill(3),
                ColorScheme = cyberCyan
            };

            var worldNames = worldFiles
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            worldList.SetSource(worldNames);
            win.Add(worldList);

            var selectButton = new Button("??? [ ? PLAY WORLD ? ] ???")
            {
                X = Pos.Center() - 10,
                Y = Pos.AnchorEnd(1),
                ColorScheme = cyberMagenta
            };

            selectButton.Clicked += () =>
            {
                if (worldList.SelectedItem >= 0 && worldList.SelectedItem < worldFiles.Length)
                {
                    selectedWorld = worldFiles[worldList.SelectedItem];
                    Application.RequestStop();
                }
            };
            win.Add(selectButton);

            var cancelButton = new Button("[ CANCEL ]")
            {
                X = Pos.Right(selectButton) + 2,
                Y = Pos.AnchorEnd(1),
                ColorScheme = cyberCyan
            };
            cancelButton.Clicked += () => Application.RequestStop();
            win.Add(cancelButton);

            // Double-click to select
            worldList.OpenSelectedItem += (_) =>
            {
                if (worldList.SelectedItem >= 0 && worldList.SelectedItem < worldFiles.Length)
                {
                    selectedWorld = worldFiles[worldList.SelectedItem];
                    Application.RequestStop();
                }
            };
        }

        top.Add(win);
        Application.Run();
        Application.Shutdown();

        return selectedWorld;
    }
}
