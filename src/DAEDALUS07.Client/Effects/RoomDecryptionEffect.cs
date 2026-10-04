using DAEDALUS07.Core.Generation;
using DAEDALUS07.Core.Grid;
using SadConsole;
using SadRogue.Primitives;

namespace DAEDALUS07.Client.Effects;

public class RoomDecryptionEffect
{
    private static readonly char[] GlitchChars = ['0', '1', 'A', 'F', 'X', '%', '#', '▓', '░', '7'];
    private readonly Random _random = new();

    public bool IsActive { get; private set; }
    private Room? _room;
    private int _originX;
    private int _originY;
    private float _currentRadius;
    private float _maxRadius;
    private const float Speed = 35f;

    public void Trigger(Room room, int originX, int originY)
    {
        _room = room;
        _originX = originX;
        _originY = originY;
        _currentRadius = 0f;

        // Wyliczamy dystans do najdalszego rogu pokoju:
        int maxDx = Math.Max(Math.Abs(room.Bounds.Left - originX), Math.Abs(room.Bounds.Right - originX));
        int maxDy = Math.Max(Math.Abs(room.Bounds.Top - originY), Math.Abs(room.Bounds.Bottom - originY));
        _maxRadius = MathF.Sqrt(maxDx * maxDx + maxDy * maxDy);

        IsActive = true;
    }

    public void Update(TimeSpan delta, SubnetGrid grid, ScreenSurface surface, Action onComplete)
    {
        if (!IsActive || _room == null) return;

        _currentRadius += Speed * (float)delta.TotalSeconds;

        for (int x = _room.Bounds.Left; x < _room.Bounds.Right; x++)
        {
            for (int y = _room.Bounds.Top; y < _room.Bounds.Bottom; y++)
            {
                if (!_room.Contains(x, y) || !grid.IsInBounds(x, y)) continue;

                float dist = MathF.Sqrt((x - _originX) * (x - _originX) + (y - _originY) * (y - _originY));

                if (dist <= _currentRadius)
                {
                    grid[x, y].IsDiscovered = true;

                    if (dist >= _currentRadius - 2.5f)
                    {
                        char glitch = GlitchChars[_random.Next(GlitchChars.Length)];
                        surface.SetGlyph(x, y, glitch, Color.Cyan, Color.Black);
                    }
                    else
                    {
                        int glyph = grid[x, y].Type == SubnetNodeType.Floor ? '.' : '#';
                        Color color = grid[x, y].Type == SubnetNodeType.Floor 
                            ? Theme.Current.FloorLit 
                            : Theme.Current.WallFront;
                        surface.SetGlyph(x, y, glyph, color, Color.Black);
                    }
                }
            }
        }

        if (_currentRadius >= _maxRadius + 2.5f)
        {
            foreach (var (dx, dy) in _room.Doors)
            {
                grid[dx, dy] = new SubnetNode(true, true, SubnetNodeType.Floor);
            }

            IsActive = false;
            onComplete?.Invoke();
        }

        surface.IsDirty = true;
    }
}