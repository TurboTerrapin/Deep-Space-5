using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]

public abstract class Setting
{
    public string Name;
}

[System.Serializable]

public class BoolSettings : Setting
{
    public bool State;
}
