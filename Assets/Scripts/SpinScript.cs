using UnityEngine;

public class SpinScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        transform.localRotation = Quaternion.AngleAxis(20f, transform.right);
    }

    // Update is called once per frame
    void Update()
    {

    }
}
