using UnityEngine;
using UnityEngine.Events;

public static class GameKeys
{
    public static KeyCode Jump = KeyCode.Space;
    public static KeyCode Attack = KeyCode.Mouse0;
    public static KeyCode Parry = KeyCode.Mouse1;
    public static KeyCode Overclock = KeyCode.E;
    public static KeyCode Dash = KeyCode.LeftShift;

    public static void LoadKeys()
    {
        Jump = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Jump", "Space"));
        Attack = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Attack", "Mouse0"));
        Parry = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Parry", "Mouse1"));
        Overclock = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Overclock", "E"));
        Dash = (KeyCode)System.Enum.Parse(typeof(KeyCode), PlayerPrefs.GetString("Dash", "LeftShift"));
    }

    public static UnityEvent OnSettingsApplied = new UnityEvent();
    public static void SaveKeys()
    {
        PlayerPrefs.SetString("Jump", Jump.ToString());
        PlayerPrefs.SetString("Attack", Attack.ToString());
        PlayerPrefs.SetString("Parry", Parry.ToString());
        PlayerPrefs.SetString("Overclock", Overclock.ToString());
        PlayerPrefs.SetString("Dash", Dash.ToString());
        PlayerPrefs.Save();

        OnSettingsApplied.Invoke();
    }
}