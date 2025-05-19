/*
    CameraMove.cs
    - Handles pausing
    - Handles looking around
    - Handles camera zoom (using CTRL)
    Contributor(s): John Aylward, Jake Schott
    Last Updated: 4/11/2025
*/

using UnityEngine;
using System.Globalization;

public class CameraMove : MonoBehaviour
{
    private Vector2 mouseMove = new Vector2();
    private Vector2 prevPos = new Vector2(180f, 0);
    private Rigidbody rb = null;

    private float mouseSensitivity = 1f;
    public Camera my_camera;
    private float zoom_FOV = 40f;
    //private ControlScript control_script;

    void Start()
    {
        rb = transform.parent.gameObject.GetComponent<Rigidbody>();
        Cursor.lockState = CursorLockMode.Locked;
        //gameObject.SetActive(true);
        gameObject.name += Random.Range(0, 100);
        //gameObject.GetComponent<Camera>().enabled = false;
        if (!transform.parent.gameObject.GetComponent<PlayerMove>().IsOwner)
        {
            Destroy(gameObject);
            //gameObject.GetComponent<Camera>().enabled = true;
        }
        else
        {
            ControlScript.Instance.my_camera = gameObject.GetComponent<Camera>();
        }
        /*
        foreach (GameObject cam in GameObject.FindGameObjectsWithTag("MainCamera"))
        {
            cam.SetActive(false);
        }
        gameObject.SetActive(true);
        */
        if (my_camera == null)
        {
            my_camera = Camera.current;
        }

        if (my_camera == null)
        {
            my_camera = Camera.main;
        }
        //ControlScript.Instance = (ControlScript)transform.parent.GetComponent("ControlScript");
    }

    // Update is called once per frame
    void Update()
    {
        if (!transform.parent.gameObject.GetComponent<PlayerMove>().IsOwner) return;
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