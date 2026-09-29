using DAEDALUS07.Core.Grid;

namespace DAEDALUS07.Core.Entities;

public class Entity
{
    public int x { get; set; }
    public int y { get; set; }
    public string Name { get; }

    public bool TryMove(int dx, int dy, SubnetGrid grid)
    {
        int targetX = x + dx;
        int targetY = y + dy;
        if (!grid.IsInBounds(targetX, targetY) || !grid[targetX, targetY].IsPassable) return false;
        this.x = targetX;
        this.y = targetY;
        return true;
            
    }
    
    public Entity(int x, int y, string name)
    {
        this.x = x;
        this.y = y;
        Name = name;
    }
}