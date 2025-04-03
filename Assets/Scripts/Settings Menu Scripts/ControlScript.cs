using UnityEngine;

public class ControlScript : MonoBehaviour
{
    public static int HUD_setting; 

    public static void setHUD(int new_value)
    {
        Debug.LogError($"setHUD() received index: {new_value}");
    }
}
