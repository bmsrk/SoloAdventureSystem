using Terminal.Gui;
using SoloAdventureSystem.UI.Themes;
using SoloAdventureSystem.Engine.Models;
using SoloAdventureSystem.Engine.Game;
using SoloAdventureSystem.Engine.WorldLoader;
using SoloAdventureSystem.Engine.Rules;

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
        _gameState.Player = new PlayerCharacter(); // Initialize player character
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

        var statsBtn = ComponentFactory.CreateButton("[ S ] Stats");
        statsBtn.X = Pos.Right(inventoryBtn) + 2;
        statsBtn.Y = 0;
        statsBtn.Clicked += ShowStats;

        var lookBtn = ComponentFactory.CreateButton("[ L ] Look Around");
        lookBtn.X = Pos.Right(statsBtn) + 2;
        lookBtn.Y = 0;
        lookBtn.Clicked += LookAround;

        var quitBtn = ComponentFactory.CreateDangerButton("[ Q ] Quit Game");
        quitBtn.X = Pos.Right(lookBtn) + 2;
        quitBtn.Y = 0;
        quitBtn.Clicked += QuitGame;

        commandFrame.Add(inventoryBtn, statsBtn, lookBtn, quitBtn);

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
            _statsLabel.Text = $"Turn: {_gameState.TurnCount} | Items: {_gameState.Inventory.Count} | HP: {_gameState.Player.HP}/{_gameState.Player.MaxHP}";
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
                    if (npc.Hostility == SoloAdventureSystem.Engine.Models.Hostility.Hostile)
                    {
                        _availableActions.Add($"Attack {npc.Name}");
                    }
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

        // Item usage from inventory
        if (_gameState.Inventory.Count > 0)
        {
            foreach (var itemId in _gameState.Inventory)
            {
                _availableActions.Add($"Use {itemId}");
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
        else if (action.StartsWith("Attack "))
        {
            var npcName = action.Substring(7);
            AttackNPC(npcName);
        }
        else if (action.StartsWith("Take "))
        {
            var itemName = action.Substring(5);
            TakeItem(itemName);
        }
        else if (action.StartsWith("Use "))
        {
            var itemName = action.Substring(4);
            UseItem(itemName);
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
            // Handle nulls gracefully with safe defaults
            var description = npc.Description ?? "They have nothing to say.";
            var name = npc.Name ?? "Someone";
            var message = $"\"{description}\"\n\n- {name}";
            MessageBox.Query("Conversation", message, "OK");
        }
    }

    private void AttackNPC(string npcName)
    {
        var npc = _gameState.World.Npcs?.FirstOrDefault(n => 
            n.Name == npcName && _gameState.CurrentLocation.NpcIds?.Contains(n.Id) == true);
        
        if (npc != null)
        {
            var playerStats = _gameState.Player.ToCharacterStats();
            var npcAttributes = new Dictionary<GameAttribute, int>
            {
                [GameAttribute.Body] = npc.Attributes.Strength,
                [GameAttribute.Mind] = npc.Attributes.Intelligence,
                [GameAttribute.Soul] = npc.Attributes.Wisdom,
                [GameAttribute.Presence] = npc.Attributes.Charisma
            };
            var npcSkills = new Dictionary<Skill, int>
            {
                [Skill.Combat] = 0, // Assume 0 for now
                [Skill.Stealth] = 0,
                [Skill.Knowledge] = 0,
                [Skill.Awareness] = 0,
                [Skill.Social] = 0,
                [Skill.Will] = 0,
                [Skill.Occult] = 0
            };
            var npcStats = new CharacterStats(npcAttributes, npcSkills, 10, 6); // Dummy HP and defense

            var attackResult = RuleEngine.RollCombatAttack(playerStats, npcStats);
            
            if (attackResult.Hit)
            {
                var damageResult = RuleEngine.RollDamage(DamageType.Unarmed);
                MessageBox.Query("Combat", $"You hit {npcName} for {damageResult.Damage} damage!\n\nThe {npcName} is defeated.", "Continue");
                // Remove NPC from location
                _gameState.CurrentLocation.NpcIds?.Remove(npc.Id);
            }
            else
            {
                MessageBox.Query("Combat", $"You miss {npcName}.", "Continue");
            }
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

    private void UseItem(string itemName)
    {
        if (_gameState.Inventory.Contains(itemName))
        {
            // Here you would define what using the item does
            MessageBox.Query("Item Used", $"You use the {itemName}.", "OK");
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

    private void ShowStats()
    {
        var statsText = $"Character: {_gameState.Player.Name}\n\n";
        statsText += "Attributes:\n";
        foreach (var attr in _gameState.Player.Attributes)
        {
            statsText += $"  {attr.Key}: {attr.Value}\n";
        }
        statsText += "\nSkills:\n";
        foreach (var skill in _gameState.Player.Skills)
        {
            statsText += $"  {skill.Key}: {skill.Value}\n";
        }
        statsText += $"\nHealth: {_gameState.Player.HP}/{_gameState.Player.MaxHP}\n";
        statsText += $"Equipment: {string.Join(", ", _gameState.Player.Equipment)}\n";
        statsText += $"Active Quests: {string.Join(", ", _gameState.Player.ActiveQuests)}";

        MessageBox.Query("Character Stats", statsText, "OK");
    }

    private void LookAround()
    {
        var locationName = _gameState.CurrentLocation.Name ?? "Unknown Location";
        var description = _gameState.CurrentLocation.Description ?? "You see nothing of interest.";
        
        var text = $"You look around {locationName}.\n\n";
        text += description;
        
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
