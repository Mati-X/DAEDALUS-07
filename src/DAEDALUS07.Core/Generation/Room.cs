using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Grid;
using DAEDALUS07.Core.Primitives;

namespace DAEDALUS07.Core.Generation;

public enum RoomType
{
    Spawn,
    Combat,
    BossVault
}

public class Room
{
    public Rect Bounds { get; }
    public Circle? CircleBounds { get; }
    public RoomType Type { get; }
    public bool IsCleared { get; set; }
    public bool IsActive { get; set; }
    public List<Enemy> Enemies { get; } = [];
    public List<(int x, int y)> Doors { get; } = [];
    public HashSet<Room> ConnectedRooms { get; } = [];
    public List<PointLight> Lights { get; } = [];
    public List<Rect> Segments { get; } = [];

    public Room(Rect bounds, RoomType type)
    {
        Bounds = bounds;
        Type = type;
        IsCleared = (type == RoomType.Spawn);
        Segments.Add(bounds);
    }
    
    public Room(Circle circle, RoomType type)
    {
        CircleBounds = circle;
        Bounds = new Rect(circle.CenterX - circle.Radius, circle.CenterY - circle.Radius, circle.Radius * 2, circle.Radius * 2);
        Type = type;
        IsCleared = false;
    }

    public bool Contains(int x, int y)
    {
        if (CircleBounds.HasValue)
            return CircleBounds.Value.Contains(x, y);
        return Segments.Any(s => s.Contains(x, y));
    }
}