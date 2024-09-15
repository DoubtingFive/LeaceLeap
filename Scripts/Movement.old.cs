using UnityEngine;
using UnityEngine.InputSystem;
using IngameDebugConsole;
using TMPro;

public class Movement : MonoBehaviour
{
    Transform orientation;
    float yRot = 0;
    static float sens = 0.2f;

    [SerializeField] float speed = 80;
    [SerializeField] float groundMaxSpeed = 10;
    [SerializeField] float airMultiplayer = 0;
    Vector2 moveDir;
    Vector2 maxSpeedController;
    Rigidbody rb;
    float maxSpeed;
    bool yToggle = false;
    bool xToggle = false;

    [SerializeField] LayerMask ground;
    [SerializeField] float jumpForce = 6;
    [SerializeField] float jumpCooldown = 0.15f;
    [SerializeField] float groundOffset = 0.78f;
    [SerializeField] float groundSize = 0.35f;
    [SerializeField] float drag = 10;
    [SerializeField] float airMaxSpeed = 13;
    Vector3 limitVel;
    bool jumpReady = true;
    bool onGround;
    bool jumping = false;
    bool isConsoleOut = false;
    bool firstJump;
    bool showSpeed = false;

    [SerializeField] TextMeshProUGUI speedText;
    [SerializeField] static GameObject timeScaleText;
    private void Start()
    {
        showSpeed = speedText.gameObject.activeSelf;
        DebugLogConsole.AddCommand("toggleSpeed", "toggles speed console real-time", ToggleSpeed);
        timeScaleText = GameObject.FindGameObjectWithTag("timeText");
        timeScaleText.SetActive(false);
        maxSpeed = groundMaxSpeed;
        orientation = transform.Find("Orientation");
        rb = GetComponent<Rigidbody>();
        rb.drag = drag;
        Cursor.lockState = CursorLockMode.Locked;
    }
    private void Update()
    {
        limitVel = rb.velocity;
        limitVel.y = 0;
        rb.velocity = limitVel.magnitude * transform.forward + rb.velocity.y * Vector3.up;
        if (moveDir.x != 0 || moveDir.y != 0)
        {
            Vector3 _moveDir = moveDir.x * transform.right + moveDir.y * transform.forward;
            rb.AddForce(_moveDir * speed * Time.deltaTime * (onGround?1:airMultiplayer));
            //limitVel = rb.velocity;
            //limitVel.y = 0;
            if (limitVel.magnitude > maxSpeed)
            {
                if (maxSpeed > groundMaxSpeed) { maxSpeed -= Time.deltaTime*airMaxSpeed*4; }
                rb.velocity = Vector3.ClampMagnitude(limitVel, maxSpeed) + (Vector3.up*rb.velocity.y);
            }
        }
        if (showSpeed) speedText.text = "JumpVelocity: " + rb.velocity.y + "\nrb.velocity: " + rb.velocity + "\nmaxSpeed: " + maxSpeed + "\nrb.velocity.magnitude: " + rb.velocity.magnitude + "\nlimitVel.magnitude: " + limitVel.magnitude + "\nFirstJump: " + firstJump;

        onGround = Physics.CheckSphere(transform.position - (Vector3.up * groundOffset), groundSize, ground);
        if (jumping)
        {
            if (maxSpeed > groundMaxSpeed) { maxSpeed -= Time.deltaTime * airMaxSpeed/4; 
                rb.velocity = Vector3.ClampMagnitude(limitVel, maxSpeed) + (Vector3.up * rb.velocity.y); }
            if (jumpReady && onGround)
            {
                if (firstJump)
                {
                    rb.drag = 0;
                    maxSpeed = airMaxSpeed + groundMaxSpeed;
                    firstJump = false;
                }
                onGround = false;
                jumpReady = false;
                maxSpeed = (airMaxSpeed + groundMaxSpeed + maxSpeed+3)/2;
                Vector3 jump = rb.velocity;
                jump.y = jumpForce;
                rb.velocity = jump;
                Invoke("JumpReset", jumpCooldown);
            }
        }
        if (onGround && rb.drag == 0 && jumpReady)
        {
            firstJump = true;
            rb.drag = drag;
            maxSpeed = groundMaxSpeed;
        }
        else if (!onGround && rb.drag != 0) { rb.drag = 0; }
        if (transform.position.y < 0) { transform.position =  new Vector3(0, 2, 0); }
    }
    public void Move(InputAction.CallbackContext context)
    {
        if (isConsoleOut) return;
        moveDir = context.ReadValue<Vector2>();
        if (xToggle) { maxSpeedController.x = moveDir.x; moveDir.x = 0; }
        if (yToggle) { maxSpeedController.y = moveDir.y; moveDir.y = 0; }
    }
    public void Mouse(InputAction.CallbackContext context)
    {
        if (isConsoleOut) return;
        Vector2 dir = context.ReadValue<Vector2>();
        dir *= sens;
        transform.Rotate(0, dir.x, 0);
        yRot -= dir.y;
        yRot = Mathf.Clamp(yRot, -90, 90);
        orientation.localRotation = Quaternion.Euler(yRot, 0, 0);
    }
    public void Jump(InputAction.CallbackContext context)
    {
        if (isConsoleOut) return;
        if (context.performed) { jumping = true; }
        else if (context.canceled) { jumping = false; }
    }
    public void Console(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            if (isConsoleOut)
            {
                isConsoleOut = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                isConsoleOut = true;
                Cursor.lockState = CursorLockMode.None;
            }
        }
    }
    void JumpReset()
    {
        jumpReady = true;
    }
    void ToggleSpeed()
    {
        showSpeed = !showSpeed;
        if (showSpeed)
        {
            speedText.transform.parent.gameObject.SetActive(true);
        }
        else
        {
            speedText.transform.parent.gameObject.SetActive(false);
        }
    }
    [ConsoleMethod("timescale","Set time scale of game. Default is 1.")]
    public static void SetTimeScale(float _timeScale)
    {
        Time.timeScale = _timeScale;
        timeScaleText.SetActive(true);
    }
    [ConsoleMethod("sens", "Set mouse sensitivity. Default is 0,2.")]
    public static void SetSensitivity(float _sens)
    {
        sens = _sens;
    }
    private void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position - (Vector3.up * groundOffset), groundSize);
    }
}
