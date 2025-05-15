/*
    PlayerMove.cs
    - Handles player movement (disabled)
    - Handles position swapping (enabled)
    - Handles player shifting (enabled)
    Contributor(s): John Aylward, Jake Schott
    Last Updated: 4/13/2025
*/

using UnityEngine;

public class PlayerMove : MonoBehaviour
{
    /*[SerializeField]
    private GameObject camera = null;
    [SerializeField]
    private Vector2 moveDir = new Vector2();
    [SerializeField]
    private Rigidbody playerRB = null;
    public float moveSpeed = 100f;*/
    // Start is called once before the first execution of Update after the MonoBehaviour is created

    private float MOVE_FACTOR = 2.0f;

    private bool is_pilot = true;
    private bool is_shifting = false;
    private float shift_direction = -1.0f;
    private float lean_direction = 1.0f;
    private float shift_percentage = 0.0f; //0 is default position, 1.0 is 
    private float default_x;
    void Start()
    {
        default_x = transform.localPosition.x;
        //playerRB = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    private void LateUpdate()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            shift_percentage = 0.0f;
            shift_direction = -1f;
            lean_direction *= -1f;
            is_shifting = false;
            is_pilot = !is_pilot;
            if (is_pilot == true)
            {
                transform.localPosition = new Vector3(2.1f, 0.8f, -2f);
            }
            else
            {
                transform.localPosition = new Vector3(-2.1f, 0.8f, -2f);
            }
            default_x = transform.localPosition.x;
        }
        if (is_shifting)
        {
            if (shift_direction > 0.0f)
            {
                shift_percentage = Mathf.Min(1f, shift_percentage += Time.deltaTime * MOVE_FACTOR);
                if (shift_percentage >= 1f)
                {
                    is_shifting = false;
                }
            }
            else
            {
                shift_percentage = Mathf.Max(0f, shift_percentage -= Time.deltaTime * MOVE_FACTOR);
                if (shift_percentage <= 0f)
                {
                    is_shifting = false;
                }
            }
            transform.localPosition = new Vector3(default_x + (shift_percentage * 0.6f * lean_direction), transform.localPosition.y, transform.localPosition.z);
        }
        else
        {
            if (UnityEngine.Input.GetKeyDown(KeyCode.LeftShift))
            {
                is_shifting = true;
                shift_direction *= -1.0f;
            }
        }
    }

    /*
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