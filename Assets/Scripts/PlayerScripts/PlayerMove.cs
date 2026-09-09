/*
    PlayerMove.cs
    - Names the player prefab to USERNAME_STEAMID if client, or OTHER_CLIENT if not
    - Handles player movement
    - Handles sitting down and getting up movement/animations
    - Handles shifting while seated
    - Enables collisions/rigidbody/gravity on the player character
    Contributor(s): John Aylward, Jake Schott
    Last Updated: 9/5/2026
*/

using System.Collections;
using Steamworks;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerMove : NetworkBehaviour
{
    //CLASS CONSTANTS
    private static float SHIFT_SPEED = 2.5f;
    private static float PUSH_TIME = 0.2f; //How long the seat push in or out takes in seconds
    private static float MOVE_SPEED = 5.0f;

    private Vector2 moveDir = new Vector2();
    [SerializeField]
    private Rigidbody playerRB = null;

    private Coroutine seatChangeCoroutine = null; //Used for sit down or get up animations
    private Coroutine shiftCoroutine = null; //Used for seat shifting
    private Coroutine moveCoroutine = null; //Used for movement checking

    [SerializeField]
    private Animator animator = null;

    AnimationController myAnimationController = null;

    private void Start()
    {
        if (SceneManager.GetActiveScene().name.CompareTo("IntroSequence") != 0)
        {
            DontDestroyOnLoad(gameObject);
        }

        myAnimationController = GetComponent<AnimationController>();

        if (NetworkManager.Singleton.IsListening == false || GetComponent<NetworkObject>().IsOwner == true)
        {
            //USERNAME_STEAMID
            transform.name = SteamClient.Name + "_" + SteamClient.SteamId.ToString();
        }
        else
        {
            transform.name = "OTHER_CLIENT";
        }
    }

    public void Initialize()
    {
        if (moveCoroutine == null)
        {
            moveCoroutine = StartCoroutine(CheckForMovement());
        }
    }

    //called by FailureHandler.cs on game restart
    public void ResetPlayerMove()
    {
        ResetCoroutines();
        myAnimationController.setAnimatorBool("IsLeft", false);
        myAnimationController.setAnimatorBool("IsCaptain", false);
        myAnimationController.setAnimatorFloat("Movement", 0.0f);
        myAnimationController.setAnimatorFloat("Forward", 0.0f);
        myAnimationController.setAnimatorBool("SittingDown", false);
        myAnimationController.setAnimatorBool("GettingUp", false);
        myAnimationController.setCharacterPosition(Vector3.zero);
    }

    //Called by ResetPlayerMove()
    private void ResetCoroutines()
    {
        StopAllCoroutines();
        moveCoroutine = null;
        seatChangeCoroutine = null;
        shiftCoroutine = null;
    }

    //Called by PrimaryScript.cs
    public void TriggerSitDownAnimation(bool isLeft, bool captainMode, GameObject physicalSeat, GameObject animationStartPoint)
    {
        ResetCoroutines();
        seatChangeCoroutine = StartCoroutine(SitDownAnimation(isLeft, captainMode, physicalSeat, animationStartPoint));
    }

    //Called by PrimaryScript.cs
    public void TriggerGetUpAnimation(bool isLeft, bool captainMode, GameObject physicalSeat, Vector2 pushAdjustment)
    {
        ResetCoroutines();
        seatChangeCoroutine = StartCoroutine(GetUpAnimation(isLeft, captainMode, physicalSeat, pushAdjustment));
    }

    //Used to move the bean during a sit or get up animation, timed to match the in place animation to simulate movement
    IEnumerator PlayerAnimationTransformationAdjustment(Vector3 movePosition)
    {
        yield return new WaitForSeconds(0.75f);

        float animTime = 0.75f;
        Vector3 startPos = transform.localPosition;
        while (animTime > 0.0f)
        {
            animTime = Mathf.Max(animTime - Time.deltaTime, 0.0f);

            transform.localPosition = Vector3.Lerp(movePosition, startPos, animTime / 0.75f);

            yield return null;
        }
    }

    //Handles sit down sequence
    IEnumerator SitDownAnimation(bool isLeft, bool captainMode, GameObject physicalSeat, GameObject animationStartPoint)
    {
        animator.transform.GetComponent<AnimatorHandler>().setIKActive(false);

        myAnimationController.setAnimatorBool("IsLeft", isLeft);
        myAnimationController.setAnimatorBool("IsCaptain", captainMode);
        myAnimationController.setAnimatorFloat("Movement", 0.0f);
        myAnimationController.setAnimatorFloat("Forward", 0.0f);

        yield return StartCoroutine(RepositionPlayerAndCamera(animationStartPoint.transform.localPosition + animationStartPoint.transform.parent.localPosition, animationStartPoint.transform.localRotation.eulerAngles.y, 0.2f));

        myAnimationController.setAnimatorBool("SittingDown", true);
        myAnimationController.setAnimatorBool("GettingUp", false); //Trigger sit down animation

        //If not captain, move the player DURING the animation because the animation happens in place
        if (captainMode == false)
        {
            Vector3 endPos = physicalSeat.transform.localPosition + physicalSeat.transform.GetChild(2).localPosition;
            yield return StartCoroutine(PlayerAnimationTransformationAdjustment(endPos));
        }

        seatChangeCoroutine = null;
    }

    //Handles get up sequence
    IEnumerator GetUpAnimation(bool isLeft, bool captainMode, GameObject physicalSeat, Vector2 pushAdjustment)
    {
        Transform cameraHolder = transform.GetComponent<CameraMove>().cameraHolder;

        myAnimationController.setIKActive(false);
        transform.GetComponent<CameraMove>().LockCamera();

        Quaternion startingRotation = cameraHolder.localRotation;

        float animTime = 0.15f;
        while (animTime > 0.0f)
        {
            animTime = Mathf.Max(0.0f, animTime - Time.deltaTime);

            cameraHolder.localRotation = Quaternion.Lerp(Quaternion.Euler(30.0f, 0.0f, 0.0f), startingRotation, animTime / 0.15f);
            cameraHolder.position = transform.GetComponent<CameraMove>().headTransform.position;

            yield return null;
        }

        //Make seat push adjustment if necessary
        if (pushAdjustment != Vector2.zero)
        {
            SeatPush(physicalSeat, -1.0f * pushAdjustment);
            yield return shiftCoroutine;
        }

        cameraHolder.parent = transform.GetComponent<CameraMove>().headTransform;
        myAnimationController.setAnimatorBool("IsLeft", isLeft);
        myAnimationController.setAnimatorBool("GettingUp", true); //Trigger get up animation

        //If not captain, move the player DURING the animation because the animation happens in place
        if (captainMode == false)
        {
            Vector3 endPos = physicalSeat.transform.localPosition + physicalSeat.transform.GetChild(0).localPosition;
            if (isLeft == true)
            {
                endPos = physicalSeat.transform.localPosition + physicalSeat.transform.GetChild(1).localPosition;
            }
            yield return StartCoroutine(PlayerAnimationTransformationAdjustment(endPos));
        }

        seatChangeCoroutine = null;
    }

    //Orients player and camera for sit down
    IEnumerator RepositionPlayerAndCamera(Vector3 newPosition, float newRotation, float time)
    {
        Transform cameraHolder = transform.GetComponent<CameraMove>().cameraHolder;

        Vector3 startingPosition = transform.localPosition;
        float startingRotation = transform.localRotation.eulerAngles.y;
        float startingCamRotation = cameraHolder.localRotation.eulerAngles.x;
        if (newRotation == 0.0f && startingRotation > 180.0f)
        {
            startingRotation = 0.0f - (360.0f - startingRotation);
        }

        float animTime = time;
        while (animTime > 0.0f)
        {
            animTime = Mathf.Max(0.0f, animTime - Time.deltaTime);

            transform.localPosition = Vector3.Lerp(newPosition, startingPosition, animTime / time);
            transform.localRotation = Quaternion.Euler(0.0f, Mathf.Lerp(newRotation, startingRotation, animTime / time), 0.0f);
            cameraHolder.localRotation = Quaternion.Euler(Mathf.Lerp(30.0f, startingCamRotation, animTime / time), 0.0f, 0.0f);
            cameraHolder.position = transform.GetComponent<CameraMove>().headTransform.position;

            yield return null;
        }
    }

    //Returns true if currently shifting
    public bool IsShifting()
    {
        return (shiftCoroutine != null);
    }

    //Returns true if shifting or sitting/getting up
    public bool IsAnimating()
    {
        return (shiftCoroutine != null || seatChangeCoroutine != null);
    }

    //Called when shifting seat positions laterally (captain doesn't shift)
    public Coroutine SeatShift(GameObject physicalSeat, Vector2 pushAdjustment, Vector3 endShiftPosition)
    {
        if (shiftCoroutine != null)
        {
            StopCoroutine(shiftCoroutine);
        }

        shiftCoroutine = StartCoroutine(Shift(physicalSeat, pushAdjustment, endShiftPosition));
        PrimaryScript.Instance.onShiftChange();
        return shiftCoroutine;
    }

    //Called when sitting down or getting up
    public void SeatPush(GameObject physicalSeat, Vector2 pushAdjustment)
    {
        if (shiftCoroutine != null)
        {
            StopCoroutine(shiftCoroutine);
        }

        shiftCoroutine = StartCoroutine(Push(physicalSeat, pushAdjustment));
        PrimaryScript.Instance.onShiftChange();
    }

    //Pushes the player in or out during sit down or get up animations
    IEnumerator Push(GameObject physicalSeat, Vector2 adjustment)
    {
        if (physicalSeat == null)
        {
            yield break;
        }

        Vector3 personStartPos = transform.localPosition;
        Vector3 personEndPos = new Vector3(personStartPos.x + adjustment.x, personStartPos.y, personStartPos.z + adjustment.y);
        Vector3 seatStartPos = physicalSeat.transform.localPosition;
        Vector3 seatEndPos = new Vector3(seatStartPos.x + adjustment.x, seatStartPos.y, seatStartPos.z + adjustment.y);

        float animTime = PUSH_TIME;
        while (animTime > 0.0f)
        {
            animTime = Mathf.Max(0.0f, animTime - Time.deltaTime);

            transform.localPosition = Vector3.Lerp(personEndPos, personStartPos, animTime / PUSH_TIME);
            if (physicalSeat != null)
            {
                physicalSeat.transform.localPosition = Vector3.Lerp(seatEndPos, seatStartPos, animTime / PUSH_TIME);
            }

            yield return null;
        }

        shiftCoroutine = null;
        PrimaryScript.Instance.onShiftChange();
    }

    //Moves the player and seat during shifts
    IEnumerator Shift(GameObject physicalSeat, Vector2 pushAdjustment, Vector3 endShiftPosition)
    {
        Vector3 startPosition = physicalSeat.transform.localPosition;
        Vector3 pushDir = new Vector3(pushAdjustment.x, 0.0f, pushAdjustment.y);
        endShiftPosition += pushDir;

        Vector3 offset = physicalSeat.transform.localPosition - transform.localPosition;

        float totalShiftTime = Vector3.Distance(startPosition, endShiftPosition) / SHIFT_SPEED;
        float shiftTime = totalShiftTime;

        while (shiftTime > 0.0f)
        {
            float dt = Mathf.Min(Time.deltaTime, 1.0f / 30.0f);
            shiftTime = Mathf.Max(0.0f, shiftTime - dt);
            transform.localPosition = Vector3.Lerp(endShiftPosition, startPosition, shiftTime / totalShiftTime) - offset;
            if (physicalSeat != null)
            {
                physicalSeat.transform.localPosition = Vector3.Lerp(endShiftPosition, startPosition, shiftTime / totalShiftTime);
            }

            yield return null;
        }

        shiftCoroutine = null;
        PrimaryScript.Instance.onShiftChange();
    }

    //Runs on Update() time
    IEnumerator CheckForMovement()
    {
        while (true)
        {
            yield return null;
            UpdateMovement();
        }
    }

    //Checks inputs and triggers move
    private void UpdateMovement()
    {
        if (!PrimaryScript.Instance.isPaused())
        {
            moveDir.x = Input.GetAxis("Horizontal");
            moveDir.y = Input.GetAxis("Vertical");
        }
        else
        {
            moveDir.x = 0.0f;
            moveDir.y = 0.0f;
        }

        if (moveDir.magnitude > 1)
        {
            moveDir.Normalize();
        }
        Move();

        //Teleport back if you fall
        if (transform.localPosition.y < -10)
        {
            transform.localPosition = Vector3.zero;
            playerRB.linearVelocity = Vector3.zero;
        }
    }

    private void Move()
    {
        Vector3 movement; //= Vector3.zero;

        myAnimationController.setAnimatorFloat("Movement", moveDir.magnitude);
        myAnimationController.setAnimatorFloat("Forward", moveDir.y);

        if (transform.parent != null) //Local movement
        {
            Quaternion combinedRotation = transform.parent.rotation * transform.localRotation;
            Vector3 localMovement = new Vector3(moveDir.x, 0, moveDir.y) * MOVE_SPEED * Time.deltaTime;
            movement = combinedRotation * localMovement;
            transform.position += movement;
        }
        else //World movement
        {
            movement = transform.TransformDirection(new Vector3(moveDir.x, 0, moveDir.y)) * MOVE_SPEED * Time.deltaTime;
            transform.position += movement;
        }
    }
}