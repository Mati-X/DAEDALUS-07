namespace DAEDALUS07.Core.Primitives;

public readonly struct Rect
{
    public int X { get; }                                                                                                                                                                                                                    
    public int Y { get; }                                                                                                                                                                                                                    
    public int Width { get; }                                                                                                                                                                                                                
    public int Height { get; }       
    
    public int Left => X;
    public int Right => X + Width;
    public int Top => Y;
    public int Bottom => Y + Height;
    
    public (int x, int y) Center => (X + Width / 2, Y + Height / 2);
    
    public Rect(int x, int y, int width, int height)
    {
        X = x;
        Y = y;
        Width = width;
        Height = height;
    }
}