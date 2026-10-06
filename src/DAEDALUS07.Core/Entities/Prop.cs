namespace DAEDALUS07.Core.Entities;

public abstract class Prop : Entity
{
    public virtual bool BlocksMovement => true;
    public virtual bool BlocksLight => false;

    public Prop(int x, int y, string name, char glyph, int maxIntegrity = 50)
        : base(x, y, name, glyph, maxIntegrity)
    {
    }

    public virtual bool Interact(Entity initiator)
    {
        return false;
    }
}