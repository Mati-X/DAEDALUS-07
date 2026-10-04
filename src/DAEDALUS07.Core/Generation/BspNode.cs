using DAEDALUS07.Core.Primitives;

namespace DAEDALUS07.Core.Generation;

public class BspNode
{
    public Rect Bounds { get; }
    public BspNode? Left { get; private set; }
    public BspNode? Right { get; private set; }
    public Room? Room { get; set; }
    public bool IsLeaf => Left == null && Right == null;

    public BspNode(Rect bounds)
    {
        Bounds = bounds;
        Left = null;
        Right = null;
        Room = null;
    }

    public bool Split(int minSize, Random random)
    {
        if(!IsLeaf) return false;
        bool isVertical = random.NextDouble() < 0.5;
        
        if (Bounds.Width > Bounds.Height * 1.25)
        {
            isVertical = true;
        }
        else if (Bounds.Height > Bounds.Width * 1.25)
        {
            isVertical = false;
        }
        
        int max = (isVertical ? Bounds.Width : Bounds.Height) - minSize; 
        if (max <= minSize)
            return false;
        
        int split = random.Next(minSize, max);
        split -= split % 2;

        if (isVertical)                                                                                                                                                                                                                      
        {                                                                                                                                                                                                                                    
            Left = new BspNode(new Rect(Bounds.X, Bounds.Y, split, Bounds.Height));                                                                                                                                                          
            Right = new BspNode(new Rect(Bounds.X + split, Bounds.Y, Bounds.Width - split, Bounds.Height));                                                                                                                                  
        }                                                                                                                                                                                                                                    
        else                                                                                                                                                                                                                                 
        {                                                                                                                                                                                                                                    
            Left = new BspNode(new Rect(Bounds.X, Bounds.Y, Bounds.Width, split));                                                                                                                                                           
            Right = new BspNode(new Rect(Bounds.X, Bounds.Y + split, Bounds.Width, Bounds.Height - split));                                                                                                                                  
        }                                                                                                                                                                                                                                    
                                                                                                                                                                                                                                                 
        return true;
    }
}