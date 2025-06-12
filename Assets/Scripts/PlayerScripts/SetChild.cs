using UnityEngine;
public class SetChild : MonoBehaviour
{
    public bool parented = false;

    private void Update()
    {
        if (parented) Destroy(this);
        // Find the spaceship object by tag
        GameObject spaceship = GameObject.FindGameObjectWithTag("Spaceship");

        if (spaceship != null)
        {
            transform.SetParent(spaceship.transform);
            parented = true;
        }
    }
}