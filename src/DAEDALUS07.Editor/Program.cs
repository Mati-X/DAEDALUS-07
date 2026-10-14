using System.IO;
using DAEDALUS07.Core.Generation;
using DAEDALUS07.Core.Generation.Templates;
using DAEDALUS07.Core.Grid;
using SadConsole;
using SadConsole.Configuration;
using SadConsole.Input;
using SadRogue.Primitives;

Settings.WindowTitle = "DAEDALUS-07 - ROOM STUDIO [HOT-RELOAD]";
Settings.AllowWindowResize = true;
Settings.ResizeMode = Settings.WindowResizeOptions.Fit;

Builder configuration = new Builder()
    .SetWindowSizeInCells(120, 50)
    .OnStart(Startup);

Game.Create(configuration);
Game.Instance.Run();
Game.Instance.Dispose();

void Startup(object? sender, GameHost host)
{
    var customFont = host.LoadFont("Fonts/IBM16x16.font");
    var font8x8 = host.LoadFont("Fonts/IBM8x8.font");
    GameHost.Instance.DefaultFont = font8x8;

    string rootDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", ".."));
    string previewFilePath = Path.Combine(rootDir, "preview_room.txt");

    if (!File.Exists(previewFilePath))
    {
        previewFilePath = Path.Combine(Directory.GetCurrentDirectory(), "preview_room.txt");
    }

    var root = new ScreenObject();
    var studio = new RoomStudioSurface(120, 50, previewFilePath, font8x8, customFont);
    root.Children.Add(studio);
    Game.Instance.Screen = root;
}

public class RoomStudioSurface : ScreenSurface
{
    private readonly string _previewFilePath;
    private readonly IFont _font;
    private readonly IFont _hudFont;
    private FileSystemWatcher? _watcher;
    private DateTime _lastRead = DateTime.MinValue;
    private readonly List<SubnetGrid> _layerGrids = [];
    private string _status = "Initializing...";
    private int _cols = 0;
    private int _rows = 0;
    private int _gridW = 0;
    private int _gridH = 0;
    private int _layersCount = 1;

    private int _viewElevation = 0;
    private int? _soloLayer = null;

    public RoomStudioSurface(int viewWidth, int viewHeight, string previewFilePath, IFont font, IFont hudFont)
        : base(viewWidth, viewHeight)
    {
        _previewFilePath = previewFilePath;
        _font = font;
        _hudFont = hudFont;
        Font = font;
        UseKeyboard = true;
        IsFocused = true;

        SetupWatcher();
        ReloadRoom();
    }

    private void SetupWatcher()
    {
        string? dir = Path.GetDirectoryName(_previewFilePath);
        string file = Path.GetFileName(_previewFilePath);

        if (Directory.Exists(dir))
        {
            _watcher = new FileSystemWatcher(dir, file)
            {
                NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
                EnableRaisingEvents = true
            };

            _watcher.Changed += (_, _) => OnFileChanged();
            _watcher.Created += (_, _) => OnFileChanged();
        }
    }

    private void OnFileChanged()
    {
        if ((DateTime.UtcNow - _lastRead).TotalMilliseconds < 200) return;
        _lastRead = DateTime.UtcNow;

        System.Threading.Thread.Sleep(50);
        ReloadRoom();
    }

