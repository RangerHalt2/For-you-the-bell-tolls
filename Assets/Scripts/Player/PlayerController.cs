using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour, IController
{

    private float xAxis, yAxis;
    public Rigidbody2D rb;
    Animator anim;
    /*
    [Header("Inputs")]
    private IA_Main playerControls;
    private PlayerInput playerInput;
    */

    private InputManager inputManager;

    [Header("Horizontal Movement")]
    [SerializeField] private float walkSpeed = 1;
    [Space(5)]

    [Header("Vertical Movement")]
    [SerializeField] private bool jumping;
    [SerializeField] private float jumpForce = 35;
    private float jumpBufferCounter;
    [SerializeField] private float jumpBufferFrames;
    private float coyoteTimeCounter = 0;
    [SerializeField] private float coyoteTime;
    private int airJumpCounter = 0;
    [SerializeField] private int maxAirJumps;
    [Space(5)]

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheckPoint;
    [SerializeField] private float groundCheckY = 0.2f;
    [SerializeField] private float groundCheckX = 0.5f;
    [SerializeField] private LayerMask whatIsGround;
    [Space(5)]

    [Header("Dashes")]
    [SerializeField] private bool canDash = true;
    [SerializeField] private float dashSpeed;
    [SerializeField] private float dashTime;
    [SerializeField] private float dashCooldown;
    [SerializeField] private bool dashed = false;
    [SerializeField] GameObject dashEffect;
    [Space(5)]

    [Header("Attacking")]
    bool attack = false;
    [SerializeField] private float timeBetweenAttack;
    private float timeSinceAttack;
    [SerializeField] private Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] private Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    [SerializeField] GameObject slashEffect;

    [Header("Recoil")]
    [SerializeField] int recoilXSteps = 5;
    [SerializeField] int recoilYSteps = 5;
    [SerializeField] float recoilXSpeed = 100;
    [SerializeField] float recoilYSpeed = 100;
    int stepsXRecoiled, stepsYRecoiled;
    [Space(5)]

    [Header("Health")] //This is all health from visuals to numbers
    public int health;
    public int maxHealth;
    [SerializeField] float hitFlashSpeed;
    public delegate void OnHealthChagnedDelegate();
    [HideInInspector] public OnHealthChagnedDelegate onHealthChangedCallback;
    float healTimer;
    [SerializeField] float timeToHeal;

    [SerializeField] GameObject damageEffect;
    [Space(5)]

    private bool canFlash = true; //This is a damage effect. The player is supposed to flash when taking damage.

    public static PlayerController Instance;
    [HideInInspector] public PlayerStateList pState;
    private float gravity;
    private SpriteRenderer sr;

    //LB: a variable to block all inputs when the player is dead
    private bool isDead = false;
    //Update and tell the controller that the player is dead
    public void SetIsDead(bool isDead)
    {
        this.isDead = isDead;
    }
    
    private void Awake()
    {
        /*
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);
        */
        timeSinceAttack = timeBetweenAttack;
    }

    // Start is called before the first frame update
    void Start()
    {
        inputManager = GameObject.FindAnyObjectByType<InputManager>();
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pState = GetComponent<PlayerStateList>();
        gravity = rb.gravityScale;
        sr = GetComponentInChildren<SpriteRenderer>();
    }
    
    private void OnDrawGizmos() //Visualize, in editor, where the attacks are. That's all
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireCube(SideAttackTransform.position, SideAttackArea);
        Gizmos.DrawWireCube(UpAttackTransform.position, UpAttackArea);
        Gizmos.DrawWireCube(DownAttackTransform.position, DownAttackArea);
    }
    

    // Update is called once per frame
    void Update()
    {
        if (pState.cutscene) return;

        JumpingVariableConfirm();

        GetInputs();
        UpdateJumpVariables();

        if (pState.dashing) return;
        FlashWhileInvincible();
        Move();

        if (pState.healing) return;

        //Updates jumps only if not healing

        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping) //Essentially says if you're not jumping, try to, and are not out of Coyote Time, jump
        {                                                                      //The jump method MAKES jumpBufferCounter > 0 through UpdateJumpVariables
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
            pState.jumping = true;
        }

        if (inputManager.AttackInput) Attack();

        if (inputManager.JumpInput) OnJump();

        if (Grounded())
        {
            dashed = false;
        }
        if(canDash && !dashed && inputManager.DashInput)
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        //anim.SetBool("Jumping", !Grounded());

        Flip();

    }

    private void FixedUpdate()
    {
        //These wrench control from the player in any of these 3 cases.

        if (pState.cutscene) return;

        if (pState.dashing) return;

        Recoil();
    }

    private void JumpingVariableConfirm()
    {
        if (inputManager.JumpInput)
        {
            jumping = true;
        }
        else
        {
            jumping = false;
        }
    }

    void GetInputs() //Takes player inputs, namely WASD type inputs right now.
    {
        xAxis = inputManager.MoveInput.x;
        yAxis = inputManager.MoveInput.y;
    }

    void Flip() //Simple just flip the PC if they're walking left as opposed to right. This flips what counts as "forward" for stuff like attacking as well.
    {
        if (xAxis < 0)
        {
            transform.localScale = new Vector2(transform.localScale.y * -1, transform.localScale.y);
            pState.lookingRight = false;
        }
        else if (xAxis > 0)
        {
            transform.localScale = new Vector2(transform.localScale.y * 1, transform.localScale.y);
            pState.lookingRight = true;
        }
    }

    private void Move() //Wonder what this does
    {
        rb.linearVelocity = new Vector2(walkSpeed * xAxis, rb.linearVelocity.y);
        //anim.SetBool("Walking", rb.linearVelocity.x != 0 && Grounded());
    }

    

    IEnumerator Dash() //Calculation and application of dash direction and magnitude
    {
        canDash = false;
        pState.dashing = true;
        pState.invincible = true;
        //anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        int _dirX = pState.lookingRight ? 1 : -1;
        if (xAxis < 0.15 && xAxis > -0.15) { _dirX = 0; }
        int _dirY = 0;
        if (yAxis > 0.15) { _dirY = 1; }
        else if (yAxis < -0.15) { _dirY = -1; }
        float _dirMult = 1f;
        if (yAxis > 0.15 && xAxis > 0.15 || yAxis < -0.15 && xAxis < -0.15 ||
            yAxis < -0.15 && xAxis > 0.15 || yAxis > 0.15 && xAxis < -0.15) { _dirMult = 0.75f; }
        if (_dirX == 0 && _dirY == 0) { _dirX = pState.lookingRight ? 1 : -1; }
        rb.linearVelocity = new Vector2(_dirX * dashSpeed * _dirMult, _dirY * dashSpeed * _dirMult);
        //if (Grounded()) Instantiate(dashEffect, transform); //This is a dash effect on the ground. Not neccesary.
        gameObject.layer = LayerMask.NameToLayer("Warping");
        yield return new WaitForSeconds(dashTime);
        rb.linearVelocity = new Vector2(0, 0);
        gameObject.layer = LayerMask.NameToLayer("Player");
        rb.gravityScale = gravity;
        pState.invincible = false;
        pState.dashing = false;
        yield return new WaitForSeconds(dashCooldown);
        canDash = true;
    }

    public IEnumerator WalkIntoNewScene(Vector2 _exitDir, float _delay) //Cutscene. Wrenches control from player and cuts the scene out.
    {
        pState.invincible = true;

        //if exit direction is upwards
        if (_exitDir.y > 0)
        {
            rb.linearVelocity = jumpForce * _exitDir;
        }

        //if exit direction requires horizontal movement
        if (_exitDir.x > 0)
        {
            xAxis = _exitDir.x > 0 ? 1 : -1;

            Move();
        }

        Flip();
        yield return new WaitForSeconds(_delay);
        pState.invincible = false;
        pState.cutscene = false;
    }

    private void Attack() //Wonder what this does. Checks time since attack, and sets the attack in the transform that's previously set
    {
        Debug.Log("Attacked!");
        timeSinceAttack += Time.deltaTime;
        if (timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            //anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                int _recoilLeftOrRight = pState.lookingRight ? 1 : -1;

                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * _recoilLeftOrRight, recoilXSpeed);
                //Instantiate(slashEffect, SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                //SlashEffectAtAngle(slashEffect, 90, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down, recoilYSpeed);
                //SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
            }
        }
    }

    private void Hit(Transform _attackTransform, Vector2 _attackArea, ref bool _recoilBool, Vector2 _recoilDir, float _recoilStrength) //If the player deals damage. Can do multiple enemies
    {
        Collider2D[] objectsToHit = Physics2D.OverlapBoxAll(_attackTransform.position, _attackArea, 0, attackableLayer);
        List<EnemyController> hitEnemies = new List<EnemyController>();

        if (objectsToHit.Length > 0)
        {
            _recoilBool = true;
        }

        for (int i = 0; i < objectsToHit.Length; i++)
        {
            EnemyController e = objectsToHit[i].GetComponent<EnemyController>();
            if (e != null && !hitEnemies.Contains(e))
            {
                //e.EnemyHit(damage, _recoilDir, _recoilStrength);
                hitEnemies.Add(e);

                if (objectsToHit[i].CompareTag("Enemy"))
                {
                    //Mana += manaGain;
                }
            }
        }
    }

    void SlashEffectAtAngle(GameObject _slashEffect, int _effectAngle, Transform _attackTransform) //This was never implimented but it took a slash animation and just animated it.
    {
        _slashEffect = Instantiate(_slashEffect, _attackTransform);
        _slashEffect.transform.eulerAngles = new Vector3(0, 0, _effectAngle);
        _slashEffect.transform.localScale = new Vector2(transform.localScale.x, DownAttackTransform.localScale.y);
    }

    void Recoil() //If the player is hit, they fly back
    {
        if (pState.recoilingX)
        {
            if (pState.lookingRight)
            {
                rb.linearVelocity = new Vector2(-recoilXSpeed, 0);
            }
            else
            {
                rb.linearVelocity = new Vector2(recoilXSpeed, 0);
            }
        }

        if (pState.recoilingY)
        {
            rb.gravityScale = 0;
            if (yAxis < 0)
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, recoilYSpeed);
            }
            else
            {
                rb.linearVelocity = new Vector2(rb.linearVelocity.x, -recoilYSpeed);
            }
            airJumpCounter = 0;
        }
        else
        {
            rb.gravityScale = gravity;
        }

        //Measures how long you've recoiled both y and x. Each separately. Used for dynamic recoil based on player position and their surroundings.
        if (pState.recoilingX && stepsXRecoiled < recoilXSteps)
        {
            stepsXRecoiled++;
        }
        else
        {
            StopRecoilX();
        }
        if (pState.recoilingY && stepsYRecoiled < recoilYSteps)
        {
            stepsYRecoiled++;
        }
        else
        {
            StopRecoilY();
        }

        if (Grounded())
        {
            StopRecoilY();
        }
    }

    void StopRecoilX()
    {
        stepsXRecoiled = 0;
        pState.recoilingX = false;
    }
    void StopRecoilY()
    {
        stepsYRecoiled = 0;
        pState.recoilingY = false;
    }

    IEnumerator Flash() //This animated the character to flash.
    {
        sr.enabled = !sr.enabled;
        canFlash = false;
        yield return new WaitForSeconds(0.2f);
        canFlash = true;
    }

    void FlashWhileInvincible() //Also animated the character to flash
    {
        if (pState.invincible && !pState.cutscene)
        {
            if (Time.timeScale > 0.2 && canFlash)
            {
                StartCoroutine(Flash());
            }
        }
        else
        {
            sr.enabled = true;
        }
    }


    public bool Grounded() //Uses raycast instead of a collider to check for the ground.
    {
        if (Physics2D.Raycast(groundCheckPoint.position, Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround)
            || Physics2D.Raycast(groundCheckPoint.position + new Vector3(-groundCheckX, 0, 0), Vector2.down, groundCheckY, whatIsGround))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private void OnJump() //LB: Please actually explain how the function does stuff if it's obvious what the function is doing, don't goof on me >:(
    {
        jumping = true;

        if (!Grounded() && airJumpCounter < maxAirJumps) //Multijump! Can be set to any number of jumps, but isn't required.
        {
            pState.jumping = true;

            airJumpCounter++;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
        }


    }

    void UpdateJumpVariables()
    {
        if (Grounded())
        {
            //Debug.Log("Grounded");
        }
        else if (!Grounded())
        {
            //Debug.Log("Not Grounded!");
        }
        if (Grounded())
        {
            pState.jumping = false;
            coyoteTimeCounter = coyoteTime;
            airJumpCounter = 0;
        }
        else
        {
            coyoteTimeCounter -= Time.deltaTime;
        }

        if (jumping)
        {
            jumpBufferCounter = jumpBufferFrames;
        }

        else
        {
            jumpBufferCounter = jumpBufferCounter - Time.deltaTime * 10;
        }
    }
    
    // Enables the player controller component
    public void EnableControl()
    {
        this.enabled = true;
    }

    // Disables the player controller component
    public void DisableControl()
    {
        this.enabled = false;
    }
}
