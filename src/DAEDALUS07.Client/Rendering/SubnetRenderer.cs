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
        
    private float _currentOffsetX;                                                                                                                                                                                          
    private float _currentOffsetY;  
    
    private const int LookAheadRadiusCells = 4;
    private const float LerpSpeed = 4f;
    private const int FovRadius = 16;
    
    private float _cameraX;                                                                                                                                                                                                      
    private float _cameraY;                                                                                                                                                                                                      
    private const float CameraSpeed = 4f;
    
    private float _currentMoveCooldown = 0f;
    private float _moveCooldown = 0.10f;
    private bool _isMoving = false;
    
    private double _fpsTimer = 0;
    private int _frameCount = 0;

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
                                                                                                                                                                                                                                 
        _cameraX = Math.Clamp(player.x * FontSize.X - (Surface.ViewWidth / 2f * FontSize.X), 0, maxPixelX);                                                                                                                          
        _cameraY = Math.Clamp(player.y * FontSize.Y - (Surface.ViewHeight / 2f * FontSize.Y), 0, maxPixelY);       
        
        _playerSurface = new ScreenSurface(1, 1)
        {
            Font = font16x16,
            UsePixelPositioning = true
        };
        _playerSurface.Surface.SetGlyph(0, 0, '@', Theme.Current.Player, Color.Transparent);
        Children.Add(_playerSurface);
        
        _fov.Compute(grid, player.x, player.y, FovRadius);
        Render();
    }

    public override void Update(TimeSpan delta)
    {
        _frameCount++;
        _fpsTimer += delta.TotalSeconds;

        if (_fpsTimer >= 1.0)
        {
            int fps = _frameCount;
            _frameCount = 0;
            _fpsTimer = 0;

            SadConsole.Game.Instance.MonoGameInstance.Window.Title = $"DAEDALUS-07 // FPS: {fps}";
        }
        
        
        GamePadState pad = Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
        Mouse mouse = SadConsole.GameHost.Instance.Mouse; 
        
        int offsetX = 0;                                                                                                                                                                                                                             
        int offsetY = 0;
        
        if (_currentMoveCooldown > 0)
        {
            _currentMoveCooldown -= (float)delta.TotalSeconds;
        }

        var keyboard = SadConsole.GameHost.Instance.Keyboard;                                                                                                                                                                        
        var (kdx, kdy) = GetKeyboardMovement(keyboard);                                                                                                                                                                              
        var (pdx, pdy) = pad.IsConnected ? GetPadMovement(pad) : (0, 0);                                                                                                                                                             
                                                                                                                                                                                                                                     
        int moveX = kdx != 0 ? kdx : pdx;                                                                                                                                                                                            
        int moveY = kdy != 0 ? kdy : pdy;                                                                                                                                                                                            
                                                                                                                                                                                                                                   
        if ((moveX != 0 || moveY != 0) && _currentMoveCooldown <= 0f)                                                                                                                                                                
        {                                                                                                                                                                                                                            
            if (_player.TryMove(moveX * _player.Size, moveY * _player.Size, _grid))
            {
                _fov.Compute(_grid, _player.x, _player.y, FovRadius);
                Render();
            }                                                                                                                                                                                                                 
                                                                                                                                                                                                                                     
            _currentMoveCooldown = _moveCooldown;                                                                                                                                                                                    
        }       

        if (pad.IsConnected)
        {
            (offsetX, offsetY) = GetPadOffset(pad);
        }

        if (offsetX == 0 && offsetY == 0 && mouse.IsOnScreen)                                                                                                                                                                        
        {                                                                                                                                                                                                                            
            (offsetX, offsetY) = GetMouseOffset(mouse);                                                                                                                                                                              
        }

        
        
        _currentOffsetX += (offsetX - _currentOffsetX) * (float)Math.Min(1.0,LerpSpeed * delta.TotalSeconds);
        _currentOffsetY += (offsetY - _currentOffsetY) * (float)Math.Min(1.0,LerpSpeed * delta.TotalSeconds);
        
        offsetX = (int)Math.Round(_currentOffsetX);                                                                                                                                                                          
        offsetY = (int)Math.Round(_currentOffsetY); 
        
        int maxPixelX = (_grid.Width - Surface.ViewWidth) * FontSize.X;                                                                                                                                                                              
        int maxPixelY = (_grid.Height - Surface.ViewHeight) * FontSize.Y; 
        
        float targetCamX = _player.x * FontSize.X + offsetX - (Surface.ViewWidth / 2f * FontSize.X);                                                                                                                                 
        float targetCamY = _player.y * FontSize.Y + offsetY - (Surface.ViewHeight / 2f * FontSize.Y);                                                                                                                                
        targetCamX = Math.Clamp(targetCamX, 0, maxPixelX);                                                                                                                                                                           
        targetCamY = Math.Clamp(targetCamY, 0, maxPixelY);        
        
        _cameraX += (targetCamX - _cameraX) * (float)Math.Min(1.0, CameraSpeed * delta.TotalSeconds);                                                                                                                                
        _cameraY += (targetCamY - _cameraY) * (float)Math.Min(1.0, CameraSpeed * delta.TotalSeconds);     
        
        int camPixelX = (int)Math.Round(_cameraX);                                                                                                                                                                                   
        int camPixelY = (int)Math.Round(_cameraY); 
                                                                                                                                                                                                                                                     
        int cellX = camPixelX / FontSize.X;                                                                                                                                                                                                          
        int subPixelX = camPixelX % FontSize.X; 
        
        int cellY = camPixelY / FontSize.Y;
        int subPixelY = camPixelY % FontSize.Y;
        
        Surface.ViewPosition = new Point(cellX,cellY);
        
        int px = (_player.x - cellX) * FontSize.X;
        int py = (_player.y - cellY) * FontSize.Y;
        _playerSurface.Position = new Point(px, py);
        
        Position = new Point(-subPixelX, -subPixelY); 
         
    }

    private (int x, int y) GetPadOffset(GamePadState pad)
    {
        float stickX = pad.ThumbSticks.Right.X;
        float stickY = -pad.ThumbSticks.Right.Y;

        int offsetX = 0;
        int offsetY = 0;

        float deadzone = 0.2f;
        
        if (stickX * stickX + stickY * stickY >= deadzone * deadzone)                                                                                                                                                                                
        {                                                                                                                                                                                                                                            
            offsetX = (int)Math.Round(stickX * LookAheadRadiusCells * FontSize.X);                                                                                                                                                                                                   
            offsetY = (int)Math.Round(stickY * LookAheadRadiusCells * FontSize.Y);                                                                                                                                                                                                   
        }

        return (offsetX, offsetY);
    }

    private (int x, int y) GetMouseOffset(Mouse mouse)
    {
        int centerX = (Surface.ViewWidth * FontSize.X) / 2 ;
        int centerY = (Surface.ViewHeight * FontSize.Y) / 2;
        
        double mouseVecX = mouse.ScreenPosition.X - centerX;
        double mouseVecY = mouse.ScreenPosition.Y - centerY;

        int maxRadius = LookAheadRadiusCells * FontSize.X;

        double dist = Math.Sqrt(mouseVecX * mouseVecX + mouseVecY * mouseVecY);

        if (dist > maxRadius)
        {
            mouseVecX *= (maxRadius / dist);
            mouseVecY *= (maxRadius / dist);
        }

        return ((int)mouseVecX, (int)mouseVecY);
    }

    private (int x, int y) GetKeyboardMovement(SadConsole.Input.Keyboard keyboard)                                                                                                                                               
    {                                                                                                                                                                                                                            
        int dx = 0, dy = 0;                                                                                
        
        if (keyboard.IsKeyDown(SadConsole.Input.Keys.W) || keyboard.IsKeyDown(SadConsole.Input.Keys.Up)) dy--;                                                                                                                   
        else if (keyboard.IsKeyDown(SadConsole.Input.Keys.S) || keyboard.IsKeyDown(SadConsole.Input.Keys.Down)) dy++;  
        
        if (keyboard.IsKeyDown(SadConsole.Input.Keys.A) || keyboard.IsKeyDown(SadConsole.Input.Keys.Left)) dx--;                                                                                                            
        else if (keyboard.IsKeyDown(SadConsole.Input.Keys.D) || keyboard.IsKeyDown(SadConsole.Input.Keys.Right)) dx++;     
        
        return (dx, dy);                                                                                                                                                                                                         
    }     
    
    private (int dx, int dy) GetPadMovement(GamePadState pad)
    {
        int dx = 0;
        int dy = 0;
        
        if (pad.DPad.Up == ButtonState.Pressed) dy--;
        else if (pad.DPad.Down == ButtonState.Pressed) dy++;
        
        if (pad.DPad.Left == ButtonState.Pressed) dx--;
        else if (pad.DPad.Right == ButtonState.Pressed) dx++;

        if (dx != 0 || dy != 0 || pad.ThumbSticks.Left is { X: 0, Y: 0 } ) return (dx, dy);

        switch (pad.ThumbSticks.Left.X)
        {
            case > 0.5f:
                dx++;
                break;
            case < -0.5f:
                dx--;
                break;
        }

        switch (pad.ThumbSticks.Left.Y)
        {
            case < -0.5f:
                dy++;
                break;
            case > 0.5f:
                dy--;
                break;
        }

        return (dx, dy);

    }

    public override bool ProcessKeyboard(SadConsole.Input.Keyboard keyboard)
    {
        return base.ProcessKeyboard(keyboard);
    }

    private bool IsRoofAt(int x, int y)
    {
        if (!_grid.IsInBounds(x, y + 3)) return false;

        return _grid[x, y].Type == SubnetNodeType.Wall &&
               _grid[x, y + 1].Type == SubnetNodeType.Wall &&
               _grid[x, y + 2].Type == SubnetNodeType.Wall &&
               _grid[x, y + 3].Type == SubnetNodeType.Floor;
    }
    
    private int GetGlyph(SubnetNode node, bool isFrontWall, bool isRoof)
    {
        return node.Type switch
        {
            SubnetNodeType.Void => ' ',
            SubnetNodeType.Floor => '.',
            SubnetNodeType.Wall => 219,
            _ => ' '
        };
    }
    
    private Color GetForeground(SubnetNode node, bool isFrontWall, bool isRoof, float intensity,bool isLit)
    {
        if (node.Type == SubnetNodeType.Floor)
        {
            if (!node.IsSight) return Theme.Current.FloorFog;
            return Color.Lerp(Theme.Current.FloorFog, Theme.Current.FloorLit, intensity);
        }
        if (node.Type == SubnetNodeType.Wall)
        {
            Color bright = isFrontWall ? Theme.Current.WallFront : Theme.Current.WallRoof;
            Color dim = Theme.Current.WallDim;
            if (!isLit) return dim;
            return Color.Lerp(dim, bright, intensity);
        }

        return Color.White;
    }

    public void Render()
    {
        int startX = Math.Max(0, _player.x - FovRadius - 4);
        int endX = Math.Min(_grid.Width - 1, _player.x + FovRadius + 4);

        int startY = Math.Max(0, _player.y - FovRadius - 4);
        int endY = Math.Min(_grid.Height - 1, _player.y + FovRadius + 4);
        
        int glyph = ' ';
        Color foreground = Color.White;
        for (int x = startX; x < endX; x++)
        {
            for (int y = startY; y < endY; y++)
            {
                glyph = ' ';
                foreground = Color.White;
                
                var node = _grid[x, y];
                
                bool isLowerFacade = _grid.IsInBounds(x, y + 1) && 
                                     _grid[x, y + 1].Type == SubnetNodeType.Floor;

                bool isUpperFacade = _grid.IsInBounds(x, y + 2) && 
                                     _grid[x, y + 1].Type == SubnetNodeType.Wall && 
                                     _grid[x, y + 2].Type == SubnetNodeType.Floor;

                bool isFrontWall = node.Type == SubnetNodeType.Wall && (isLowerFacade || isUpperFacade);
                
                bool hasSideWallAbove = _grid.IsInBounds(x, y - 1) && 
                                        _grid[x, y - 1].Type == SubnetNodeType.Wall &&
                                        ((_grid.IsInBounds(x - 1, y - 1) && _grid[x - 1, y - 1].Type == SubnetNodeType.Floor) ||
                                         (_grid.IsInBounds(x + 1, y - 1) && _grid[x + 1, y - 1].Type == SubnetNodeType.Floor));
                
                
                
                bool isRoof = !hasSideWallAbove && node.Type == SubnetNodeType.Wall && !isFrontWall && (
                    IsRoofAt(x, y) || 
                    (_grid.IsInBounds(x - 1, y) && IsRoofAt(x - 1, y)) || 
                    (_grid.IsInBounds(x + 1, y) && IsRoofAt(x + 1, y))
                );
                
                bool isSideWallAbove = node.Type == SubnetNodeType.Wall && !isFrontWall && !isRoof &&
                                       _grid.IsInBounds(x, y + 1) && _grid[x, y + 1].IsSight;
                
                
                bool isLit = node.IsSight || 
                             (isUpperFacade && _grid.IsInBounds(x, y + 1) && _grid[x, y + 1].IsSight) ||
                             (isRoof && _grid.IsInBounds(x, y + 2) && _grid[x, y + 2].IsSight) ||
                             isSideWallAbove;
                
                bool isDiscovered = node.IsDiscovered || isLit ||
                                    (isUpperFacade && _grid.IsInBounds(x, y + 1) && _grid[x, y + 1].IsDiscovered) ||
                                    (isRoof && _grid.IsInBounds(x, y + 2) && _grid[x, y + 2].IsDiscovered);

                if (!isDiscovered && _grid.IsInBounds(x, y + 1))
                {
                    var below = _grid[x, y + 1];
                    if (below.Type == SubnetNodeType.Wall && (below.IsDiscovered && 
                        _grid.IsInBounds(x, y + 2) && _grid[x, y + 2].Type == SubnetNodeType.Floor) || (_grid.IsInBounds(x+1,y+2) && _grid[x+1, y + 1].IsDiscovered &&  _grid[x+1, y + 2].Type == SubnetNodeType.Floor)
                        || (_grid.IsInBounds(x-1,y+2) && _grid[x-1, y + 1].IsDiscovered &&  _grid[x-1, y + 2].Type == SubnetNodeType.Floor))
                    {
                        isDiscovered = true;
                    }
                }
                
                float intensity = 0f;
                if (isLit)
                {
                    float dx = x - _player.x;
                    float dy = y - _player.y;
                    float distance = MathF.Sqrt(dx * dx + dy * dy);
                    float light = 1f - Math.Clamp(distance / FovRadius, 0f, 1f);
                    intensity = light * light;
                }
                
                if (isDiscovered)
                {
                    glyph = GetGlyph(node, isFrontWall, isRoof);
                    foreground = GetForeground(node, isFrontWall, isRoof,intensity,isLit);
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