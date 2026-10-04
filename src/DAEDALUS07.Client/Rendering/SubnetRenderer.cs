using DAEDALUS07.Client.Effects;
using DAEDALUS07.Client.Input;
using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Fov;
using DAEDALUS07.Core.Generation;
using SadConsole;
using DAEDALUS07.Core.Grid;
using DAEDALUS07.Core.Systems;
using Microsoft.Xna.Framework.Input;
using SadRogue.Primitives;
using Mouse = SadConsole.Input.Mouse;

namespace DAEDALUS07.Client.Rendering;

public class SubnetRenderer : ScreenSurface
{
    private readonly SubnetGrid _grid;
    private readonly ShadowcastFov _fov = new();
    private readonly TurnManager _turnManager = new();
    private readonly List<Enemy> _enemies;
    private readonly ScreenSurface _playerSurface;
    private readonly List<Room> _rooms;
    
    private Entity _player;

    private const int FovRadius = 16;
    
    private float _visualPlayerX;
    private float _visualPlayerY;
    
    const float lerpSpeed = 25f;
    
    
    private readonly CameraController _cameraController;
    private readonly PlayerMovementController _playerMovementController = new();
    private readonly GhostTrailEffect _ghostTrailEffect;
    private readonly RoomDecryptionEffect _decryptionEffect = new();

    public SubnetRenderer(SubnetGrid grid, Entity player, List<Enemy> enemies, List<Room> rooms, IFont font16x16) : base(116,74, grid.Width, grid.Height)
    {
        _grid = grid;
        _rooms = rooms;
        
        UseKeyboard = true;
        UseMouse = true;
        IsFocused = true;
        UsePixelPositioning = true;
        _player = player;

        _enemies = enemies;
        
        int maxPixelX = (_grid.Width - Surface.ViewWidth) * FontSize.X;                                                                                                                                                              
        int maxPixelY = (_grid.Height - Surface.ViewHeight) * FontSize.Y;
        
        _visualPlayerX = player.x * FontSize.X;
        _visualPlayerY = player.y * FontSize.Y;

        _cameraController = new CameraController(Math.Clamp(player.x * FontSize.X - (Surface.ViewWidth / 2f * FontSize.X), 0, maxPixelX), Math.Clamp(player.y * FontSize.Y - (Surface.ViewHeight / 2f * FontSize.Y), 0, maxPixelY), this);
        
        _ghostTrailEffect = new GhostTrailEffect(this, font16x16);

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
        float centerX = _player.x + (_player.Size / 2f);
        float centerY = _player.y + (_player.Size / 2f);

        float dx = x - centerX;
        float dy = y - centerY;
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
        if (_decryptionEffect.IsActive)
        {
            _decryptionEffect.Update(delta, _grid, this, () => Render());
        }
        
        if (_playerMovementController.Update(delta, _player, _grid, _ghostTrailEffect))
        {
            UpdateFov();
            var currentRoom = _rooms.FirstOrDefault(r => r.Contains(_player.x, _player.y));
            
            if (currentRoom != null && !currentRoom.IsCleared && !currentRoom.IsActive)
            {
                currentRoom.IsActive = true;
                foreach (var (dx, dy) in currentRoom.Doors)
                {
                    _grid[dx, dy] = new SubnetNode(false, true, SubnetNodeType.LaserBarrier);
                }
            }
            _turnManager.ExecuteTurn(_player,_enemies,_grid);
            
            if (currentRoom != null && currentRoom.IsActive && currentRoom.Enemies.All(e => !e.IsAlive))
            {
                currentRoom.IsActive = false;
                currentRoom.IsCleared = true;

                _decryptionEffect.Trigger(currentRoom, _player.x, _player.y);
            }
            Render();
        }
        
        GamePadState pad = Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
        Mouse mouse = SadConsole.GameHost.Instance.Mouse; 
        
        int playerPixelX = (int)Math.Round(_visualPlayerX);
        int playerPixelY = (int)Math.Round(_visualPlayerY);

        var (cellX, cellY, subPixelX, subPixelY) = _cameraController.Update(
            delta, 
            new Point(playerPixelX, playerPixelY), 
            mouse, pad, _grid
        );

        Surface.ViewPosition = new Point(cellX, cellY);
        Position = new Point(-subPixelX, -subPixelY);

        int targetX = _player.x * FontSize.X;
        int targetY = _player.y * FontSize.Y;
        
        float t = (float)Math.Min(1.0, lerpSpeed * delta.TotalSeconds);
        
        _visualPlayerX += (targetX - _visualPlayerX) * t;
        _visualPlayerY += (targetY - _visualPlayerY) * t;

        int px = (int)Math.Round(_visualPlayerX) - (cellX * FontSize.X);
        int py = (int)Math.Round(_visualPlayerY) - (cellY * FontSize.Y);
        _playerSurface.Position = new Point(px, py);

        _ghostTrailEffect.Update(delta, cellX, cellY, FontSize);
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
    
    private bool IsWallNearClearedRoom(int wx, int wy)
    {
        for (int dy = -1; dy <= 3; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int fx = wx + dx, fy = wy + dy;
                if (_grid.IsInBounds(fx, fy) && _grid[fx, fy].Type == SubnetNodeType.Floor)
                {
                    if (_rooms.Any(r => r.IsCleared && r.Contains(fx, fy)))
                        return true;
                }
            }
        }
        return false;
    }
    
