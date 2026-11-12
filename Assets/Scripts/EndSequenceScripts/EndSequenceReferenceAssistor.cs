/*
    EndSequenceReferenceAssistor.cs
    - Used to reference objects in the end sequence scene
    Contributor(s): Jake Schott
    Last Updated: 10/30/2026
*/

using UnityEngine;

public class EndSequenceReferenceAssistor : MonoBehaviour
{
    public GameObject EndSequenceCamera;
    public GameObject EndSequenceCameraPositions;
    public GameObject EndSequenceShip;
    public GameObject EndSequenceShipPositions;
    public GameObject DeepSpaceFiveSpawnPoints;
    public GameObject DeepSpaceFivePlayersHolder;
    public GameObject DeepSpaceFiveGateBDoor;

    public static EndSequenceReferenceAssistor Instance { get; private set; }

    private void Start()
    {
        if (Instance != null)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
}
