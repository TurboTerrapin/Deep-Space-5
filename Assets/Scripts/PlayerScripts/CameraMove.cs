/*
    CameraMove.cs
    - Handles pausing
    - Handles looking around
    - Handles camera zoom (using CTRL)
    Contributor(s): John Aylward, Jake Schott
    Last Updated: 4/2/2025
*/

using UnityEngine;
using System.Globalization;

public class PlayerCameraMove : MonoBehaviour
{
    [SerializeField]
    private Vector2 mouseMove = new Vector2();
    private Vector2 prevPos = new Vector2();
    [SerializeField]
    private float mouseSensitivity = 100f;
    public Camera my_camera;
    private float zoom_FOV = 40f;
    private ControlScript control_script;

    void Start()
    {
        FindAnyObjectByType<AudioManager>()?.SetMasterVolume(0.75f);
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.SetActive(true);
        control_script = (ControlScript)transform.parent.GetComponent("ControlScript");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                control_script.pause();
            }
            else
            {
                control_script.unpause();
            }
        }
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            MouseMove();
        }
        if (!control_script.isPaused())
        {
            if (Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl) || Input.GetKey(KeyCode.Mouse1))
            {
                my_camera.fieldOfView = Mathf.Max(zoom_FOV, my_camera.fieldOfView -= 100f * Time.deltaTime);
                return;
            }
        }
        my_camera.fieldOfView = Mathf.Min(60f, my_camera.fieldOfView += 100f * Time.deltaTime);
    }

    void MouseMove()
    {
        Cursor.visible = false;
        //Gets mouse input
        mouseMove = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        mouseMove.Normalize();
        //Increases the sensitivity to movement
        mouseMove *= mouseSensitivity * Time.deltaTime * Mathf.Min(1f, (1.1f - ((60f - my_camera.fieldOfView) / 20f)));

        prevPos.y = Mathf.Clamp(prevPos.y, -90f, 90f);

        prevPos.y -= mouseMove.y;
        prevPos.x += mouseMove.x;
        transform.localRotation = Quaternion.AngleAxis(prevPos.y, Vector3.right);
        transform.parent.localRotation = Quaternion.AngleAxis(prevPos.x, Vector3.up);
    }

}