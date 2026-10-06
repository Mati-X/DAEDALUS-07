

using SadRogue.Primitives;

namespace DAEDALUS07.Core.Grid;

public class PointLight(int x, int y, int radius, Color color)
{
    public int X = x;
    public int Y = y;
    public int Radius = radius;
    public Color Color = color;
    public bool IsActive = true;
}