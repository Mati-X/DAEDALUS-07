using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Grid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace DAEDALUS07.Client.Input;

public class PlayerMovementController
{
    private float _currentMoveCooldown = 0f;
    private readonly float _moveCooldown = 0.05f;
    
    public bool Update(TimeSpan delta, Entity player, SubnetGrid grid)
    {
        if (_currentMoveCooldown > 0)
        {
            _currentMoveCooldown -= (float)delta.TotalSeconds;
        }

        var keyboard = SadConsole.GameHost.Instance.Keyboard;
        var (kdx, kdy) = GetKeyboardMovement(keyboard);
        
        GamePadState pad = GamePad.GetState(PlayerIndex.One);
        var (pdx, pdy) = pad.IsConnected ? GetPadMovement(pad) : (0, 0);

        int moveX = kdx != 0 ? kdx : pdx;
        int moveY = kdy != 0 ? kdy : pdy;

        if ((moveX != 0 || moveY != 0) && _currentMoveCooldown <= 0f)
        {
            _currentMoveCooldown = _moveCooldown;
            return player.TryMove(moveX, moveY, grid);
        }

        return false;
    }

    private (int x, int y) GetKeyboardMovement(SadConsole.Input.Keyboard keyboard)                                                                                                                                               
    {                                                                                                                                                                                                                            
        int dx = 0, dy = 0;                                                                                
        
        if (keyboard.IsKeyDown(SadConsole.Input.Keys.W) || keyboard.IsKeyDown(SadConsole.Input.Keys.Up)) dy--;                                                                                                                   
        else if (keyboard.IsKeyDown(SadConsole.Input.Keys.S) || keyboard.IsKeyDown(SadConsole.Input.Keys.Down)) dy++;  
        
        if (keyboard.IsKeyDown(SadConsole.Input.Keys.A) || keyboard.IsKeyDown(SadConsole.Input.Keys.Left)) dx--;                                                                                                            
        else if (keyboard.IsKeyDown(SadConsole.Input.Keys.D) || keyboard.IsKeyDown(SadConsole.Input.Keys.Right)) dx++;     
        
        return (dx, dy);                                                                                                                                                                                                         
    }     
    
    private (int dx, int dy) GetPadMovement(GamePadState pad)
    {
        int dx = 0;
        int dy = 0;
        
        if (pad.DPad.Up == ButtonState.Pressed) dy--;
        else if (pad.DPad.Down == ButtonState.Pressed) dy++;
        
        if (pad.DPad.Left == ButtonState.Pressed) dx--;
        else if (pad.DPad.Right == ButtonState.Pressed) dx++;

        if (dx != 0 || dy != 0 || pad.ThumbSticks.Left is { X: 0, Y: 0 } ) return (dx, dy);

        switch (pad.ThumbSticks.Left.X)
        {
            case > 0.5f:
                dx++;
                break;
            case < -0.5f:
                dx--;
                break;
        }

        switch (pad.ThumbSticks.Left.Y)
        {
            case < -0.5f:
                dy++;
                break;
            case > 0.5f:
                dy--;
                break;
        }

        return (dx, dy);

    }
}