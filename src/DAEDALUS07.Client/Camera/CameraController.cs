using DAEDALUS07.Core.Grid;
using Microsoft.Xna.Framework.Input;
using SadRogue.Primitives;
using Mouse = SadConsole.Input.Mouse;

namespace DAEDALUS07.Client.Rendering;

public class CameraController
{
    private const int LookAheadRadiusCells = 8;
    public const float LerpSpeed = 4f;
    public const float CameraSpeed = 4f;
    private SubnetRenderer _subnetRenderer;
    private float _currentOffsetX;
    private float _currentOffsetY;
    private float _cameraX;
    private float _cameraY;

    public CameraController(float cameraX, float cameraY, SubnetRenderer subnetRenderer)
    {
        this._cameraX = cameraX;
        this._cameraY = cameraY;
        _subnetRenderer = subnetRenderer;
    }

    public (int cellX, int cellY, int subPixelX, int subPixelY) Update(TimeSpan delta, Point playerPos, Mouse mouse, GamePadState pad, SubnetGrid grid)
    {
        int offsetX = 0;
        int offsetY = 0;

        if (pad.IsConnected)
        {
            (offsetX, offsetY) = GetPadOffset(pad);
        }

        if (offsetX == 0 && offsetY == 0 && mouse.IsOnScreen)
        {
            (offsetX, offsetY) = GetMouseOffset(mouse);
        }

        _currentOffsetX += (offsetX - _currentOffsetX) * (float)Math.Min(1.0, LerpSpeed * delta.TotalSeconds);
        _currentOffsetY += (offsetY - _currentOffsetY) * (float)Math.Min(1.0, LerpSpeed * delta.TotalSeconds);

        offsetX = (int)Math.Round(_currentOffsetX);
        offsetY = (int)Math.Round(_currentOffsetY);

        int maxPixelX = (grid.Width - _subnetRenderer.Surface.ViewWidth) * _subnetRenderer.FontSize.X;
        int maxPixelY = (grid.Height - _subnetRenderer.Surface.ViewHeight) * _subnetRenderer.FontSize.Y;

        float targetCamX = playerPos.X * _subnetRenderer.FontSize.X + offsetX - (_subnetRenderer.Surface.ViewWidth / 2f * _subnetRenderer.FontSize.X);
        float targetCamY = playerPos.Y * _subnetRenderer.FontSize.Y + offsetY - (_subnetRenderer.Surface.ViewHeight / 2f * _subnetRenderer.FontSize.Y);
        targetCamX = Math.Clamp(targetCamX, 0, maxPixelX);
        targetCamY = Math.Clamp(targetCamY, 0, maxPixelY);

        _cameraX += (targetCamX - _cameraX) * (float)Math.Min(1.0, CameraSpeed * delta.TotalSeconds);
        _cameraY += (targetCamY - _cameraY) * (float)Math.Min(1.0, CameraSpeed * delta.TotalSeconds);

        int camPixelX = (int)Math.Round(_cameraX);
        int camPixelY = (int)Math.Round(_cameraY);

        int cellX = camPixelX / _subnetRenderer.FontSize.X;
        int subPixelX = camPixelX % _subnetRenderer.FontSize.X;

        int cellY = camPixelY / _subnetRenderer.FontSize.Y;
        int subPixelY = camPixelY % _subnetRenderer.FontSize.Y;

        return (cellX, cellY, subPixelX, subPixelY);
    }

    public (int x, int y) GetPadOffset(GamePadState pad)
    {
        float stickX = pad.ThumbSticks.Right.X;
        float stickY = -pad.ThumbSticks.Right.Y;

        int offsetX = 0;
        int offsetY = 0;

        float deadzone = 0.2f;
        
        if (stickX * stickX + stickY * stickY >= deadzone * deadzone)                                                                                                                                                                                
        {                                                                                                                                                                                                                                            
            offsetX = (int)Math.Round(stickX * LookAheadRadiusCells * _subnetRenderer.FontSize.X);                                                                                                                                                                                                   
            offsetY = (int)Math.Round(stickY * LookAheadRadiusCells * _subnetRenderer.FontSize.Y);                                                                                                                                                                                                   
        }

        return (offsetX, offsetY);
    }

    public (int x, int y) GetMouseOffset(Mouse mouse)
    {
        int centerX = (_subnetRenderer.Surface.ViewWidth * _subnetRenderer.FontSize.X) / 2 ;
        int centerY = (_subnetRenderer.Surface.ViewHeight * _subnetRenderer.FontSize.Y) / 2;
        
        double mouseVecX = mouse.ScreenPosition.X - centerX;
        double mouseVecY = mouse.ScreenPosition.Y - centerY;

        int maxRadius = LookAheadRadiusCells * _subnetRenderer.FontSize.X;

        double dist = Math.Sqrt(mouseVecX * mouseVecX + mouseVecY * mouseVecY);

        if (dist > maxRadius)
        {
            mouseVecX *= (maxRadius / dist);
            mouseVecY *= (maxRadius / dist);
        }

        return ((int)mouseVecX, (int)mouseVecY);
    }
}