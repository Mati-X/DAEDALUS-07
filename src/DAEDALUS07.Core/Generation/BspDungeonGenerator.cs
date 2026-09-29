using DAEDALUS07.Core.Grid;
using DAEDALUS07.Core.Primitives;

namespace DAEDALUS07.Core.Generation;

public class BspDungeonGenerator
{
    public static void CreateHorizontalTunnel(SubnetGrid grid, int xStart, int xEnd, int y)
    {
        for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd); x++)
        {
            grid[x, y] = new SubnetNode(true, true, SubnetNodeType.Floor);
        }
    }

    public static void CreateVerticalTunnel(SubnetGrid grid, int yStart, int yEnd, int x)
    {
        for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd); y++)
        {
            grid[x, y] = new SubnetNode(true, true, SubnetNodeType.Floor);
        }
    }

    public static void CreateLShapeTunnel(SubnetGrid grid, (int x, int y) start, (int x, int y) end, bool mirrored = false)
    {
        if (mirrored)
        {
            CreateVerticalTunnel(grid, start.y, end.y, start.x);
            CreateHorizontalTunnel(grid, start.x, end.x, end.y);
            return;
        }

        CreateHorizontalTunnel(grid, start.x, end.x, start.y);
        CreateVerticalTunnel(grid, start.y, end.y, end.x);
    }

    private static (int x, int y) GetRandomPointInRoom(Rect room, Random random)
    {
        return (
            random.Next(room.Left + 1, room.Right - 1),
            random.Next(room.Top + 1, room.Bottom - 1)
        );
    }

    private static void ConnectPoints(SubnetGrid grid, (int x, int y) point1, (int x, int y) point2, Random random)
    {
        switch (random.Next(3))
        {
            case 0:
                CreateLShapeTunnel(grid, point1, point2, mirrored: false);
                break;
            case 1:
                CreateLShapeTunnel(grid, point1, point2, mirrored: true);
                break;
            case 2:
                int midX = (point1.x + point2.x) / 2;
                CreateHorizontalTunnel(grid, point1.x, midX, point1.y);
                CreateVerticalTunnel(grid, point1.y, point2.y, midX);
                CreateHorizontalTunnel(grid, midX, point2.x, point2.y);
                break;
        }
    }
}