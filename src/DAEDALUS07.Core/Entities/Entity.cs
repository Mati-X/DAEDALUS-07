using DAEDALUS07.Core.Grid;

namespace DAEDALUS07.Core.Entities;

public class Entity
{
    public int x { get; set; }
    public int y { get; set; }
    public string Name { get; }
    public char Glyph { get; set; } 
    public int MaxIntegrity { get; set; } 
    public int Integrity { get; set; }
    public int Size { get; set; } = 1;
    public bool IsAlive => Integrity > 0;

    public bool TryMove(int dx, int dy, SubnetGrid grid)
    {
        int targetX = x + dx;
        int targetY = y + dy;

        int feetY = targetY + (Size - 1);

        for (int cx = 0; cx < Size; cx++)
        {
            int checkX = targetX + cx;
            if (!grid.IsInBounds(checkX, feetY) || !grid[checkX, feetY].IsPassable)
                return false;
        }

        x = targetX;
        y = targetY;
        return true;
    }
    
    public Entity(int x, int y, string name, char glyph = '@', int maxIntegrity = 100) 
    {
        this.x = x;
        this.y = y;
        Name = name;
        Glyph = glyph;
        MaxIntegrity = maxIntegrity;
        Integrity = maxIntegrity;
    }
}