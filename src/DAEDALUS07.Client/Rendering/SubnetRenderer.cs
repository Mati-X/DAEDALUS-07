using DAEDALUS07.Client.Input;
using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Fov;
using SadConsole;
using DAEDALUS07.Core.Grid;
using Microsoft.Xna.Framework.Input;
using SadRogue.Primitives;
using Mouse = SadConsole.Input.Mouse;

namespace DAEDALUS07.Client.Rendering;

public class SubnetRenderer : ScreenSurface
{
    private readonly SubnetGrid _grid;
    private readonly ShadowcastFov _fov = new();
    private Entity _player;
    private readonly List<Entity> _enemies;
    private readonly ScreenSurface _playerSurface;

    private const int FovRadius = 16;
    
    private readonly CameraController _cameraController;
    private readonly PlayerMovementController _playerMovementController = new();

    public SubnetRenderer(SubnetGrid grid, Entity player, List<Entity> enemies, IFont font16x16) : base(116,74, grid.Width, grid.Height)
    {
        _grid = grid;
        
        UseKeyboard = true;
        UseMouse = true;
        IsFocused = true;
        UsePixelPositioning = true;
        _player = player;

        _enemies = enemies;
        
        int maxPixelX = (_grid.Width - Surface.ViewWidth) * FontSize.X;                                                                                                                                                              
        int maxPixelY = (_grid.Height - Surface.ViewHeight) * FontSize.Y;

        _cameraController = new CameraController(Math.Clamp(player.x * FontSize.X - (Surface.ViewWidth / 2f * FontSize.X), 0, maxPixelX), Math.Clamp(player.y * FontSize.Y - (Surface.ViewHeight / 2f * FontSize.Y), 0, maxPixelY), this);
        _playerSurface = new ScreenSurface(1, 1)
        {
            Font = font16x16,
            UsePixelPositioning = true
        };
        _playerSurface.Surface.SetGlyph(0, 0, '@', Theme.Current.Player, Color.Transparent);
        Children.Add(_playerSurface);
        
        UpdateFov();
        Render();
    }

    private float GetIntensity(int x, int y)
    {
        float dx = x - _player.x;
        float dy = y - _player.y;
        float distance = MathF.Sqrt(dx * dx + dy * dy);
        float light = 1f - Math.Clamp(distance / FovRadius, 0f, 1f);
        return light * light;
    }

    public CameraController CameraController
    {
        get { return _cameraController; }
    }

    public override void Update(TimeSpan delta)
    {
        if (_playerMovementController.Update(delta, _player, _grid))
        {
            UpdateFov();
            Render();
        }
        
        GamePadState pad = Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
        Mouse mouse = SadConsole.GameHost.Instance.Mouse; 
        
        var (cellX, cellY, subPixelX, subPixelY) = _cameraController.Update(delta, new Point(_player.x, _player.y), mouse, pad, _grid);

        Surface.ViewPosition = new Point(cellX, cellY);
        Position = new Point(-subPixelX, -subPixelY);

        int px = (_player.x - cellX) * FontSize.X;
        int py = (_player.y - cellY) * FontSize.Y;
        _playerSurface.Position = new Point(px, py);
    }
    
    private void UpdateFov()
    {
        _fov.Compute(_grid, _player.x, _player.y + (_player.Size - 1), FovRadius);

        int startX = Math.Max(0, _player.x - FovRadius);
        int endX = Math.Min(_grid.Width, _player.x + FovRadius + 1);
        int startY = Math.Max(0, _player.y - FovRadius);
        int endY = Math.Min(_grid.Height, _player.y + FovRadius + 1);

        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                if (_grid[x, y].Type == SubnetNodeType.Floor && _grid[x, y].IsSight)
                {
                    for (int dx = -2; dx <= 2; dx++)
                    {
                        for (int dy = -3; dy <= 1; dy++)
                        {
                            int wx = x + dx, wy = y + dy;
                            if (_grid.IsInBounds(wx, wy) && _grid[wx, wy].Type != SubnetNodeType.Floor && _grid[wx, wy].Type != SubnetNodeType.Void)
                            {
                                _grid[wx, wy].IsSight = true;
                                _grid[wx, wy].IsDiscovered = true;
                            }
                        }
                    }
                }
            }
        }
    }

    public void Render()
    {
        int startX = Math.Max(2, _player.x - FovRadius - 4);
        int endX = Math.Min(_grid.Width - 3, _player.x + FovRadius + 4);

        int startY = Math.Max(0, _player.y - FovRadius - 4);
        int endY = Math.Min(_grid.Height - 1, _player.y + FovRadius + 4);
        
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                var node = _grid[x, y];
                if (!node.IsDiscovered || node.Type == SubnetNodeType.Void)
                {
                    Surface.SetGlyph(x, y, ' ', Color.White, Color.Black);
                    continue;
                }

                int glyph = node.Type switch
                {
                    SubnetNodeType.Floor => '.',
                    SubnetNodeType.WallFront => 219,
                    SubnetNodeType.WallRoof => 219,
                    _ => ' '
                };

                float intensity = node.IsSight ? GetIntensity(x, y) : 0f;
                Color foreground;

                if (node.Type == SubnetNodeType.Floor)
                {
                    foreground = node.IsSight
                        ? Color.Lerp(Theme.Current.FloorFog, Theme.Current.FloorLit, intensity)
                        : Theme.Current.FloorFog;
                }
                else
                {
                    Color bright = node.Type == SubnetNodeType.WallFront 
                        ? Theme.Current.WallFront 
                        : Theme.Current.WallRoof;

                    foreground = node.IsSight 
                        ? Color.Lerp(Theme.Current.WallDim, bright, intensity) 
                        : Theme.Current.WallDim;
                }

                Surface.SetGlyph(x, y, glyph, foreground, Color.Black);
            }
        }

        foreach (var enemy in  _enemies)
        {
            if(!enemy.IsAlive) continue;
            if(!_grid[enemy.x,enemy.y].IsSight) continue;
            Surface.SetGlyph(enemy.x, enemy.y, enemy.Glyph, Theme.Current.Enemy, Color.Black);
        }
        
        IsDirty = true;
    }
}