    private bool IsWallNearRoom(Room room, int wx, int wy)
    {
        for (int dy = -1; dy <= 3; dy++)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                int fx = wx + dx, fy = wy + dy;
                if (_grid.IsInBounds(fx, fy) && _grid[fx, fy].Type == SubnetNodeType.Floor && room.Contains(fx, fy))
                    return true;
            }
        }
        return false;
    }

    public void Render()
    {
        int startX = Math.Max(2, _player.x - FovRadius - 4);
        int endX = Math.Min(_grid.Width - 3, _player.x + FovRadius + 4);

        int startY = Math.Max(0, _player.y - FovRadius - 4);
        int endY = Math.Min(_grid.Height - 1, _player.y + FovRadius + 4);
        
        var currentRoom = _rooms.FirstOrDefault(r => r.Contains(_player.x, _player.y));
        
        bool playerInClearedRoom = currentRoom != null && currentRoom.IsCleared;
        
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                var node = _grid[x, y];
                
                bool isRoomLit = false;
                if (playerInClearedRoom)
                {
                    isRoomLit = node.Type == SubnetNodeType.Floor
                        ? currentRoom!.Contains(x, y)
                        : IsWallNearRoom(currentRoom!, x, y);
                }
 
                if ((!node.IsDiscovered && !isRoomLit) || node.Type == SubnetNodeType.Void)
                {
                    Surface.SetGlyph(x, y, ' ', Color.White, Color.Black);
                    continue;
                }
                
                bool isHorizontal = _grid[x - 1, y].Type != SubnetNodeType.Floor || _grid[x + 1, y].Type != SubnetNodeType.Floor;
                
                int glyph = node.Type switch
                {
                    SubnetNodeType.Floor => '.',
                    SubnetNodeType.WallFront => '#',
                    SubnetNodeType.WallRoof => '#',
                    SubnetNodeType.LaserBarrier => isHorizontal ? '═' : '║',
                    _ => ' '
                };

                float intensity = node.IsSight ? GetIntensity(x, y) : 0f;
                Color foreground;
                if (node.Type == SubnetNodeType.LaserBarrier)
                {
                    foreground = Color.Crimson;;
                }
                else if (node.Type == SubnetNodeType.Floor)
                {
                    foreground = isRoomLit ? Theme.Current.FloorLit : node.IsSight
                        ? Color.Lerp(Theme.Current.FloorFog, Theme.Current.FloorLit, intensity)
                        : Theme.Current.FloorFog;
                }
                else
                {
                    Color bright = node.Type == SubnetNodeType.WallFront 
                        ? Theme.Current.WallFront 
                        : Theme.Current.WallRoof;

                    foreground = isRoomLit ? bright : node.IsSight 
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