using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Grid;

namespace DAEDALUS07.Core.Systems;

public class TurnManager
{
    public void ExecuteTurn(Entity player, List<Enemy> enemies, SubnetGrid grid)
    {
        foreach (var enemy in enemies)
        {
            if (!enemy.IsAlive) continue;
            enemy.TakeTurn(player,grid);
        }
    }
}