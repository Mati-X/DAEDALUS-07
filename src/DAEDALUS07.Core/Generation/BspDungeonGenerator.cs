using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Grid;
using DAEDALUS07.Core.Primitives;
using SadRogue.Primitives;

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

    private static (int x, int y) GetRandomPointInRoom(Room room, Random random)
    {
        if (room.CircleBounds.HasValue)
        {
            var circle = room.CircleBounds.Value;
            return (circle.CenterX, circle.CenterY);
        }

        int rx = random.Next(room.Bounds.Left + 1, room.Bounds.Right - 2);
        int ry = random.Next(room.Bounds.Top + 1, room.Bounds.Bottom - 2);
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

    private static Room GetRoomFromSubtree(BspNode node, Random random)
    {
        if (node.IsLeaf) return node.Room!;
        if (random.Next(2) == 0) return GetRoomFromSubtree(node.Left!, random);
        return GetRoomFromSubtree(node.Right!, random);
    }

    private static void ConnectNodes(SubnetGrid grid, BspNode node, Random random)
    {
        if (node.IsLeaf) return;
        
        ConnectNodes(grid, node.Left!, random);
        ConnectNodes(grid, node.Right!, random);
        
        Room leftRoom = GetRoomFromSubtree(node.Left!, random);
        Room rightRoom = GetRoomFromSubtree(node.Right!, random);
        
        if (!leftRoom.ConnectedRooms.Contains(rightRoom))
        {
            (int, int) leftPoint = GetRandomPointInRoom(leftRoom, random);
            (int, int) rightPoint = GetRandomPointInRoom(rightRoom, random);
            ConnectPoints(grid, leftPoint, rightPoint, random);
            leftRoom.ConnectedRooms.Add(rightRoom);
            rightRoom.ConnectedRooms.Add(leftRoom);
        }
    }
    
    private static void DetectDoors(SubnetGrid grid, List<Room> rooms)
    {
        foreach (var room in rooms)
        {
            for (int x = room.Bounds.Left; x < room.Bounds.Right; x++)
            {
                for (int y = room.Bounds.Top; y < room.Bounds.Bottom; y++)
                {
                    if (!room.Contains(x, y) || grid[x, y].Type != SubnetNodeType.Floor)
                        continue;

                    (int dx, int dy)[] neighbors = [(1, 0), (-1, 0), (0, 1), (0, -1)];
                    foreach (var (ndx, ndy) in neighbors)
                    {
                        int nx = x + ndx, ny = y + ndy;
                        if (grid.IsInBounds(nx, ny) && 
                            grid[nx, ny].Type == SubnetNodeType.Floor && 
                            !room.Contains(nx, ny))
                        {
                            room.Doors.Add((x, y));
                            break;
                        }
                    }
                }
            }
        }
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

    public static (List<Room> rooms, (int x, int y) playerSpawn, List<Enemy> enemies) Generate(SubnetGrid grid, int minSize, int maxSplits,
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
        
        
            List<Room> rooms = [];   
            
            var leaves = nodes.Where(n => n.IsLeaf).ToList();
            for (int i = 0; i < leaves.Count; i++)
            {
                var leaf = leaves[i];
                Room room;

                if (i == 0)
                {
                    int roomW = 8, roomH = 8;
                    int roomX = leaf.Bounds.X + (leaf.Bounds.Width - roomW) / 2;
                    int roomY = leaf.Bounds.Y + (leaf.Bounds.Height - roomH) / 2;
                    roomX -= roomX % 2; roomY -= roomY % 2;

                    room = new Room(new Rect(roomX, roomY, roomW, roomH), RoomType.Spawn);
                    grid.CreateRoom(room.Bounds.Width, room.Bounds.Height, room.Bounds.X, room.Bounds.Y);
                }
                else if (i == leaves.Count - 1)
                {
                    int radius = Math.Min(leaf.Bounds.Width, leaf.Bounds.Height) / 3;
                    radius -= radius % 2;
                    var c = leaf.Bounds.Center;
                    Circle circle = new Circle(c.x - (c.x % 2), c.y - (c.y % 2), radius);

                    room = new Room(circle, RoomType.BossVault);

                    for (int x = circle.CenterX - circle.Radius; x <= circle.CenterX + circle.Radius; x++)
                    for (int y = circle.CenterY - circle.Radius; y <= circle.CenterY + circle.Radius; y++)
                        if (circle.Contains(x, y))
                            grid[x, y] = new SubnetNode(true, true, SubnetNodeType.Floor);
                }
                else
                {
                    int minW = Math.Min(minSize / 2, leaf.Bounds.Width - 4);
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


                    Rect roomRect = new Rect(roomX, roomY, roomW, roomH);
                    room = new Room(roomRect, RoomType.Combat);
                    grid.CreateRoom(room.Bounds.Width, room.Bounds.Height, room.Bounds.X, room.Bounds.Y);
                }

                leaf.Room = room;
                
                var center = room.Bounds.Center;
                grid[center.x, center.y] = new SubnetNode(false, true, SubnetNodeType.ServerTerminal);

                var mainLight = new PointLight(center.x, center.y, radius: 9, Color.Cyan)
                {
                    IsActive = (room.Type == RoomType.Spawn)
                };
                room.Lights.Add(mainLight);
                
                int[] cornerXs = [room.Bounds.Left + 1, room.Bounds.Right - 2];
                int[] cornerYs = [room.Bounds.Top + 1, room.Bounds.Bottom - 2];

                foreach (int cx in cornerXs)
                {
                    foreach (int cy in cornerYs)
                    {
                        room.Lights.Add(new PointLight(cx, cy, radius: 4, Color.Goldenrod)
                        {
                            IsActive = (room.Type == RoomType.Spawn)
                        });
                    }
                }
                
                rooms.Add(room);
            }
           
            
            List<Enemy> enemies = [];                                                                                                                                                                                                 
            foreach (var room in rooms.Where(r => r.Type != RoomType.Spawn))
            {
                var (ex, ey) = GetRandomPointInRoom(room, random);
                // var enemy = new Enemy(ex, ey, "MINOS_DAEMON", 'D', 30, 12, 1, 2, 5);
                //
                // enemies.Add(enemy);
                // room.Enemies.Add(enemy);
            }   

            ConnectNodes(grid, root, random);
            DetectDoors(grid, rooms);
            var spawn = rooms[0].Bounds.Center;
            var alignedSpawn = (spawn.x - (spawn.x % 2), spawn.y - (spawn.y % 2));
            BakeWalls(grid);
            return (rooms, alignedSpawn, enemies);
    }
    
}