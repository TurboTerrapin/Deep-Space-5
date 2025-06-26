/*
    Billboard.cs
    - Constantly aims the object towards the player's camera
    Contributor(s): Jake Schott
    Last Updated: 6/25/2025
*/

using UnityEngine;

public class Billboard : MonoBehaviour
{
    private SpriteRenderer star;
    private void Start()
    {
        star = GetComponent<SpriteRenderer>();
    }
    void LateUpdate()
    {
        transform.LookAt(Camera.main.transform.position);
        float dist = Vector3.Distance(transform.position, Camera.main.transform.position);
        star.color = new Color(1.0f, 1.0f, 1.0f, Mathf.Clamp((dist - 20.0f) / 20.0f, 0.0f, 1.0f));
    }
}
