using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Grid;
using DAEDALUS07.Core.Primitives;

namespace DAEDALUS07.Core.Generation;

public class BspDungeonGenerator

{
    const int roomMargin = 4;
    
    public static void CreateHorizontalTunnel(SubnetGrid grid, int xStart, int xEnd, int y)
    {
        
        for (int x = Math.Min(xStart, xEnd); x <= Math.Max(xStart, xEnd) + 1; x++)
        {
            grid[x, y] = new SubnetNode(true, true, SubnetNodeType.Floor);
            grid[x, y + 1] = new SubnetNode(true, true, SubnetNodeType.Floor);
        }
    }

    public static void CreateVerticalTunnel(SubnetGrid grid, int yStart, int yEnd, int x)
    {
        for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd) + 1; y++)
        {
            grid[x, y] = new SubnetNode(true, true, SubnetNodeType.Floor);
            grid[x + 1, y] = new SubnetNode(true, true, SubnetNodeType.Floor);
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
        int rx = random.Next(room.Left + 1, room.Right - 2);
        int ry = random.Next(room.Top + 1, room.Bottom - 2);

        return (rx - (rx % 2), ry - (ry % 2));
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
                midX -= midX % 2;
                CreateHorizontalTunnel(grid, point1.x, midX, point1.y);
                CreateVerticalTunnel(grid, point1.y, point2.y, midX);
                CreateHorizontalTunnel(grid, midX, point2.x, point2.y);
                break;
        }
    }

    private static Rect GetRoomFromSubtree(BspNode node, Random random)
    {
        if (node.IsLeaf) return node.Room!.Value;
        if (random.Next(2) == 0) return GetRoomFromSubtree(node.Left!, random);
        return GetRoomFromSubtree(node.Right!, random);
    }

    private static void ConnectNodes(SubnetGrid grid, BspNode node, Random random)
    {
        if (node.IsLeaf) return;
        
        ConnectNodes(grid, node.Left!, random);
        ConnectNodes(grid, node.Right!, random);
        
        Rect leftRoom = GetRoomFromSubtree(node.Left!, random);
        Rect rightRoom = GetRoomFromSubtree(node.Right!, random);
        
        (int,int) leftPoint = GetRandomPointInRoom(leftRoom, random);
        (int,int) rightPoint = GetRandomPointInRoom(rightRoom, random);
        
        ConnectPoints(grid, leftPoint, rightPoint, random);
    }
    
    public static void BakeWalls(SubnetGrid grid)
    {
        List<(int x, int y)> floors = [];
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                if (grid[x, y].Type == SubnetNodeType.Floor)
                    floors.Add((x, y));
                else
                    grid[x, y] = new SubnetNode(false, false, SubnetNodeType.Void);
            }
        }

        foreach (var (fx, fy) in floors)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -3; dy <= 1; dy++)
                {
                    int wx = fx + dx, wy = fy + dy;
                    if (!grid.IsInBounds(wx, wy) || grid[wx, wy].Type == SubnetNodeType.Floor) continue;
                    grid[wx, wy] = new SubnetNode(false, false, SubnetNodeType.WallRoof);
                }
            }
        }

        foreach (var (fx, fy) in floors)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -2; dy <= 0; dy++)
                {
                    int wx = fx + dx, wy = fy + dy;
                    if (!grid.IsInBounds(wx, wy) || grid[wx, wy].Type == SubnetNodeType.Floor) continue;
                    grid[wx, wy] = new SubnetNode(false, false, SubnetNodeType.WallFront);
                }
            }
        }
    }

    public static (List<Rect> rooms, (int x, int y) playerSpawn, List<Entity> enemies) Generate(SubnetGrid grid, int minSize, int maxSplits,
        Random random, int padding)
    {
        BspNode root = new BspNode(new Rect(padding, padding, grid.Width - 2 * padding, grid.Height - 2 * padding));
        List<BspNode> nodes = [root];

        for (int i = 0; i < maxSplits; i++)
        {
            foreach (var node in nodes.ToList())
            {
                if (node.IsLeaf && node.Split(minSize, random))
                {
                    nodes.Add(node.Left!);
                    nodes.Add(node.Right!);
                }
            }
        }
        
            List<Rect> rooms = [];   
            
            foreach (var leaf in nodes.Where(n => n.IsLeaf))
            {
                int minW = Math.Min(minSize / 2, leaf.Bounds.Width - 4 );
                int maxW = Math.Max(minW, leaf.Bounds.Width - roomMargin * 2);
                int roomW = (minW >= maxW) ? minW : random.Next(minW, maxW);
                roomW -= roomW % 2;

                int minH = Math.Min(minSize / 2, leaf.Bounds.Height - 4);
                int maxH = Math.Max(minH, leaf.Bounds.Height - roomMargin * 2);
                int roomH = (minH >= maxH) ? minH : random.Next(minH, maxH);
                roomH -= roomH % 2;

                int minX = leaf.Bounds.X + roomMargin;
                int maxX = Math.Max(minX, leaf.Bounds.Right - roomW - roomMargin);
                int roomX = (minX >= maxX) ? minX : random.Next(minX, maxX);
                roomX -= roomX % 2;

                int minY = leaf.Bounds.Y + roomMargin;
                int maxY = Math.Max(minY, leaf.Bounds.Bottom - roomH - roomMargin);
                int roomY = (minY >= maxY) ? minY : random.Next(minY, maxY);
                roomY -= roomY % 2;

                Rect room = new Rect(roomX, roomY, roomW, roomH);
                leaf.Room = room;
                rooms.Add(room);

                grid.CreateRoom(room.Width, room.Height, room.X, room.Y);
            }
            
            List<Entity> enemies = [];                                                                                                                                                                                                 
                foreach (var room in rooms.Skip(1))                                                                                                                                                                                        
                {                                                                                                                                                                                                                          
                    var (ex, ey) = GetRandomPointInRoom(room, random);                                                                                                                                                                     
                    enemies.Add(new Entity(ex, ey, "MINOS_DAEMON", 'D', 30));                                                                                                                                                              
                }       

            ConnectNodes(grid, root, random);
            var spawn = rooms[0].Center;
            var alignedSpawn = (spawn.x - (spawn.x % 2), spawn.y - (spawn.y % 2));
            BakeWalls(grid);
            return (rooms, alignedSpawn, enemies);
    }
    
}