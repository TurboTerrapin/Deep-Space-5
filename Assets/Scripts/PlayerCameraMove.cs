using UnityEngine;
using Unity.Netcode;

public class PlayerCameraMove : NetworkBehaviour
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
        gameObject.SetActive(false);
        if (IsOwner)
        {
            gameObject.SetActive(true);
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
        
        if (Cursor.lockState == CursorLockMode.Locked)
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
