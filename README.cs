using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    #region Variables
    [HideInInspector]
    public CharacterController controller;

    //Public Variables
    public float LookSensitivity = 100f;
    public float MoveSpeed;
    public float JumpForce;

    //Sprint Variables
    float BaseSpeed;
    bool IsSprinting;

    //Crouching Variables
    bool IsCrouching;
    float CrouchChangeSpeed;
    float BaseHeight;

    //Camera Fov Variables
    float FovChangeSpeed;
    float BaseFov;

    //Ground Interaction Variables
    float gravity = -19.81f;
    float BodyXRot;
    float GroundDistance = 0.2f;
    bool isGrounded;
    Transform groundCheck;
    LayerMask groundMask;
    Vector3 velocity;

    //Player Camera Variables
    Camera PlayerCamera;

    [Space(10)]
    [SerializeField] private bool ChangeCameraFovOnSprint;
    #endregion

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        PlayerCamera = GetComponentInChildren<Camera>();
    }

    void Start()
    {
        StartingVariables();
    }

    void Update()
    {
        Look();
        Move();
        Jump();
    }

    void StartingVariables()
    {
        SpawnGroundCheck();
        Cursor.lockState = CursorLockMode.Locked;
        groundMask = LayerMask.GetMask("Ground");

        BaseFov = PlayerCamera.fieldOfView;
        BaseHeight = controller.height;
    }

    void SpawnGroundCheck()
    {
        CapsuleCollider PlayerCapsule = GetComponent<CapsuleCollider>();

        GameObject NewGroundCheck = new GameObject("GroundCheckObj");
        NewGroundCheck.transform.parent = transform;
        NewGroundCheck.transform.position = transform.position + Vector3.down * (PlayerCapsule.height / 2);

        groundCheck = NewGroundCheck.transform;
    }

    void Look()
    {
        float mouseX = Input.GetAxis("Mouse X") * LookSensitivity * Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y") * LookSensitivity * Time.deltaTime;

        BodyXRot -= mouseY;
        BodyXRot = Mathf.Clamp(BodyXRot, -90f, 90f);

        PlayerCamera.gameObject.transform.localRotation = Quaternion.Euler(BodyXRot, 0, 0);
        transform.Rotate(Vector3.up * mouseX);
    }

    void Move()
    {
        if (!IsSprinting && isGrounded) Crouch();
        if (!IsCrouching) Sprint();

        float x = Input.GetAxis("Horizontal");
        float z = Input.GetAxis("Vertical");

        Vector3 move = transform.right * x + transform.forward * z;
        controller.Move(move * BaseSpeed * Time.deltaTime);

        //Gravity
        velocity.y += gravity * Time.deltaTime;
        controller.Move(velocity * Time.deltaTime);
    }

    void Jump()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, GroundDistance, groundMask);

        if (isGrounded && velocity.y < 0) velocity.y = -2f;

        if (Input.GetButtonDown("Jump") && isGrounded) velocity.y = Mathf.Sqrt(JumpForce * -2f * gravity);
    }

    void Sprint()
    {
        if (Input.GetKeyDown(KeyCode.LeftShift) && !IsSprinting) IsSprinting = true;
        if (Input.GetKeyUp(KeyCode.LeftShift) && IsSprinting) IsSprinting = false;

        if (IsSprinting) BaseSpeed = MoveSpeed * 2;
        else BaseSpeed = MoveSpeed;

        //Changes the Cameras fov when Sprinting
        if (ChangeCameraFovOnSprint)
        {
            if (IsSprinting)
            { if (FovChangeSpeed < 1) FovChangeSpeed = FovChangeSpeed + 5 * Time.deltaTime; }
            else
            { if (FovChangeSpeed > 0) FovChangeSpeed = FovChangeSpeed - 5 * Time.deltaTime; }

            PlayerCamera.fieldOfView = Mathf.Lerp(BaseFov, BaseFov + 10, FovChangeSpeed);
        }
    }

    void Crouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl) && !IsCrouching) IsCrouching = true;
        if (Input.GetKeyUp(KeyCode.LeftControl) && IsCrouching) IsCrouching = false;

        if (IsCrouching) BaseSpeed = MoveSpeed / 2;
        else BaseSpeed = MoveSpeed;

        if (IsCrouching)
        { if (CrouchChangeSpeed < 1) CrouchChangeSpeed = CrouchChangeSpeed + 5 * Time.deltaTime; }
        else
        { if (CrouchChangeSpeed > 0) CrouchChangeSpeed = CrouchChangeSpeed - 5 * Time.deltaTime; }

        controller.height = Mathf.Lerp(BaseHeight, BaseHeight / 2, CrouchChangeSpeed);

    }
}
