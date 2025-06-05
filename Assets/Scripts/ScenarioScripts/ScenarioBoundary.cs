using UnityEngine;

public class ScenarioBoundary : MonoBehaviour
{
    public GameObject ship = null;
    public bool UseManualBoundary = false;

    public bool UseCircularBoundary = true;
    public float ScenarioSize = 100f;



    public static ScenarioBoundary Instance { get; private set; } = null;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ship = GameObject.FindGameObjectWithTag("Spaceship");
    }

    // Update is called once per frame
    void Update()
    {
        if (UseManualBoundary) return;
        if (UseCircularBoundary)
        {
            if ((ship.transform.position - transform.position).magnitude > ScenarioSize)
            {
                //Ship is outside the boundary
                Debug.DrawLine(ship.transform.position, ship.transform.position + (ship.transform.forward * 10), Color.red);
                //Debug.Log("Ship outside boundary");
            }
            else
            {
                Debug.DrawLine(ship.transform.position, ship.transform.position + (ship.transform.forward * 10), Color.green);
                //Debug.Log("Ship inside boundary");
                return;
            }
        }
    }
}
