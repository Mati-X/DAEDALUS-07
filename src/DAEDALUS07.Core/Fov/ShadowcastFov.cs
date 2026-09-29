using DAEDALUS07.Core.Grid;

namespace DAEDALUS07.Core.Fov;

public class ShadowcastFov
{
    private static (int x, int y) TransformOctant(int octant, int originX, int originY, int row, int col)
    {
        return octant switch
        {
            0 => (originX + row, originY + col),
            1 => (originX + col, originY + row),
            2 => (originX - col, originY + row),
            3 => (originX - row, originY + col),
            4 => (originX - row, originY - col),
            5 => (originX - col, originY - row),
            6 => (originX + col, originY - row),
            7 => (originX + row, originY - col),
            _ => (originX, originY)
        };
    }
    
    public void Compute(SubnetGrid grid, int originX, int originY, int radius)
    {

        for (int x = 0; x < grid.Width; x++)
        {
            for (int y = 0; y < grid.Height; y++)
            {
                grid[x, y].IsSight = false;
            }
        }

        if (grid.IsInBounds(originX, originY))
        {
            grid[originX, originY].IsSight = true;
            grid[originX, originY].IsDiscovered = true;
        }

        for (int octant = 0; octant < 8; octant++)
        {
            ScanOctant(octant, new[] { originX, originY }, row:1, startSlope:0.0, endSlope:1.0, radius, grid);
        }
    }

    private void ScanOctant(int octant, int[] origin, int row, double startSlope, double endSlope, int radius, SubnetGrid grid)
    {
        if (row > radius || startSlope >= endSlope)
            return;

        bool previousBlocked = false;
        double currentStartSlope = startSlope;

        for (int col = 0; col <= row; col++)
        {
            double cellMinSlope = (col - 0.5) / row;
            double cellMaxSlope = (col + 0.5) / row;

            if (cellMaxSlope <= currentStartSlope) continue;                                                                                                                                                                             
            if (cellMinSlope >= endSlope) break;  


            var (x, y) = TransformOctant(octant, origin[0], origin[1], row, col);

            if (grid.IsInBounds(x, y) && col * col + row * row <= radius * radius)
            {
                grid[x, y].IsSight = true;
                grid[x, y].IsDiscovered = true;
            }

            bool isBlocked = !grid.IsInBounds(x, y) || !grid[x, y].IsTransparent;

            if (isBlocked)
            {
                if (!previousBlocked)
                {
                    ScanOctant(octant, origin, row + 1, currentStartSlope, cellMinSlope, radius, grid);
                    previousBlocked = true;
                }
            }
            else
            {
                if (previousBlocked)
                {
                    currentStartSlope = cellMaxSlope;
                    previousBlocked = false;
                }
            }
        }

        if (!previousBlocked)
        {
            ScanOctant(octant, origin, row + 1, currentStartSlope, endSlope, radius, grid);
        }
    }
}