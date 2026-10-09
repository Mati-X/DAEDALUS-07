using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Generation.Templates;
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
            grid[x, y] = new SubnetNode(SubnetNodeType.Floor);
            grid[x, y + 1] = new SubnetNode(SubnetNodeType.Floor);
        }
    }

    public static void CreateVerticalTunnel(SubnetGrid grid, int yStart, int yEnd, int x)
    {
        for (int y = Math.Min(yStart, yEnd); y <= Math.Max(yStart, yEnd) + 1; y++)
        {
            grid[x, y] = new SubnetNode(SubnetNodeType.Floor);
            grid[x + 1, y] = new SubnetNode(SubnetNodeType.Floor);
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

            var seg = room.Segments.Count > 0 
                ? room.Segments[random.Next(room.Segments.Count)] 
                : room.Bounds;

            int rx = random.Next(seg.Left + 1, Math.Max(seg.Left + 2, seg.Right - 2));
            int ry = random.Next(seg.Top + 1, Math.Max(seg.Top + 2, seg.Bottom - 2));
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
    
    private static bool isInteriorTile(SubnetNodeType nodeType)
    {
        return nodeType is SubnetNodeType.Floor 
            or SubnetNodeType.CatwalkFloor 
            or SubnetNodeType.Stairs 
            or SubnetNodeType.LowCover 
            or SubnetNodeType.Pillar 
            or SubnetNodeType.ServerTerminal;
    }
    
    public static void BakeWalls(SubnetGrid grid)
    {
        List<(int x, int y)> floors = [];
        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                bool isInterior = grid[x, y].Type is SubnetNodeType.Floor 
                    or SubnetNodeType.CatwalkFloor 
                    or SubnetNodeType.Stairs 
                    or SubnetNodeType.LowCover 
                    or SubnetNodeType.Pillar 
                    or SubnetNodeType.ServerTerminal;

                if (isInterior)
                    floors.Add((x, y));
                else
                    grid[x, y] = new SubnetNode(SubnetNodeType.Void);
            }
        }

        foreach (var (fx, fy) in floors)
        {
            for (int dx = -2; dx <= 2; dx++)
            {
                for (int dy = -3; dy <= 1; dy++)
                {
                    int wx = fx + dx, wy = fy + dy;
                    if (!grid.IsInBounds(wx, wy) || isInteriorTile(grid[wx, wy].Type)) continue;
                    grid[wx, wy] = new SubnetNode(SubnetNodeType.WallRoof);
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
                    if (!grid.IsInBounds(wx, wy) || isInteriorTile(grid[wx, wy].Type)) continue;
                    grid[wx, wy] = new SubnetNode(SubnetNodeType.WallFront);
                }
            }
        }
    }
    
    private static void StampTemplate(SubnetGrid grid, Room room, MacroRoomTemplate template, int startX, int startY, Random random)
    {
        for (int l = 0; l < template.MacroLayers.Length; l++)
        {
            for (int mr = 0; mr < template.MacroRows; mr++)
            {
                for (int mc = 0; mc < template.MacroCols; mc++)
                {
                    char moduleCode = template.GetModuleCode(l, mr, mc);
                    if (moduleCode == ' ') continue;

                    string[] variant = ModuleLibrary.GetRandomVariant(moduleCode, random);
                    int baseX = startX + (mc * ModuleLibrary.ModuleSize);
                    int baseY = startY + (mr * ModuleLibrary.ModuleSize);

                    for (int vy = 0; vy < ModuleLibrary.ModuleSize; vy++)
                    {
                        for (int vx = 0; vx < ModuleLibrary.ModuleSize; vx++)
                        {
                            char ch = variant[vy][vx];
                            int gx = baseX + vx, gy = baseY + vy;

                            switch (ch)
                            {
                                case '.':
                                    grid[gx, gy] = new SubnetNode(SubnetNodeType.Floor);
                                    break;
                                case 'O':
                                    grid[gx, gy] = new SubnetNode(SubnetNodeType.Pillar);
                                    break;
                                case '░':
                                    grid[gx, gy] = new SubnetNode(SubnetNodeType.LowCover);
                                    break;
                                case 'T':
                                    grid[gx, gy] = new SubnetNode(SubnetNodeType.ServerTerminal);
                                    break;
                                case '≡':
                                    grid[gx, gy] = new SubnetNode(SubnetNodeType.CatwalkFloor);
                                    break;
                                case '=':
                                    grid[gx, gy] = new SubnetNode(SubnetNodeType.Stairs);
                                    break;
                            }
                        }
                    }
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

                RoomType roomType = (i == 0) ? RoomType.Spawn 
                    : (i == leaves.Count - 1 ? RoomType.BossVault : RoomType.Combat);

                var template = TemplateLibrary.GetRandomTemplate(roomType, 
                    leaf.Bounds.Width - roomMargin * 2, 
                    leaf.Bounds.Height - roomMargin * 2, 
                    random);

                int roomX = leaf.Bounds.X + (leaf.Bounds.Width - template.Width) / 2;
                int roomY = leaf.Bounds.Y + (leaf.Bounds.Height - template.Height) / 2;
                roomX -= roomX % 2; roomY -= roomY % 2;

                room = new Room(new Rect(roomX, roomY, template.Width, template.Height), roomType);
                StampTemplate(grid, room, template, roomX, roomY, random);

                leaf.Room = room;
                
                var cp = room.Segments.Count > 0 ? room.Segments[0].Center : room.Bounds.Center;
                (int x, int y) center = (cp.x - (cp.x % 2), cp.y - (cp.y % 2));
                grid[center.x, center.y] = new SubnetNode(SubnetNodeType.ServerTerminal);

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
                        room.Lights.Add(new PointLight(cx, cy, radius: (int)(Math.Sqrt(Math.Abs(cx-center.x)*Math.Abs(cx-center.x)+Math.Abs(cy-center.y)*Math.Abs(cy-center.y))), Color.Goldenrod)
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