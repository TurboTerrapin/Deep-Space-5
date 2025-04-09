/*
    IControllable.cs
    - Interface for all controls
    - Used to define information retrieval and input handling
    Contributor(s): Jake Schott
    Last Updated: 3/25/2025
*/

using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;
public interface IControllable
{
    public HUDInfo getHUDinfo();
    public void handleInputs(List<KeyCode> inputs);
}
