using UnityEngine;

public class PlaceEndPoint : MonoBehaviour
{
    public GameObject StartPoint = null;
    public GameObject EndPointPrefab = null;
    public bool UseManualEndPlacement = false;
    public bool UseCircularEndPlacement = true;

    public float CircularSpawningAngle = 30f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        StartPoint = GameObject.FindGameObjectWithTag("StartPoint");
        if (UseManualEndPlacement) return;
        if (UseCircularEndPlacement)
        {

            Vector3 pos = StartPoint.transform.position;

            float angle = Random.Range(-CircularSpawningAngle, CircularSpawningAngle);

            

            Instantiate(EndPointPrefab, -pos, Quaternion.identity);
        }
    }
}
