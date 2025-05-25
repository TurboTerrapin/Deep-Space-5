using UnityEngine;

public class TestTarget : MonoBehaviour
{
    public float health = 100f;

    /*
    void OnTriggerStay(Collider other)
    {
        if (other.CompareTag("LongRangePhaser"))
        {
            health -= 10f * Time.deltaTime; // Flat damage rate
            Debug.Log($"Target Health: {health}"); // Visual feedback
        }
    }
    */
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"{other.name} touched me!");
    }
}