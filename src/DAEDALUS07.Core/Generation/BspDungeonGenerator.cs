using DAEDALUS07.Core.Entities;
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
                int roomW = random.Next(minSize - 2, leaf.Bounds.Width - 2);
                int roomH = random.Next(minSize - 2, leaf.Bounds.Height - 2);
                int roomX = random.Next(leaf.Bounds.X + 1, leaf.Bounds.Right - roomW);
                int roomY = random.Next(leaf.Bounds.Y + 1, leaf.Bounds.Bottom - roomH);
                
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
            return (rooms, rooms[0].Center, enemies);
    }
    
}