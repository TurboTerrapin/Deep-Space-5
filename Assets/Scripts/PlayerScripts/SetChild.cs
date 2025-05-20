using UnityEngine;
public class SetChild : MonoBehaviour
{
    public bool parented = false;
    void Start()
    {
        // Find the spaceship object by tag
        GameObject spaceship = GameObject.FindGameObjectWithTag("Spaceship");

        if (spaceship != null)
        {
            transform.SetParent(spaceship.transform);

        }

    }


    private void Update()
    {
        if (parented) return;
        // Find the spaceship object by tag
        GameObject spaceship = GameObject.FindGameObjectWithTag("Spaceship");

        if (spaceship != null)
        {
            transform.SetParent(spaceship.transform);

        }
    }

}