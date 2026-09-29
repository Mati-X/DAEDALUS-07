namespace DAEDALUS07.Core.Primitives;

public readonly struct Circle
{
    public int CenterX { get; }
    public int CenterY { get; }
    public int Radius { get; }

    public Circle(int centerX, int centerY, int radius)
    {
        CenterX = centerX;
        CenterY = centerY;
        Radius = radius;
    }

    public bool Contains(int x, int y)
    {
        int dx = x- CenterX;
        int dy = y - CenterY;
        return dx*dx + dy*dy <= Radius*Radius;
    }
}