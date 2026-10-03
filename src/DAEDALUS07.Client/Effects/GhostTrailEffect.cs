using SadConsole;
using SadRogue.Primitives;
using System;
using System.Collections.Generic;

namespace DAEDALUS07.Client.Effects;

public class GhostTrailEffect
{
    private class Ghost
    {
        public ScreenSurface Surface = null!;
        public int CellX;
        public int CellY;
        public float TimeRemaining;
        public float MaxTime;
        public Color BaseColor;
    }

    private readonly List<Ghost> _pool = [];

    public GhostTrailEffect(ScreenObject parent, IFont font, int poolSize = 8)
    {
        for (int i = 0; i < poolSize; i++)
        {
            var surface = new ScreenSurface(1, 1)
            {
                Font = font,
                UsePixelPositioning = true,
                IsVisible = false
            };
            surface.Surface.SetGlyph(0, 0, '@', Color.White, Color.Transparent);
            parent.Children.Add(surface);

            _pool.Add(new Ghost
            {
                Surface = surface,
                TimeRemaining = 0f,
                MaxTime = 0.25f,
                BaseColor = Color.White
            });
        }
    }

    public void SpawnGhost(int cellX, int cellY, Color color, float lifetime = 0.25f)
    {
        foreach (var ghost in _pool)
        {
            if (ghost.TimeRemaining <= 0f)
            {
                ghost.CellX = cellX;
                ghost.CellY = cellY;
                ghost.TimeRemaining = lifetime;
                ghost.MaxTime = lifetime;
                ghost.BaseColor = color;
                ghost.Surface.IsVisible = true;
                ghost.Surface.Surface.SetGlyph(0, 0, '@', color, Color.Transparent);
                return;
            }
        }
    }

    public void Update(TimeSpan delta, int viewCellX, int viewCellY, Point fontSize)
    {
        float deltaSeconds = (float)delta.TotalSeconds;

        foreach (var ghost in _pool)
        {
            if (ghost.TimeRemaining <= 0f)
            {
                ghost.Surface.IsVisible = false;
                continue;
            }

            ghost.TimeRemaining -= deltaSeconds;
            float progress = Math.Clamp(ghost.TimeRemaining / ghost.MaxTime, 0f, 1f);

            int px = (ghost.CellX - viewCellX) * fontSize.X;
            int py = (ghost.CellY - viewCellY) * fontSize.Y;
            ghost.Surface.Position = new Point(px, py);

            Color color = ghost.BaseColor * progress;
            ghost.Surface.Surface.SetGlyph(0, 0, '@', color, Color.Transparent);

            if (ghost.TimeRemaining <= 0f)
            {
                ghost.Surface.IsVisible = false;
            }
        }
    }
}
