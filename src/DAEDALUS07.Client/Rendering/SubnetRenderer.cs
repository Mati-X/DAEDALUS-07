using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Fov;
using SadConsole;
using DAEDALUS07.Core.Grid;
using SadRogue.Primitives;

namespace DAEDALUS07.Client.Rendering;

public class SubnetRenderer : ScreenSurface
{
    private readonly SubnetGrid _grid;
    private readonly ShadowcastFov _fov = new();
    private Entity _player;
        

    public SubnetRenderer(SubnetGrid grid, Entity player) : base(100,35, grid.Width, grid.Height)
    {
        _grid = grid;
        
        UseKeyboard = true;
        UseMouse = true;
        IsFocused = true;
        _player = player;
        _fov.Compute(grid, player.x, player.y, 8);
        Render();
    }

    public override void Update(TimeSpan delta)
    {
        var pad = Microsoft.Xna.Framework.Input.GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
        
        if(!pad.IsConnected) return;
        
        float stickX = pad.ThumbSticks.Right.X;
        float stickY = -pad.ThumbSticks.Right.Y;

        float deadzone = 0.2f;
        
        int offsetX = 0;                                                                                                                                                                                                                             
        int offsetY = 0; 
        
        if (stickX * stickX + stickY * stickY >= deadzone * deadzone)                                                                                                                                                                                
        {                                                                                                                                                                                                                                            
            offsetX = (int)Math.Round(stickX * 8);                                                                                                                                                                                                   
            offsetY = (int)Math.Round(stickY * 8);                                                                                                                                                                                                   
        }     
        
        int targetX = _player.x + offsetX - (Surface.ViewWidth / 2);
        int targetY = _player.y + offsetY - (Surface.ViewHeight / 2);
    
        Surface.ViewPosition = new Point(
            Math.Clamp(targetX, 0, _grid.Width - Surface.ViewWidth),
            Math.Clamp(targetY, 0, _grid.Height - Surface.ViewHeight)
        );
    }

    public override bool ProcessKeyboard(SadConsole.Input.Keyboard keyboard)
    {
        int dx = 0;
        int dy = 0;
        
        if(keyboard.IsKeyPressed(SadConsole.Input.Keys.Up) || keyboard.IsKeyPressed(SadConsole.Input.Keys.W)) dy--;
        else if (keyboard.IsKeyPressed(SadConsole.Input.Keys.Down) || keyboard.IsKeyPressed(SadConsole.Input.Keys.S)) dy++;
        else if (keyboard.IsKeyPressed(SadConsole.Input.Keys.Left) || keyboard.IsKeyPressed(SadConsole.Input.Keys.A)) dx--;
        else if (keyboard.IsKeyPressed(SadConsole.Input.Keys.Right) || keyboard.IsKeyPressed(SadConsole.Input.Keys.D)) dx++;

        if (dx != 0 || dy != 0)
        {
            if (_player.TryMove(dx, dy, _grid))
            {
                _fov.Compute(_grid, _player.x, _player.y, 8);
                Render();
                return true;
            }
        }
        
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
        Surface.SetGlyph(_player.x , _player.y, '@', Color.Yellow, Color.Black);
        IsDirty = true;
    }
}