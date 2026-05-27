using UnityEngine;

/// <summary>
/// Centralized Input Wrapper (as claimed in project report).
/// Abstraction layer for Input handling.
/// </summary>
public static class GameInput
{
    // Axes
    public static float GetHorizontal() => Input.GetAxisRaw("Horizontal");
    public static float GetVertical() => Input.GetAxisRaw("Vertical");

    // Actions
    public static bool GetJumpDown() => Input.GetKeyDown(GameKeys.Jump);
    public static bool GetJumpUp() => Input.GetKeyUp(GameKeys.Jump);
    public static bool GetDashDown() => Input.GetKeyDown(GameKeys.Dash);
    
    // System
    public static bool GetDevModeToggle() => 
        Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftShift) && Input.GetKeyDown(KeyCode.C);

    public static bool GetPauseToggle() => Input.GetKeyDown(KeyCode.Escape);
}
