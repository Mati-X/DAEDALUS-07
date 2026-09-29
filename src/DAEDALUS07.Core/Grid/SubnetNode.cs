namespace DAEDALUS07.Core.Grid;

public struct SubnetNode
{
    public bool IsPassable;
    public bool IsTransparent;
    public bool IsDiscovered;
    public bool IsSight;
    public SubnetNodeType Type;

    public SubnetNode(bool isPassable, bool isTransparent, SubnetNodeType type = SubnetNodeType.Void) : this()
    {
        IsPassable = isPassable;
        IsTransparent = isTransparent;
        IsDiscovered = false;
        IsSight = false;
        Type = type;
    }
}