namespace DAEDALUS07.Core.Grid;

public struct SubnetNode
{
    public bool IsPassable;
    public bool IsTransparent;
    public bool IsDiscovered;
    public bool IsSight;

    public SubnetNode(bool isPassable, bool isTransparent)
    {
        IsPassable = isPassable;
        IsTransparent = isTransparent;
        IsDiscovered = false;
        IsSight = false;
    }
}