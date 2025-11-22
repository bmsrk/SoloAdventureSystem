using Terminal.Gui;
using SoloAdventureSystem.UI.Themes;
using MudVision.WorldLoader;
using MudVision.World.Models;
using MudVision.Game;

namespace SoloAdventureSystem.UI.Game;

/// <summary>
/// Main gameplay UI for the adventure
/// </summary>
public class GameUI
{
    private readonly IWorldState _worldState;
    private GameState _gameState;
    private Window? _mainWindow;
    private TextView? _descriptionView;
    private ListView? _actionsListView;
    private Label? _locationLabel;
    private Label? _statsLabel;
    private List<string> _availableActions;

    public GameUI(IWorldState worldState)
    {
        _worldState = worldState;
        _gameState = new GameState();
        _availableActions = new List<string>();
    }

    public void StartGame()
    {
        Application.Init();
        try
        {
            StartGameInternal();
        }
        finally
        {
            Application.Shutdown();
        }
    }

    public void StartGameInternal()
    {
        var world = _worldState.GetWorld();
        if (world?.WorldDefinition == null)
        {
            throw new InvalidOperationException("No world loaded!");
        }

        // Initialize game state
        _gameState.World = world;
        var startLocation = world.Rooms?.FirstOrDefault(l => l.Id == world.WorldDefinition.StartLocationId);
        
        if (startLocation == null)
        {
            throw new InvalidOperationException("Start location not found!");
        }

        _gameState.CurrentLocation = startLocation;
        _gameState.TurnCount = 0;

        // Main window
        _mainWindow = ComponentFactory.CreateWindow(world.WorldDefinition.Name);
        _mainWindow.X = 0;
        _mainWindow.Y = 0;
        _mainWindow.Width = Dim.Fill();
        _mainWindow.Height = Dim.Fill();

        // Location name at top
        _locationLabel = ComponentFactory.CreateAccentLabel("");
        _locationLabel.X = 1;
        _locationLabel.Y = 0;
        _locationLabel.Width = Dim.Fill(1);
        _locationLabel.Height = 1;

        // Stats panel
        _statsLabel = ComponentFactory.CreateMutedLabel("");
        _statsLabel.X = 1;
        _statsLabel.Y = 1;
        _statsLabel.Width = Dim.Fill(1);
        _statsLabel.Height = 1;

        // Description frame
        var descriptionFrame = ComponentFactory.CreateFrame("Description");
        descriptionFrame.X = 1;
        descriptionFrame.Y = 3;
        descriptionFrame.Width = Dim.Percent(70);
        descriptionFrame.Height = Dim.Fill(9);

        _descriptionView = ComponentFactory.CreateTextView();
        _descriptionView.X = 0;
        _descriptionView.Y = 0;
        _descriptionView.Width = Dim.Fill();
        _descriptionView.Height = Dim.Fill();
        _descriptionView.ReadOnly = true;
        _descriptionView.WordWrap = true;
        descriptionFrame.Add(_descriptionView);

        // Actions panel
        var actionsFrame = ComponentFactory.CreateFrame("Actions");
        actionsFrame.X = Pos.Right(descriptionFrame) + 1;
        actionsFrame.Y = 3;
        actionsFrame.Width = Dim.Fill(1);
        actionsFrame.Height = Dim.Fill(9);

        _actionsListView = ComponentFactory.CreateListView();
        _actionsListView.X = 0;
        _actionsListView.Y = 0;
        _actionsListView.Width = Dim.Fill();
        _actionsListView.Height = Dim.Fill(2);
        _actionsListView.AllowsMarking = false;
        _actionsListView.OpenSelectedItem += OnActionSelected;

        var actionHint = ComponentFactory.CreateHintLabel("Up/Down Select - Enter to confirm");
        actionHint.X = 0;
        actionHint.Y = Pos.AnchorEnd(1);
        actionHint.Width = Dim.Fill();

        actionsFrame.Add(_actionsListView, actionHint);

        // Command panel at bottom
        var commandFrame = ComponentFactory.CreateFrame("Command");
        commandFrame.X = 1;
        commandFrame.Y = Pos.AnchorEnd(5);
        commandFrame.Width = Dim.Fill(1);
        commandFrame.Height = 5;

        var inventoryBtn = ComponentFactory.CreateButton("[ I ] Inventory");
        inventoryBtn.X = 1;
        inventoryBtn.Y = 0;
        inventoryBtn.Clicked += ShowInventory;

        var lookBtn = ComponentFactory.CreateButton("[ L ] Look Around");
        lookBtn.X = Pos.Right(inventoryBtn) + 2;
        lookBtn.Y = 0;
        lookBtn.Clicked += LookAround;

        var quitBtn = ComponentFactory.CreateDangerButton("[ Q ] Quit Game");
        quitBtn.X = Pos.Right(lookBtn) + 2;
        quitBtn.Y = 0;
        quitBtn.Clicked += QuitGame;

        commandFrame.Add(inventoryBtn, lookBtn, quitBtn);

        _mainWindow.Add(_locationLabel, _statsLabel, descriptionFrame, actionsFrame, commandFrame);

        // Initial update
        UpdateUI();

        Application.Run(_mainWindow);
    }

