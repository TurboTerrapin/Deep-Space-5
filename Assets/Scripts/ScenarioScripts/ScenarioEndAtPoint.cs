using UnityEngine;

public class ScenarioEndAtPoint : MonoBehaviour
{

    public float range = 10;
    private GameObject ship = null;
    private bool alreadyIn = false;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        ship = GameObject.FindGameObjectWithTag("Spaceship");
    }

    // Update is called once per frame
    void Update()
    {
        if ((ship.transform.position - transform.position).magnitude <= range && !alreadyIn)
        {
            alreadyIn = true;
            //Call the scenario manager to change scenarios
            //SceneSwapper.Instance.ChangeSceneRandom();
            Debug.Log("Ship in range");
        }
    }
}
