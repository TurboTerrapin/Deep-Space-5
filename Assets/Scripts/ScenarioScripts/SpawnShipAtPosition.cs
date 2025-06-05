using UnityEngine;

public class SpawnShipAtPosition : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        GameObject ship = GameObject.FindGameObjectWithTag("Spaceship");
        ship.transform.position = transform.position;
    }
}
