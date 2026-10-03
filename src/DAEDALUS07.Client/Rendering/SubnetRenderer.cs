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
        
    private float _currentOffsetX;                                                                                                                                                                                          
    private float _currentOffsetY;  
    
    private const int LookAheadRadiusCells = 4;
    private const float LerpSpeed = 4f;
    
    private float _cameraX;                                                                                                                                                                                                      
    private float _cameraY;                                                                                                                                                                                                      
    private const float CameraSpeed = 2f;
    
    private float _currentMoveCooldown = 0f;
    private float _moveCooldown = 0.12f;
    private bool _isMoving = false;

    public SubnetRenderer(SubnetGrid grid, Entity player, List<Entity> enemies) : base(58,37, grid.Width, grid.Height)
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
        
        _fov.Compute(grid, player.x, player.y, 8);
        Render();
    }

    public override void Update(TimeSpan delta)
    {
        
        
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
            if (_player.TryMove(moveX, moveY, _grid))                                                                                                                                                                                
            {                                                                                                                                                                                                                        
                _fov.Compute(_grid, _player.x, _player.y, 8);                                                                                                                                                                        
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

    public void Render()
    {
        char glyph = ' ';
        Color foreground = Color.White;
        for (int x = 0; x < _grid.Width; x++)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
                glyph = ' ';
                foreground = Color.White;
                
                var node = _grid[x, y];
                if (node.IsDiscovered)
                {
                    glyph = node.Type switch
                    {
                        SubnetNodeType.Void => ' ',
                        SubnetNodeType.Floor => '.',
                        SubnetNodeType.Wall => '#',
                        _ => ' '
                    };
                    foreground = node.Type switch
                    {
                        SubnetNodeType.Floor => node.IsSight ? Color.White : Color.DarkSlateGray,
                        SubnetNodeType.Wall => node.IsSight ? Color.Red : Color.DarkRed,
                        _ => Color.White
                    };
                }
                Surface.SetGlyph(x, y, glyph, foreground, Color.Black);                                                                                                                                                                                                                                                                             
            }
            
        }

        foreach (var enemy in  _enemies)
        {
            if(!enemy.IsAlive) continue;
            if(!_grid[enemy.x,enemy.y].IsSight) continue;
            Surface.SetGlyph(enemy.x, enemy.y, enemy.Glyph, Color.Red, Color.Black); 
        }
        
        Surface.SetGlyph(_player.x , _player.y, '@', Color.Yellow, Color.Black);
        IsDirty = true;
    }
}