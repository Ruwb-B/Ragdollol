using System.Collections;
//using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    //=== Basic properties ===//

    //Movement
    public float moveSpeed = 25000f;
    
    //Jumping
    public float jumpForce = 100000f;
    public float jumpDelayTime = 0.5f;

    //=== Assingables ===//
    public Rigidbody hip;
    public Transform Camera;

    //Animator
    public Animator targetAnimator;
    
    //Hand trigger
    public InverseKinematics leftHandIK;
    public InverseKinematics rightHandIK;
    private bool leftHandOn = false;
    private bool rightHandOn = false;

    //=== Other properties ===//

    //Movement
    [HideInInspector] public bool isGrounded = true;
    //Jumping
    [HideInInspector] public bool jumpDelay = false;

    //Hip
    private Transform hipTransform; //For getting the direction player is facing
    private ConfigurableJoint hipJoint; //For rotating player

    //Input
    private float horizontal;
    private float vertical;
    private float jump;


    private void Start()
    { 
        //Assign the hip component
        hipTransform = hip.GetComponent<Transform>();
        hipJoint = hip.GetComponent<ConfigurableJoint>();
    }


    private void Update()
    {
        PlayerInput();
    }

    void FixedUpdate()
    {
        if (this.isGrounded)
        {
            Move();
        }
    }


    /// <summary>
    /// Find player's input. (Should put this in its own class)
    /// </summary>
    private void PlayerInput()
    {
        horizontal = Input.GetAxisRaw("Horizontal");
        vertical = Input.GetAxisRaw("Vertical");

        jump = Input.GetAxisRaw("Jump");


        if (Input.GetKeyDown(KeyCode.Q))
        {
            leftHandOn = !leftHandOn;
            EnableHand(leftHandIK, leftHandOn);
        }
        if (Input.GetKeyDown(KeyCode.E))
        {
            rightHandOn = !rightHandOn;
            EnableHand(rightHandIK, rightHandOn);
        }
    }

    /// <summary>
    /// Move, jump, rotate the player
    /// </summary>
    private void Move()
    {
        //This is the input direction (WASD)
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;

        if (jump >= 0.1f)
        {
            this.hip.AddForce(new Vector3(0f, jumpForce, 0f));

            this.isGrounded = false;
            this.jumpDelay = true;
            //Delay the FootOnGround script
            StartCoroutine(this.OverDelayJump());
        }
        else if (direction.magnitude >= 0.1f)
        {
            //Get the angle of the player relative to the camera angle
            float targetAngle = Mathf.Atan2(direction.z, direction.x) * Mathf.Rad2Deg - 90 - Camera.eulerAngles.y;

            //Rotate the hip to rotate the player
            this.hipJoint.targetRotation = Quaternion.Euler(0f, targetAngle, 0f);

            //Move the hip to move the player
            this.hip.AddForce(hipTransform.forward * this.moveSpeed * Time.deltaTime);

            //Switch to walk animation
            this.targetAnimator.SetBool("Walk", true);
        }
        else
        {
            //Switch to default animation
            this.targetAnimator.SetBool("Walk", false);
        }
    }


    /// <summary>
    /// Enable/Disable hand point toward target / IK hand
    /// </summary>
    private void EnableHand(InverseKinematics IK, bool enable)
    {
        IK.enabled = enable;

        ConfigurableJoint joint;
        JointDrive drive;
        Transform current = IK.GetComponent<Transform>();

        if (enable)
        {
            for (int i = 0; i < IK.chainLength; i++)
            {
                current = current.parent;
                joint = current.GetComponent<ConfigurableJoint>();

                drive = joint.angularXDrive;
                drive.positionSpring = 0f;
                joint.angularXDrive = drive;

                drive = joint.angularYZDrive;
                drive.positionSpring = 0f;
                joint.angularYZDrive = drive;
            }
        }
        else
        {
            for (int i = 0; i < IK.chainLength; i++)
            {
                current = current.parent;
                joint = current.GetComponent<ConfigurableJoint>();

                drive = joint.angularXDrive;
                drive.positionSpring = 800f;
                joint.angularXDrive = drive;

                drive = joint.angularYZDrive;
                drive.positionSpring = 800f;
                joint.angularYZDrive = drive;
            }
        }
    }


    IEnumerator OverDelayJump()
    {
        yield return new WaitForSeconds(jumpDelayTime);
        this.jumpDelay = false;
    }
}