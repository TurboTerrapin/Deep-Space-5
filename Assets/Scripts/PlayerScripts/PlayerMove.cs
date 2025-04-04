using UnityEngine;

public class PlayerMove : MonoBehaviour
{
/*    [SerializeField]
    private GameObject camera = null;
    [SerializeField]
    private Vector2 moveDir = new Vector2();
    [SerializeField]
    private Rigidbody playerRB = null;
    public float moveSpeed = 100f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        moveDir.x = Input.GetAxis("Horizontal");
        moveDir.y = Input.GetAxis("Vertical");
        Debug.DrawLine(transform.position, transform.position + transform.forward * 10);
        if (moveDir.magnitude > 1)
        {
            moveDir.Normalize();
        }
        Move();

        if (transform.localPosition.y < -10)
        {
            transform.localPosition = new Vector3(0f, 1f, 1f);
            playerRB.linearVelocity = Vector3.zero;
        }
        else if (moveDir == Vector2.zero)
        {
            playerRB.linearVelocity = Vector3.zero;
        }


    }

    void Move()
    {
        //transform.localPosition += new Vector3  (moveDir.x, 0, moveDir.y) * moveSpeed * Time.deltaTime;
        transform.localPosition += transform.right * moveDir.x * moveSpeed * Time.deltaTime;
        transform.localPosition += transform.forward * moveDir.y * moveSpeed * Time.deltaTime;
    }*/

}