    private void UpdateUI()
    {
        if (_locationLabel != null)
        {
            _locationLabel.Text = $"@ {_gameState.CurrentLocation.Name}";
        }

        if (_statsLabel != null)
        {
            _statsLabel.Text = $"Turn: {_gameState.TurnCount} | Items: {_gameState.Inventory.Count}";
        }

        if (_descriptionView != null)
        {
            var description = _gameState.CurrentLocation.Description;
            
            // Add NPCs info
            if (_gameState.CurrentLocation.NpcIds?.Count > 0)
            {
                var npcs = _gameState.World.Npcs?
                    .Where(n => _gameState.CurrentLocation.NpcIds.Contains(n.Id))
                    .ToList();
                
                if (npcs?.Count > 0)
                {
                    description += "\n\n= People here:\n";
                    foreach (var npc in npcs)
                    {
                        description += $"  * {npc.Name} - {npc.Description}\n";
                    }
                }
            }

            // Add items info
            if (_gameState.CurrentLocation.ItemIds?.Count > 0)
            {
                description += "\n\n= Items here:\n";
                foreach (var itemId in _gameState.CurrentLocation.ItemIds)
                {
                    description += $"  * {itemId}\n";
                }
            }

            _descriptionView.Text = description;
        }

        // Update available actions
        UpdateActions();
    }

    private void UpdateActions()
    {
        _availableActions.Clear();

        // Movement options
        if (_gameState.CurrentLocation.Connections?.Count > 0)
        {
            foreach (var connection in _gameState.CurrentLocation.Connections)
            {
                _availableActions.Add($"Go {connection.Key}");
            }
        }

        // Interaction options
        if (_gameState.CurrentLocation.NpcIds?.Count > 0)
        {
            var npcs = _gameState.World.Npcs?
                .Where(n => _gameState.CurrentLocation.NpcIds.Contains(n.Id))
                .ToList();
            
            if (npcs?.Count > 0)
            {
                foreach (var npc in npcs)
                {
                    _availableActions.Add($"Talk to {npc.Name}");
                }
            }
        }

        if (_gameState.CurrentLocation.ItemIds?.Count > 0)
        {
            foreach (var itemId in _gameState.CurrentLocation.ItemIds)
            {
                _availableActions.Add($"Take {itemId}");
            }
        }

        if (_actionsListView != null)
        {
            _actionsListView.SetSource(_availableActions);
        }
    }

    private void OnActionSelected(ListViewItemEventArgs args)
    {
        if (args.Item < 0 || args.Item >= _availableActions.Count)
            return;

        var action = _availableActions[args.Item];
        _gameState.TurnCount++;

        if (action.StartsWith("Go "))
        {
            var direction = action.Substring(3);
            MoveToLocation(direction);
        }
        else if (action.StartsWith("Talk to "))
        {
            var npcName = action.Substring(8);
            TalkToNPC(npcName);
        }
        else if (action.StartsWith("Take "))
        {
            var itemName = action.Substring(5);
            TakeItem(itemName);
        }

        UpdateUI();
    }

    private void MoveToLocation(string direction)
    {
        if (_gameState.CurrentLocation.Connections?.TryGetValue(direction, out var locationId) == true)
        {
            var newLocation = _gameState.World.Rooms?.FirstOrDefault(l => l.Id == locationId);
            if (newLocation != null)
            {
                _gameState.CurrentLocation = newLocation;
                MessageBox.Query("Travel", $"You travel {direction}...", "Continue");
            }
        }
    }

    private void TalkToNPC(string npcName)
    {
        var npc = _gameState.World.Npcs?.FirstOrDefault(n => 
            n.Name == npcName && _gameState.CurrentLocation.NpcIds?.Contains(n.Id) == true);
        
        if (npc != null)
        {
            var message = $"\"{npc.Description}\"\n\n- {npc.Name}";
            MessageBox.Query("Conversation", message, "OK");
        }
    }

    private void TakeItem(string itemName)
    {
        if (_gameState.CurrentLocation.ItemIds?.Contains(itemName) == true)
        {
            _gameState.Inventory.Add(itemName);
            _gameState.CurrentLocation.ItemIds?.Remove(itemName);
            MessageBox.Query("Item Acquired", $"You take the {itemName}.", "OK");
        }
    }

    private void ShowInventory()
    {
        if (_gameState.Inventory.Count == 0)
        {
            MessageBox.Query("Inventory", "Your inventory is empty.", "OK");
            return;
        }

        var inventoryText = "Your inventory:\n\n";
        foreach (var item in _gameState.Inventory)
        {
            inventoryText += $"* {item}\n";
        }

        MessageBox.Query("Inventory", inventoryText, "OK");
    }

    private void LookAround()
    {
        var text = $"You look around {_gameState.CurrentLocation.Name}.\n\n";
        text += _gameState.CurrentLocation.Description;
        
        MessageBox.Query("Look Around", text, "OK");
    }

    private void QuitGame()
    {
        var result = MessageBox.Query("Quit Game", 
            "Are you sure you want to quit?\nYour progress will not be saved.", 
            "Yes", "No");
        
        if (result == 0)
        {
            Application.RequestStop();
        }
    }
}
