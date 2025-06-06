/*
    CameraMove.cs
    - Handles pausing
    - Handles looking around
    - Handles camera zoom (using RMB)
    Contributor(s): John Aylward, Jake Schott
    Last Updated: 5/29/2025
*/

using System.Collections;
using UnityEngine;
using Unity.Netcode;
using System.Globalization;
using Unity.VisualScripting;
using Steamworks;
using Steamworks.Data;

public class CameraMove : MonoBehaviour
{
    private Vector2 mouseMove = new Vector2();
    private Vector2 prevPos = new Vector2(0f, 0);
    private Rigidbody rb = null;

    private float mouseSensitivity = 1f;
    public Camera my_camera;
    private float zoom_FOV = 40f;

    public void Start()
    {
        if (transform.parent.gameObject.GetComponent<PlayerMove>().IsOwner)
        {
            transform.parent.name = SteamClient.Name + "_" + SteamClient.SteamId.ToString();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void initialize()
    {
        rb = transform.parent.gameObject.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;

        my_camera = transform.GetComponent<Camera>();
        ControlScript.Instance.my_camera = my_camera;

        if (my_camera != null)
        {
            my_camera.gameObject.AddComponent<AudioListener>();
        }

        StartCoroutine(cameraUpdater());
    }

    //calls updateCamera() every frame
    IEnumerator cameraUpdater()
    {
        while (true)
        {
            updateCamera();
            yield return null;
        }
    }

    //runs every frame after initialize() is called
    private void updateCamera()
    {
        //make sure we are the owner
        if (!transform.parent.gameObject.GetComponent<PlayerMove>().IsOwner) return;

        //handle pause/unpause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (Cursor.lockState == CursorLockMode.Locked)
            {
                ControlScript.Instance.pause();
                rb.angularVelocity = Vector3.zero;
            }
            else
            {
                ControlScript.Instance.unpause();
            }
        }

        //if not paused
        if (Cursor.lockState == CursorLockMode.Locked)
        {
            MouseMove();
        }
        if (!ControlScript.Instance.isPaused())
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
 
        //Increases the sensitivity to movement
        mouseMove *= mouseSensitivity * Mathf.Min(1f, (1.1f - ((60f - my_camera.fieldOfView) / 20f)));

        prevPos.y = Mathf.Clamp(prevPos.y, -90f, 90f);

        prevPos.y -= mouseMove.y;
        prevPos.x += mouseMove.x;
        transform.localRotation = Quaternion.AngleAxis(prevPos.y, Vector3.right);
        transform.parent.localRotation = Quaternion.AngleAxis(prevPos.x, Vector3.up);
    }

    public void SetMouseSensitvity(float newSensitivity)
    {
        mouseSensitivity = newSensitivity;
    }
}