using UnityEngine;
using Unity.Netcode;

public class PlayerMove : NetworkBehaviour
{
    public Vector2 moveDir = new Vector2();
    public float moveSpeed = 10f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        /*
        if(IsOwner)
        {
            Move();
        }
        else
        {
            return;
        }
        */
        Move();
    }

    void Move()
    {
        moveDir.x = Input.GetAxis("Horizontal");
        moveDir.y = Input.GetAxis("Vertical");

        if (moveDir.magnitude > 1)
        {
            moveDir.Normalize();
        }

        transform.position += new Vector3(moveDir.x, 0, moveDir.y) * moveSpeed * Time.deltaTime;
    }


}
