using Unity.Netcode;
using UnityEngine;

[RequireComponent(typeof(PilotingSystem))]
[RequireComponent(typeof(WeaponsSystem))]
public class ShipController : NetworkBehaviour
{
    private PilotingSystem pilotingSystem;
    private WeaponsSystem weaponsSystem;

    private GameObject controlHandler;
    private bool shipReady = false;

    public Transform world_Root; 

    private void Awake()
    {
        pilotingSystem = GetComponent<PilotingSystem>();
        weaponsSystem = GetComponent<WeaponsSystem>();

 
    }

    void Start()
    {
        controlHandler = GameObject.FindGameObjectWithTag("ControlHandler");

        if (controlHandler != null && pilotingSystem.AssignControlReferences(controlHandler)
            && weaponsSystem.AssignControlReferences(controlHandler))
        {
            shipReady = true;
        }

        transform.position = Vector3.zero;
    }

    void Update()
    {
        if (!shipReady || !IsHost) return;

        pilotingSystem.UpdateInput();
        //weaponsSystem.UpdateInput();

        //weaponsSystem.UpdateWeapons();
        pilotingSystem.UpdateMovement(world_Root); // Pass the world root to move
    }
}