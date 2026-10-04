using DAEDALUS07.Client.Effects;
using DAEDALUS07.Core.Entities;
using DAEDALUS07.Core.Grid;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;

namespace DAEDALUS07.Client.Input;

public class PlayerMovementController
{

    private float _currentDashCooldown = 0f;
    private readonly float _dashCooldown = 0.35f;
    private const int DashDistance = 6;

    private const float InitialDelay = 0.18f;
    private const float RepeatRate = 0.08f;

    private float _holdTimer = 0f;
    (int dx, int dy) _lastDirection = (0, 0);

    public bool Update(TimeSpan delta, Entity player, SubnetGrid grid, GhostTrailEffect? ghostTrail = null)
    {
        float deltaSeconds = (float)delta.TotalSeconds;

        if (_currentDashCooldown > 0f)
            _currentDashCooldown -= deltaSeconds;

        var keyboard = SadConsole.GameHost.Instance.Keyboard;
        var (kdx, kdy) = GetKeyboardMovement(keyboard);

        GamePadState pad = GamePad.GetState(PlayerIndex.One);
        var (pdx, pdy) = pad.IsConnected ? GetPadMovement(pad) : (0, 0);

        int moveX = kdx != 0 ? kdx : pdx;
        int moveY = kdy != 0 ? kdy : pdy;

        
        bool dashTriggered = SadConsole.GameHost.Instance.Mouse.RightClicked ||
                             (pad.IsConnected && pad.Buttons.B == ButtonState.Pressed);

        if (dashTriggered && _currentDashCooldown <= 0f && (moveX != 0 || moveY != 0))
        {
            _currentDashCooldown = _dashCooldown;

            int startX = player.x;
            int startY = player.y;
            List<(int x, int y)> trailPositions = [];

            for (int step = 1; step <= DashDistance; step++)
            {
                if (!player.TryMove(moveX, moveY, grid))
                    break;

                trailPositions.Add((player.x, player.y));
            }

            if (trailPositions.Count > 0 && ghostTrail != null)
            {
                ghostTrail.SpawnGhost(startX, startY, Theme.Current.Player, 0.25f);

                for (int i = 0; i < trailPositions.Count - 1; i += 2)
                {
                    var (gx, gy) = trailPositions[i];
                    ghostTrail.SpawnGhost(gx, gy, Theme.Current.Player, 0.20f);
                }
            }

            return trailPositions.Count > 0;
        }

        if (moveX == 0 && moveY == 0)
        {
            _lastDirection = (0, 0);
            _holdTimer = 0f;
            return false;
        }
        
        if ((moveX, moveY) != _lastDirection)
        {
            _lastDirection = (moveX, moveY);
            _holdTimer = InitialDelay;
            return player.TryMove(moveX, moveY, grid);
        }
        
        _holdTimer -= deltaSeconds;
        if (_holdTimer <= 0f)
        {
            _holdTimer = RepeatRate;
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

        if (dx != 0 || dy != 0 || pad.ThumbSticks.Left is { X: 0, Y: 0 }) return (dx, dy);

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