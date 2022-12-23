using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using TMPro;
using UnityEngine.SceneManagement;

public class SlimeController : MonoBehaviour
{
    [SerializeField]
    private Camera playerCamera;
    private Vector3 normalVector = Vector3.up;
    public float maxSlopeAngle = 35f;
    public Transform orientation;
    Vector3 moveDirection;
    public float moveSpeed;
    public float airMultiplier;

    public TextMeshProUGUI countText;

    private Rigidbody rb;

    private PlayerControl pi;
    private InputAction move;
    private float liqCount = 0;

    [SerializeField]
    private float movementForce = 1f;
    [SerializeField]
    private float jumpForce = 5f;
    [SerializeField]
    private float maxSpeed = 5f;
    private Vector3 forceDirection = Vector3.zero;

    Animator anim;

    //jumping variables
    bool isJumpPressed = false;
    bool grounded = false;

    public LayerMask whatIsGround;

    //Jumping
    private bool readyToJump = true;
    private float jumpCooldown = 0.25f;

    //Gas Form "Double Jump"
    public int startDoubleJumps = 1;
    int doubleJumpsLeft;

    // Start is called before the first frame update
    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        anim = GetComponent<Animator>();
        pi = new PlayerControl();
        pi.Player.Fire.performed += ctx => changeToLiquid();
        pi.Player.Jump.started += onJump;
        pi.Player.Jump.canceled += onJump;
        pi.Player.Reset.performed += ctx => resetStage();
        //SetCountText();
    }

    void onJump(InputAction.CallbackContext ctx) {
        isJumpPressed = ctx.ReadValueAsButton();
        //If holding jump && ready to jump, then jump
        if (readyToJump && grounded) JumpCheck();
    }

    private void FixedUpdate()
    {
        forceDirection += move.ReadValue<Vector2>().x * GetCameraRight(playerCamera) * movementForce;
        forceDirection += move.ReadValue<Vector2>().y * GetCameraForward(playerCamera) * movementForce;

        rb.AddForce(forceDirection, ForceMode.Impulse);
        forceDirection = Vector3.zero;

        if (rb.velocity.y < 0f)
            rb.velocity -= Vector3.down * Physics.gravity.y * Time.fixedDeltaTime;

        Vector3 horizontalVelocity = rb.velocity;
        horizontalVelocity.y = 0;
        if (horizontalVelocity.sqrMagnitude > maxSpeed * maxSpeed)
            rb.velocity = horizontalVelocity.normalized * maxSpeed + Vector3.up * rb.velocity.y;

        LookAt();
        //MovePlayer();
    }

    private void LookAt()
    {
        Vector3 direction = rb.velocity;
        direction.y = 0f;

        if (move.ReadValue<Vector2>().sqrMagnitude > 0.1f && direction.sqrMagnitude > 0.1f)
            this.rb.rotation = Quaternion.LookRotation(direction, Vector3.up);
        else
            rb.angularVelocity = Vector3.zero;
    }

    private Vector3 GetCameraForward(Camera playerCamera)
    {
        Vector3 forward = playerCamera.transform.forward;
        forward.y = 0;
        return forward.normalized;
    }

    private Vector3 GetCameraRight(Camera playerCamera)
    {
        Vector3 right = playerCamera.transform.right;
        right.y = 0;
        return right.normalized;
    }

    void changeToLiquid()
    {
        if (anim.GetBool("basetoliq") == false)
        {   
            if(liqCount >= 1)
            {
                Debug.Log("F'd");
                anim.SetBool("basetoliq", true);
                liqCount -= 1;
                SetCountText();
            }
        }
        else
        {
            anim.SetBool("basetoliq", false);
        }
    }

    void OnEnable()
    {
        move = pi.Player.Move;
        pi.Player.Enable();
    }

    void OnDisable()
    {
        pi.Player.Disable();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("PickUp"))
        {
            other.gameObject.SetActive(false);
            liqCount += 1;
            SetCountText();
        }

        if (other.gameObject.CompareTag("Barrier"))
        {
            other.gameObject.SetActive(false);
            changeToLiquid();
        }

        if (other.gameObject.CompareTag("Enemy_Head"))
        {
            Debug.Log("Enemy Head");
            Jump();
            
            GameObject enemy = other.gameObject.transform.root.gameObject;
            Debug.Log(enemy.name);
            if(enemy.TryGetComponent<EnemyAI>(out EnemyAI aiComponent))
                aiComponent.TakeDamage(10);
        }
    }
#region Jumping
    private bool cancellingGrounded;
    private void OnCollisionStay(Collision other) {
        
        //Make sure we are only checking for walkable layers
        int layer = other.gameObject.layer;
        if (whatIsGround != (whatIsGround | (1 << layer))) return;

        //Iterate through every collision in a physics update
        for (int i = 0; i < other.contactCount; i++)
        {
            Vector3 normal = other.contacts[i].normal;
            //FLOOR
            if (IsFloor(normal))
            {
                grounded = true;
                cancellingGrounded = false;
                normalVector = normal;
                CancelInvoke(nameof(StopGrounded));
            }
        }
        float delay = 3f;
        if (!cancellingGrounded)
        {
            cancellingGrounded = true;
            Invoke(nameof(StopGrounded), Time.deltaTime * delay);
        }
    }

    private void StopGrounded()
    {
        grounded = false;
    }
#endregion

    void SetCountText()
    {
        countText.text = "Liquid Data: " + liqCount.ToString();
    }

    void JumpCheck() {
        if (grounded) { 
            Jump();
        }
    }

    void Jump() {
        readyToJump = false;

        //Add jump forces
        rb.AddForce(Vector3.up * jumpForce * 1.5f);
        rb.AddForce(normalVector * jumpForce * 0.5f);

        //If jumping while falling, reset y velocity.
        Vector3 vel = rb.velocity;
        if (rb.velocity.y < 0.5f)
            rb.velocity = new Vector3(vel.x, 0, vel.z);
        else if (rb.velocity.y > 0)
            rb.velocity = new Vector3(vel.x, vel.y / 2, vel.z);

        Invoke(nameof(ResetJump), jumpCooldown);
    }

    private void ResetJump()
    {
        readyToJump = true;
    }

    private bool IsFloor(Vector3 v)
    {
        float angle = Vector3.Angle(Vector3.up, v);
        return angle < maxSlopeAngle;
    }

#region Debugging Methods
    private void resetStage() {
        SceneManager.LoadScene("LevelSelector");
    }
#endregion
}