using SadConsole;
using DAEDALUS07.Core.Grid;
using SadRogue.Primitives;

namespace DAEDALUS07.Client.Rendering;

public class SubnetRenderer : ScreenSurface
{
    private readonly SubnetGrid _grid;

    public SubnetRenderer(SubnetGrid grid) : base(grid.Width, grid.Height)
    {
        _grid = grid;
        Render();
    }

    public void Render()
    {
        char glyph = ' ';
        Color foreground = Color.White;
        for (int x = 0; x < _grid.Width; x++)
        {
            for (int y = 0; y < _grid.Height; y++)
            {
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
                else
                {x
                }
                Surface.SetGlyph(x, y, glyph, foreground, Color.Black);                                                                                                                                                                                                                                                                             
            }
            
        }
        IsDirty = true;
    }
}