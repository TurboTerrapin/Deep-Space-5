/*
    InertialDampener.cs
    - Handles an individual inertial dampener
    - Yet to be implemented
    - Meant to be extended
    Contributor(s): Jake Schott
    Last Updated: 3/30/2025
*/

using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Windows;
using static UnityEngine.Rendering.VirtualTexturing.Debugging;

public class InertialDampener : MonoBehaviour
{
    public GameObject lever;
    public GameObject display_canvas;

    protected bool is_dampened = true;
}