    public void ReloadRoom()
    {
        try
        {
            if (!File.Exists(_previewFilePath))
            {
                _status = $"File not found: {_previewFilePath}";
                Draw();
                return;
            }

            var lines = File.ReadAllLines(_previewFilePath);
            List<List<string>> rawLayers = [];
            List<string>? current = null;

            foreach (var rawLine in lines)
            {
                string line = rawLine.Trim();
                if (string.IsNullOrEmpty(line) || line.StartsWith("#")) continue;

                if (line.StartsWith("LAYER", StringComparison.OrdinalIgnoreCase))
                {
                    current = [];
                    rawLayers.Add(current);
                }
                else if (current != null)
                {
                    current.Add(line);
                }
            }

            if (rawLayers.Count == 0)
            {
                _status = "No LAYER definitions found in file.";
                Draw();
                return;
            }

            _layersCount = rawLayers.Count;
            _rows = rawLayers[0].Count;
            _cols = rawLayers[0][0].Split(' ', StringSplitOptions.RemoveEmptyEntries).Length;

            _gridW = _cols * ModuleLibrary.ModuleSize;
            _gridH = _rows * ModuleLibrary.ModuleSize;

            if (_gridW <= 0 || _gridH <= 0)
            {
                _status = "Invalid dimensions.";
                Draw();
                return;
            }

            _layerGrids.Clear();

            for (int l = 0; l < rawLayers.Count; l++)
            {
                var layerGrid = new SubnetGrid(_gridW, _gridH);
                for (int y = 0; y < _gridH; y++)
                {
                    for (int x = 0; x < _gridW; x++)
                    {
                        layerGrid[x, y] = new SubnetNode(SubnetNodeType.Void, (byte)l);
                    }
                }

                var layer = rawLayers[l];

                for (int mr = 0; mr < layer.Count; mr++)
                {
                    string[] tokens = layer[mr].Split(' ', StringSplitOptions.RemoveEmptyEntries);
                    for (int mc = 0; mc < tokens.Length; mc++)
                    {
                        string code = tokens[mc];
                        if (code == "--") continue;

                        string[] variant = ModuleLibrary.GetVariant(code);
                        int baseX = mc * ModuleLibrary.ModuleSize;
                        int baseY = mr * ModuleLibrary.ModuleSize;

                        for (int vy = 0; vy < ModuleLibrary.ModuleSize; vy++)
                        {
                            for (int vx = 0; vx < ModuleLibrary.ModuleSize; vx++)
                            {
                                char ch = variant[vy][vx];
                                int gx = baseX + vx;
                                int gy = baseY + vy;

                                if (ch == ' ') continue;

                                SubnetNodeType type = ch switch
                                {
                                    '.' => SubnetNodeType.Floor,
                                    'O' => SubnetNodeType.Pillar,
                                    '░' => SubnetNodeType.LowCover,
                                    'T' => SubnetNodeType.ServerTerminal,
                                    '≡' => SubnetNodeType.CatwalkFloor,
                                    '=' => SubnetNodeType.Stairs,
                                    _   => SubnetNodeType.Void
                                };

                                if (type != SubnetNodeType.Void)
                                {
                                    layerGrid[gx, gy] = new SubnetNode(type, (byte)l);
                                }
                            }
                        }
                    }
                }
                _layerGrids.Add(layerGrid);
            }

            _status = $"Loaded {_cols}x{_rows} mod ({_gridW}x{_gridH} cells), {_layersCount} layers.";
            Draw();
        }
        catch (Exception ex)
        {
            _status = $"Error: {ex.Message}";
            Draw();
        }
    }

    private void Draw()
    {
        Surface.Clear();

        if (_layerGrids.Count > 0)
        {
            for (int vx = 0; vx < Surface.ViewWidth; vx++)
            {
                for (int vy = 0; vy < Surface.ViewHeight - 2; vy++)
                {
                    int gx = Surface.ViewPosition.X + vx;
                    int gy = Surface.ViewPosition.Y + vy;

                    if (gx < 0 || gx >= _gridW || gy < 0 || gy >= _gridH)
                    {
                        Surface.SetGlyph(vx, vy, ' ', Color.DarkSlateGray, Color.Black);
                        continue;
                    }

                    SubnetNode visibleNode = default;
                    bool hasTile = false;
                    int tileElevation = 0;

                    if (_soloLayer.HasValue)
                    {
                        int solo = _soloLayer.Value;
                        if (solo >= 0 && solo < _layerGrids.Count)
                        {
                            var node = _layerGrids[solo][gx, gy];
                            if (node.Type != SubnetNodeType.Void)
                            {
                                visibleNode = node;
                                hasTile = true;
                                tileElevation = solo;
                            }
                        }
                    }
                    else
                    {
                        int maxL = Math.Min(_viewElevation, _layerGrids.Count - 1);
                        for (int l = maxL; l >= 0; l--)
                        {
                            var node = _layerGrids[l][gx, gy];
                            if (node.Type != SubnetNodeType.Void)
                            {
                                visibleNode = node;
                                hasTile = true;
                                tileElevation = l;
                                break;
                            }
                        }
                    }

                    if (!hasTile || visibleNode.Type == SubnetNodeType.Void)
                    {
                        Surface.SetGlyph(vx, vy, ' ', Color.DarkSlateGray, Color.Black);
                        continue;
                    }

                    int glyph = visibleNode.Type switch
                    {
                        SubnetNodeType.Floor => '.',
                        SubnetNodeType.CatwalkFloor => 240,
                        SubnetNodeType.Stairs => '=',
                        SubnetNodeType.LowCover => 220,
                        SubnetNodeType.Pillar => 219,
                        SubnetNodeType.ServerTerminal => 234,
                        _ => ' '
                    };

                    Color fg = visibleNode.Type switch
                    {
                        SubnetNodeType.Floor => tileElevation > 0 ? Color.Cyan : Color.LightCyan * 0.8f,
                        SubnetNodeType.CatwalkFloor => Color.Aqua,
                        SubnetNodeType.Stairs => Color.Gold,
                        SubnetNodeType.LowCover => Color.Orange,
                        SubnetNodeType.Pillar => Color.DarkGray,
                        SubnetNodeType.ServerTerminal => Color.LimeGreen,
                        _ => Color.Transparent
                    };

                    if (!_soloLayer.HasValue && tileElevation < _viewElevation)
                    {
                        int diff = _viewElevation - tileElevation;
                        float dim = MathF.Pow(0.35f, diff);
                        fg *= dim;
                    }

                    Color bg = Color.Black;
                    if (gx % 20 == 0 || gy % 20 == 0)
                    {
                        bg = new Color(20, 26, 32);
                    }

                    Surface.SetGlyph(vx, vy, glyph, fg, bg);
                }
            }
        }

        int hudY = Surface.ViewHeight - 2;
        Surface.DrawLine(new Point(0, hudY), new Point(Surface.ViewWidth - 1, hudY), 196, Color.Cyan);

        string elevLabel = _viewElevation == 0 ? "L0 (Parter)" : $"L{_viewElevation} (Kladka)";
        string soloText = _soloLayer.HasValue ? $"[SOLO: L{_soloLayer.Value}]" : $"[VIEW: {elevLabel} + nizsze]";

        Surface.Print(1, hudY + 1, $"{soloText} | [Q/E]: Zmiana pietra | [L]: Solo on/off | Arrows: Pan", Color.Yellow, Color.Black);
        IsDirty = true;
    }

