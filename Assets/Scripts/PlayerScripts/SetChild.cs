using UnityEngine;
public class SetChild : MonoBehaviour
{
    void Start()
    {
        // Find the spaceship object by tag
        GameObject spaceship = GameObject.FindGameObjectWithTag("Spaceship");

        if (spaceship != null)
        {
            transform.SetParent(spaceship.transform);

        }

    }
}