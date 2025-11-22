using Terminal.Gui;
using SoloAdventureSystem.ContentGenerator.Utils;
using SoloAdventureSystem.UI.Themes;

namespace SoloAdventureSystem.UI.Game;

/// <summary>
/// World selection UI for choosing which world to play
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
        try
        {
            return SelectWorldInternal();
        }
        finally
        {
            Application.Shutdown();
        }
    }

    public string? SelectWorldInternal()
    {
        string? selectedPath = null;
        
        var win = ComponentFactory.CreateWindow("Select World");
        // Use full screen but with margins for better visibility
        win.X = 0;
        win.Y = 0;
        win.Width = Dim.Fill();
        win.Height = Dim.Fill();

        // Title
        var title = ComponentFactory.CreateTitle("[ Choose Your Adventure ]");
        title.X = Pos.Center();
        title.Y = 1;

        // Instructions
        var instructions = ComponentFactory.CreateMutedLabel("Select a world to explore");
        instructions.X = Pos.Center();
        instructions.Y = 3;
        instructions.TextAlignment = TextAlignment.Centered;

        var worldFiles = Directory.GetFiles(_worldsPath, "*.zip")
            .Select(Path.GetFileName)
            .Where(f => f != null)
            .Cast<string>()
            .ToArray();

        if (worldFiles.Length == 0)
        {
            var noWorldsLabel = ComponentFactory.CreateMutedLabel("No worlds found. Generate a world first!");
            noWorldsLabel.X = Pos.Center();
            noWorldsLabel.Y = Pos.Center();
            noWorldsLabel.TextAlignment = TextAlignment.Centered;

            var okBtn = ComponentFactory.CreatePrimaryButton("[ OK ]");
            okBtn.X = Pos.Center();
            okBtn.Y = Pos.Center() + 2;
            okBtn.Clicked += () => Application.RequestStop();

            win.Add(title, instructions, noWorldsLabel, okBtn);
            Application.Run(win);
            return null;
        }

        var listView = ComponentFactory.CreateListView(worldFiles);
        listView.X = 1;
        listView.Y = 5;
        listView.Width = Dim.Fill(1);
        listView.Height = Dim.Fill(5);

        var selectBtn = ComponentFactory.CreatePrimaryButton("[ > ] Select");
        selectBtn.X = 1;
        selectBtn.Y = Pos.AnchorEnd(1);
        selectBtn.Clicked += () =>
        {
            if (listView.SelectedItem >= 0 && listView.SelectedItem < worldFiles.Length)
            {
                selectedPath = Path.Combine(_worldsPath, worldFiles[listView.SelectedItem]);
                Application.RequestStop();
            }
        };

        var cancelBtn = ComponentFactory.CreateButton("[ < ] Cancel");
        cancelBtn.X = Pos.Right(selectBtn) + 2;
        cancelBtn.Y = Pos.AnchorEnd(1);
        cancelBtn.Clicked += () => Application.RequestStop();

        win.Add(title, instructions, listView, selectBtn, cancelBtn);

        Application.Run(win);

        return selectedPath;
    }
}
