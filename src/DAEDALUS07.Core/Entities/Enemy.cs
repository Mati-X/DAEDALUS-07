using DAEDALUS07.Core.Grid;

namespace DAEDALUS07.Core.Entities;

public class Enemy : Entity
{
    public int MovesPerTurn { get; set; } = 1;
    public int AttackRadius { get; set; } = 1;
    public int AttackDamage { get; set; } = 10;
    public int VisionRadius { get; set; } = 12;
    public Enemy(int x, int y, string name, char glyph, int maxIntegrity, int visionRadius, int attackRadius, int movesPerTurn, int attackDamage) 
        : base(x, y, name, glyph, maxIntegrity)
    {
        MovesPerTurn = movesPerTurn;
        AttackDamage = attackDamage;
        AttackRadius = attackRadius;
        VisionRadius = visionRadius;
    }
    
    public virtual void TakeTurn(Entity player, SubnetGrid grid)
    {
        float distance = MathF.Sqrt(MathF.Pow(player.x - x, 2) + MathF.Pow(player.y - y, 2));
        bool canSeePlayer = grid[x, y].IsSight && distance <= VisionRadius;
        
        if (!canSeePlayer) return;
        
        for (int m = 0; m < MovesPerTurn; m++)
        {
            int distX = Math.Max(0, Math.Max(player.x - (x + Size - 1), x - (player.x + player.Size - 1)));
            int distY = Math.Max(0, Math.Max(player.y - (y + Size - 1), y - (player.y + player.Size - 1)));

            if (distX <= AttackRadius && distY <= AttackRadius)
            {
                player.Integrity -= AttackDamage;
                break;
            }
            
            int stepX = Math.Sign(player.x - x);
            int stepY = Math.Sign(player.y - y);
            
            if (TryMove(stepX,stepY,grid)) continue;
            if (stepX != 0 && TryMove(stepX,0,grid)) continue;
            if (stepY != 0 && TryMove(0,stepY,grid)) continue;
            
        }
    }
}