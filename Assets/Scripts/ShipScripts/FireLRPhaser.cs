using UnityEngine;

[RequireComponent (typeof(LineRenderer))]

public class FireLRPhaser : MonoBehaviour 
{
    public Transform beamOrigin;
    public float beamRange = 1000f;
    LineRenderer beam;

    private void Awake()
    {
        beam = GetComponent<LineRenderer>();
    }

    private void Update()
    {
        if(true) // replace with if fire action
        {


        }

    }

   
    
}
