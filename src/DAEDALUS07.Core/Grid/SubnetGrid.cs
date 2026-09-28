namespace DAEDALUS07.Core.Grid;

public sealed class SubnetGrid
{
    public int Width { get; }
    public int Height { get; }
    
    private readonly SubnetNode[] _nodes;
    
    public SubnetGrid(int width, int height)
    {
        Width = width;
        Height = height;
        _nodes = new SubnetNode[width * height];
    }
    
    public ref SubnetNode this[int x, int y] => ref _nodes[GetIndex(x, y)];
    
    public int GetIndex(int x, int y) => y * Width + x;
    
    public bool IsInBounds(int x, int y) => x >= 0 && (uint)x < Width && y >= 0 && (uint)y < Height;
}