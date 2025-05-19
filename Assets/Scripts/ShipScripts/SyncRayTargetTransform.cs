using UnityEngine;

public class SyncRayTargetTransform : MonoBehaviour
{
    private GameObject spaceship;
    Vector3 rayTargetsOffset = new Vector3(57.62543f, -40.84872f, -18.40079f);
    void Start()
    {
        spaceship = GameObject.FindGameObjectWithTag("Spaceship");
    }

    void LateUpdate()
    {
        transform.SetPositionAndRotation(
            spaceship.transform.position + rayTargetsOffset, spaceship.transform.rotation
        );
    }


}
