namespace DAEDALUS07.Core.Grid;

public struct SubnetNode
{
    public bool IsDiscovered;
    public bool IsSight;
    public SubnetNodeType Type;
    public bool IsPassable => Type switch
    {
        SubnetNodeType.Floor => true,
        _ => false
    };
    public bool IsTransparent => Type switch
    {
        SubnetNodeType.Floor or SubnetNodeType.LaserBarrier or SubnetNodeType.LowCover => true,
        _ => false
    };
    public bool IsShootThrough => Type switch
    {
        SubnetNodeType.Floor or SubnetNodeType.LowCover => true,
        _ => false
    };
    public SubnetNode(SubnetNodeType type = SubnetNodeType.Void) : this()
    {
        Type = type;
        IsDiscovered = false;
        IsSight = false;
    }
}