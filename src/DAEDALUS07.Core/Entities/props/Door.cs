namespace DAEDALUS07.Core.Entities.props;

public enum DoorState
{
    Closed,
    Open,
    Locked,
    Lockdown,
    Broken
}

public class Door : Prop
{
    private static char GetGlyphForState(DoorState state) => state switch
    {
        DoorState.Open => '/',
        DoorState.Closed => '+',
        DoorState.Locked => '=',
        DoorState.Lockdown => 'X',
        DoorState.Broken => '_',
        _ => '+'
    };
    
    DoorState _state = DoorState.Closed;
    
    public Door(int x, int y, DoorState initialState = DoorState.Closed, int maxIntegrity = 50)
        : base(x, y, "Door", '+', maxIntegrity)
    {
        _state = initialState;
        Glyph = GetGlyphForState(initialState);
    }
}