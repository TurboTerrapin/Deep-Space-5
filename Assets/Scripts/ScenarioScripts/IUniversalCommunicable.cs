/*
    IUniversalCommunicable.cs
    - Interface for all scenarios that involve receiving a transmission
    Contributor(s): Jake Schott
    Last Updated: 6/10/2025
*/

using UnityEngine;
using System.Collections.Generic;
public interface IUniversalCommunicable
{
    public void handleTransmission(List<int> code_indexes, List<int> code_colors, List<int> code_is_numeric);
}
