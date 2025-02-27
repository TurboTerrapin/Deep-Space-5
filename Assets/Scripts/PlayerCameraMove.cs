using UnityEngine;
using Unity.Netcode;

public class PlayerCameraMove : MonoBehaviour
{
    [SerializeField]
    private Vector2 mouseMove = new Vector2();
    private Vector2 prevPos = new Vector2();
    [SerializeField]
    private float mouseSensitivity = 50f;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        gameObject.GetComponent<Camera>().enabled = false;
        if (transform.parent.gameObject.GetComponent<PlayerMove>().IsOwner)
        {
            gameObject.GetComponent<Camera>().enabled = true;
        }
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.Escape) && Cursor.lockState == CursorLockMode.Locked)
        {
            Cursor.lockState = CursorLockMode.None;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && Cursor.lockState == CursorLockMode.None)
        {
            Cursor.lockState= CursorLockMode.Locked;
        }

    }



    void LateUpdate()
    {
        
        if (transform.parent.gameObject.GetComponent<PlayerMove>().IsOwner && Cursor.lockState == CursorLockMode.Locked)
        {
            MouseMove();
        }
    }

    void MouseMove()
    {
        //Gets mouse input
        mouseMove = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));
        //Increases the sensitivity to movement
        mouseMove *= mouseSensitivity * Time.deltaTime;

        prevPos.y = Mathf.Clamp(prevPos.y, -90f, 90f);

        prevPos.y -= mouseMove.y;
        prevPos.x += mouseMove.x;
        transform.localRotation = Quaternion.AngleAxis(prevPos.y, Vector3.right);

        transform.parent.localRotation = Quaternion.AngleAxis(prevPos.x, Vector3.up);
    }


}
