using DAEDALUS07.Core.Entities;
using SadConsole;
using DAEDALUS07.Core.Grid;
using SadRogue.Primitives;

namespace DAEDALUS07.Client.Rendering;

public class SubnetRenderer : ScreenSurface
{
    private readonly SubnetGrid _grid;
    private Entity _player;
        

    public SubnetRenderer(SubnetGrid grid, Entity player) : base(grid.Width, grid.Height)
    {
        _grid = grid;
        
        UseKeyboard = true;
        IsFocused = true;
        _player = player;
        Render();
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
                        SubnetNodeType.Floor => node.IsSight ? Color.White : Color.LightGray,
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