    public override bool ProcessKeyboard(Keyboard keyboard)
    {
        int panSpeed = 2;
        if (keyboard.IsKeyDown(Keys.LeftShift) || keyboard.IsKeyDown(Keys.RightShift))
            panSpeed = 10;

        bool redraw = false;

        if (keyboard.IsKeyPressed(Keys.Q))
        {
            _viewElevation = Math.Max(0, _viewElevation - 1);
            redraw = true;
        }
        if (keyboard.IsKeyPressed(Keys.E))
        {
            _viewElevation = Math.Min(_layersCount - 1, _viewElevation + 1);
            redraw = true;
        }

        if (keyboard.IsKeyPressed(Keys.L))
        {
            if (!_soloLayer.HasValue)
                _soloLayer = 0;
            else if (_soloLayer.Value + 1 < _layersCount)
                _soloLayer = _soloLayer.Value + 1;
            else
                _soloLayer = null;

            redraw = true;
        }

        if (keyboard.IsKeyPressed(Keys.D0) || keyboard.IsKeyPressed(Keys.NumPad0))
        {
            _viewElevation = 0;
            redraw = true;
        }
        if (keyboard.IsKeyPressed(Keys.D1) || keyboard.IsKeyPressed(Keys.NumPad1))
        {
            _viewElevation = 1;
            redraw = true;
        }

        if (keyboard.IsKeyPressed(Keys.Left) || keyboard.IsKeyPressed(Keys.A))
        {
            Surface.ViewPosition = new Point(Math.Max(0, Surface.ViewPosition.X - panSpeed), Surface.ViewPosition.Y);
            redraw = true;
        }
        if (keyboard.IsKeyPressed(Keys.Right) || keyboard.IsKeyPressed(Keys.D))
        {
            Surface.ViewPosition = new Point(Surface.ViewPosition.X + panSpeed, Surface.ViewPosition.Y);
            redraw = true;
        }
        if (keyboard.IsKeyPressed(Keys.Up) || keyboard.IsKeyPressed(Keys.W))
        {
            Surface.ViewPosition = new Point(Surface.ViewPosition.X, Math.Max(0, Surface.ViewPosition.Y - panSpeed));
            redraw = true;
        }
        if (keyboard.IsKeyPressed(Keys.Down) || keyboard.IsKeyPressed(Keys.S))
        {
            Surface.ViewPosition = new Point(Surface.ViewPosition.X, Surface.ViewPosition.Y + panSpeed);
            redraw = true;
        }
        if (keyboard.IsKeyPressed(Keys.R))
        {
            ReloadRoom();
            redraw = true;
        }

        if (redraw)
        {
            Draw();
            return true;
        }

        return base.ProcessKeyboard(keyboard);
    }
}
