using UnityEngine;
using Unity.Netcode;

public class PlayerMove : NetworkBehaviour
{
    [SerializeField]
    private GameObject camera = null;
    [SerializeField]
    private Vector2 moveDir = new Vector2();
    [SerializeField]
    private Rigidbody playerRB = null;
    public float moveSpeed = 100f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        moveDir.x = Input.GetAxis("Horizontal");
        moveDir.y = Input.GetAxis("Vertical");
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10);
        if (moveDir.magnitude > 1)
        {
            moveDir.Normalize();
        }
        Move();

        if(transform.localPosition.y < -10)
        {
            transform.localPosition = Vector3.zero;
            playerRB.linearVelocity = Vector3.zero;
        }


    }
    void FixedUpdate()
    {
        //MoveRB();
    }

    void Move()
    {
        //transform.localPosition += new Vector3  (moveDir.x, 0, moveDir.y) * moveSpeed * Time.deltaTime;
        transform.localPosition += transform.right * moveDir.x * moveSpeed * Time.deltaTime;
        transform.localPosition += transform.forward * moveDir.y * moveSpeed * Time.deltaTime;
    }

    void MoveRB()
    {
        playerRB.AddForce(transform.forward * moveDir.y * moveSpeed * Time.fixedDeltaTime);
        playerRB.AddForce(transform.right * moveDir.x * moveSpeed * Time.fixedDeltaTime);
        if(moveDir.magnitude == 0)
        {
            playerRB.AddForce(-playerRB.linearVelocity * 500f * Time.fixedDeltaTime);
        }
        playerRB.AddForce(-playerRB.linearVelocity * 0.1f * Time.fixedDeltaTime);
    }

}
