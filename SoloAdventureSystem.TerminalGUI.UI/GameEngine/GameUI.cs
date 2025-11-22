using Terminal.Gui;

namespace SoloAdventureSystem.TerminalGUI.GameEngine;

/// <summary>
/// Main game UI with cyberpunk terminal aesthetic
/// </summary>
public class GameUI
{
    private readonly GameState _gameState;
    
    private TextView? _locationView;
    private TextView? _logView;
    private TextField? _commandInput;
    private Label? _statusLabel;
    private Label? _healthLabel;
    private ListView? _exitsView;
    private ListView? _npcsView;

    public GameUI(GameState gameState)
    {
        _gameState = gameState;
    }

    public void Run()
    {
        Application.Init();
        var top = Application.Top;

        // Cyberpunk color scheme
        var cyberCyan = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Cyan, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Cyan),
            HotNormal = new Terminal.Gui.Attribute(Color.BrightCyan, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.BrightCyan)
        };

        var cyberGreen = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.BrightGreen, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.BrightGreen),
            HotNormal = new Terminal.Gui.Attribute(Color.Green, Color.Black),
            HotFocus = new Terminal.Gui.Attribute(Color.Black, Color.Green)
        };

        var cyberMagenta = new ColorScheme
        {
            Normal = new Terminal.Gui.Attribute(Color.Magenta, Color.Black),
            Focus = new Terminal.Gui.Attribute(Color.Black, Color.Magenta)
        };

        Colors.Base = cyberCyan;

        // Main window
        var win = new Window($"? SOLO ADVENTURE ? {_gameState.World.WorldDefinition!.Name}")
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = cyberCyan
        };

        // Status bar (top)
        _statusLabel = new Label($">>> HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth} | LVL: {_gameState.Player.Level}")
        {
            X = 1, Y = 0,
            Width = Dim.Fill(1),
            ColorScheme = cyberMagenta
        };
        win.Add(_statusLabel);

        _healthLabel = new Label($">>> LOCATION: {_gameState.CurrentLocation.Name}")
        {
            X = 1, Y = 1,
            Width = Dim.Fill(1),
            ColorScheme = cyberGreen
        };
        win.Add(_healthLabel);

        // Location description (left panel)
        var locationFrame = new FrameView("? LOCATION ?")
        {
            X = 1, Y = 3,
            Width = Dim.Percent(60),
            Height = Dim.Percent(50),
            ColorScheme = cyberCyan
        };

        _locationView = new TextView()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            WordWrap = true,
            ColorScheme = cyberGreen
        };
        locationFrame.Add(_locationView);
        win.Add(locationFrame);

        // Exits & NPCs (right panel)
        var exitsFrame = new FrameView("? EXITS ?")
        {
            X = Pos.Right(locationFrame) + 1, Y = 3,
            Width = Dim.Fill(1),
            Height = Dim.Percent(25),
            ColorScheme = cyberCyan
        };

        _exitsView = new ListView()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = cyberGreen
        };
        exitsFrame.Add(_exitsView);
        win.Add(exitsFrame);

        var npcsFrame = new FrameView("? NPCS ?")
        {
            X = Pos.Right(locationFrame) + 1,
            Y = Pos.Bottom(exitsFrame) + 1,
            Width = Dim.Fill(1),
            Height = Dim.Percent(25),
            ColorScheme = cyberCyan
        };

        _npcsView = new ListView()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ColorScheme = cyberGreen
        };
        npcsFrame.Add(_npcsView);
        win.Add(npcsFrame);

        // Game log (bottom left)
        var logFrame = new FrameView("? GAME LOG ?")
        {
            X = 1,
            Y = Pos.Bottom(locationFrame) + 1,
            Width = Dim.Percent(60),
            Height = Dim.Fill(3),
            ColorScheme = cyberCyan
        };

        _logView = new TextView()
        {
            X = 0, Y = 0,
            Width = Dim.Fill(),
            Height = Dim.Fill(),
            ReadOnly = true,
            WordWrap = true,
            ColorScheme = cyberGreen
        };
        logFrame.Add(_logView);
        win.Add(logFrame);

        // Command input (bottom)
        var commandLabel = new Label(">>> COMMAND:")
        {
            X = 1,
            Y = Pos.AnchorEnd(1),
            ColorScheme = cyberMagenta
        };
        win.Add(commandLabel);

        _commandInput = new TextField("")
        {
            X = Pos.Right(commandLabel) + 1,
            Y = Pos.AnchorEnd(1),
            Width = Dim.Fill(1),
            ColorScheme = cyberGreen
        };
        _commandInput.KeyPress += OnCommandInput;
        win.Add(_commandInput);

        top.Add(win);

        // Initialize display
        UpdateDisplay();
        _gameState.AddLog($"Welcome to {_gameState.World.WorldDefinition!.Name}!");
        _gameState.AddLog("Type 'help' for commands.");

        Application.Run();
        Application.Shutdown();
    }

    private void OnCommandInput(View.KeyEventEventArgs args)
    {
        if (args.KeyEvent.Key == Key.Enter)
        {
            var command = _commandInput!.Text.ToString()?.Trim() ?? "";
            if (!string.IsNullOrEmpty(command))
            {
                ProcessCommand(command);
                _commandInput.Text = "";
            }
            args.Handled = true;
        }
    }

    private void ProcessCommand(string command)
    {
        _gameState.AddLog($"> {command}");

        var parts = command.ToLower().Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0) return;

        var cmd = parts[0];
        var args = parts.Skip(1).ToArray();

        switch (cmd)
        {
            case "help":
            case "?":
                ShowHelp();
                break;

            case "look":
            case "l":
                LookAround();
                break;

            case "go":
            case "move":
            case "n":
            case "s":
            case "e":
            case "w":
            case "north":
            case "south":
            case "east":
            case "west":
                var direction = cmd == "go" || cmd == "move" ? (args.Length > 0 ? args[0] : "") : cmd;
                Move(direction);
                break;

            case "talk":
            case "speak":
                if (args.Length > 0)
                    TalkToNPC(string.Join(" ", args));
                else
                    _gameState.AddLog("Talk to whom?");
                break;

            case "examine":
            case "inspect":
                if (args.Length > 0)
                    Examine(string.Join(" ", args));
                else
                    _gameState.AddLog("Examine what?");
                break;

            case "inventory":
            case "inv":
            case "i":
                ShowInventory();
                break;

            case "stats":
            case "status":
                ShowStats();
                break;

            case "quit":
            case "exit":
                Application.RequestStop();
                break;

            default:
                _gameState.AddLog($"Unknown command: {cmd}. Type 'help' for commands.");
                break;
        }

        UpdateDisplay();
    }

    private void ShowHelp()
    {
        _gameState.AddLog("?????????????????????????????????????????");
        _gameState.AddLog("?         AVAILABLE COMMANDS            ?");
        _gameState.AddLog("?????????????????????????????????????????");
        _gameState.AddLog("? look (l)        - Look around         ?");
        _gameState.AddLog("? go <dir>        - Move direction      ?");
        _gameState.AddLog("? n/s/e/w         - Move north/etc      ?");
        _gameState.AddLog("? talk <npc>      - Talk to NPC         ?");
        _gameState.AddLog("? examine <thing> - Examine object      ?");
        _gameState.AddLog("? inventory (i)   - Show inventory      ?");
        _gameState.AddLog("? stats           - Show player stats   ?");
        _gameState.AddLog("? help (?)        - This help           ?");
        _gameState.AddLog("? quit            - Exit game           ?");
        _gameState.AddLog("?????????????????????????????????????????");
    }

    private void LookAround()
    {
        _gameState.AddLog($"You are at: {_gameState.CurrentLocation.Name}");
        _gameState.AddLog(_gameState.CurrentLocation.Description);

        if (_gameState.CurrentLocation.NpcIds?.Any() == true)
        {
            _gameState.AddLog($"NPCs here: {string.Join(", ", _gameState.CurrentLocation.NpcIds.Select(id => _gameState.GetNpcById(id)?.Name ?? id))}");
        }

        if (_gameState.CurrentLocation.Connections?.Any() == true)
        {
            _gameState.AddLog($"Exits: {string.Join(", ", _gameState.CurrentLocation.Connections.Keys)}");
        }
    }

    private void Move(string direction)
    {
        if (_gameState.CurrentLocation.Connections == null || !_gameState.CurrentLocation.Connections.Any())
        {
            _gameState.AddLog("There are no exits here.");
            return;
        }

        // Normalize direction
        var normalizedDir = direction.ToLower() switch
        {
            "n" => "north",
            "s" => "south",
            "e" => "east",
            "w" => "west",
            _ => direction.ToLower()
        };

        if (_gameState.CurrentLocation.Connections.TryGetValue(normalizedDir, out var targetId))
        {
            var targetLocation = _gameState.GetLocationById(targetId);
            if (targetLocation != null)
            {
                _gameState.CurrentLocation = targetLocation;
                _gameState.AddLog($"You move {normalizedDir} to {targetLocation.Name}.");
                LookAround();
            }
            else
            {
                _gameState.AddLog($"ERROR: Location {targetId} not found!");
            }
        }
        else
        {
            _gameState.AddLog($"You can't go {normalizedDir} from here.");
            _gameState.AddLog($"Available exits: {string.Join(", ", _gameState.CurrentLocation.Connections.Keys)}");
        }
    }

    private void TalkToNPC(string npcName)
    {
        if (_gameState.CurrentLocation.NpcIds == null || !_gameState.CurrentLocation.NpcIds.Any())
        {
            _gameState.AddLog("There's no one here to talk to.");
            return;
        }

        var npc = _gameState.CurrentLocation.NpcIds
            .Select(id => _gameState.GetNpcById(id))
            .FirstOrDefault(n => n?.Name.Contains(npcName, StringComparison.OrdinalIgnoreCase) == true);

        if (npc == null)
        {
            _gameState.AddLog($"You don't see anyone named '{npcName}' here.");
            return;
        }

        _gameState.AddLog($"You talk to {npc.Name}.");
        _gameState.AddLog($"{npc.Name}: \"{npc.Description}\"");

        var faction = _gameState.GetFactionById(npc.FactionId);
        if (faction != null)
        {
            _gameState.AddLog($"({npc.Name} is a member of {faction.Name})");
        }
    }

    private void Examine(string target)
    {
        _gameState.AddLog($"You examine {target}...");
        _gameState.AddLog("You don't find anything special.");
    }

    private void ShowInventory()
    {
        _gameState.AddLog("?????????????????????????????????????????");
        _gameState.AddLog("?           INVENTORY                   ?");
        _gameState.AddLog("?????????????????????????????????????????");
        
        if (_gameState.Inventory.Count == 0)
        {
            _gameState.AddLog("Your inventory is empty.");
        }
        else
        {
            foreach (var item in _gameState.Inventory)
            {
                _gameState.AddLog($"  • {item.Key} x{item.Value}");
            }
        }
    }

    private void ShowStats()
    {
        var p = _gameState.Player;
        _gameState.AddLog("?????????????????????????????????????????");
        _gameState.AddLog($"? NAME: {p.Name,-32}?");
        _gameState.AddLog($"? LEVEL: {p.Level,-31}?");
        _gameState.AddLog($"? HP: {p.Health}/{p.MaxHealth,-28}?");
        _gameState.AddLog($"? XP: {p.Experience,-32}?");
        _gameState.AddLog("?????????????????????????????????????????");
        _gameState.AddLog($"? STR: {p.Attributes.Strength,-31}?");
        _gameState.AddLog($"? DEX: {p.Attributes.Dexterity,-31}?");
        _gameState.AddLog($"? INT: {p.Attributes.Intelligence,-31}?");
        _gameState.AddLog($"? CON: {p.Attributes.Constitution,-31}?");
        _gameState.AddLog($"? WIS: {p.Attributes.Wisdom,-31}?");
        _gameState.AddLog($"? CHA: {p.Attributes.Charisma,-31}?");
        _gameState.AddLog("?????????????????????????????????????????");
    }

    private void UpdateDisplay()
    {
        // Update status bar
        if (_statusLabel != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _statusLabel.Text = $">>> HP: {_gameState.Player.Health}/{_gameState.Player.MaxHealth} | LVL: {_gameState.Player.Level} | XP: {_gameState.Player.Experience}";
            });
        }

        if (_healthLabel != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _healthLabel.Text = $">>> LOCATION: {_gameState.CurrentLocation.Name}";
            });
        }

        // Update location description
        if (_locationView != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _locationView.Text = $"??? {_gameState.CurrentLocation.Name.ToUpper()} ???\n\n{_gameState.CurrentLocation.Description}";
            });
        }

        // Update exits list
        if (_exitsView != null && _gameState.CurrentLocation.Connections != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                var exits = _gameState.CurrentLocation.Connections.Keys
                    .Select(k => $"? {k.ToUpper()}")
                    .ToList();
                _exitsView.SetSource(exits);
            });
        }

        // Update NPCs list
        if (_npcsView != null && _gameState.CurrentLocation.NpcIds != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                var npcs = _gameState.CurrentLocation.NpcIds
                    .Select(id => _gameState.GetNpcById(id))
                    .Where(n => n != null)
                    .Select(n => $"• {n!.Name}")
                    .ToList();
                _npcsView.SetSource(npcs);
            });
        }

        // Update game log
        if (_logView != null)
        {
            Application.MainLoop.Invoke(() =>
            {
                _logView.Text = string.Join("\n", _gameState.GameLog);
                _logView.MoveEnd();
            });
        }
    }
}
