using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInput))]
public class PlayerController : MonoBehaviour
{

    private float xAxis, yAxis;
    public Rigidbody2D rb;
    Animator anim;

    [Header("Inputs")]
    private IA_Main playerControls;
    private PlayerInput playerInput;

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
    [SerializeField] Transform SideAttackTransform, UpAttackTransform, DownAttackTransform;
    [SerializeField] Vector2 SideAttackArea, UpAttackArea, DownAttackArea;
    [SerializeField] LayerMask attackableLayer;
    [SerializeField] float damage;
    [SerializeField] GameObject slashEffect;

    bool restoreTime;
    float restoreTimeSpeed;
    [Space(5)]

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

    /*[Header("Mana")] //When you hit an enemy, gain mana. Use mana to cast spells, heal (probably taking this out), and seeing beyond
    [SerializeField] float mana;
    [SerializeField] float manaDrainSpeed;
    [SerializeField] float manaGain;
    [SerializeField] UnityEngine.UI.Image manaStorage;
    [Space(5)]

    [Header("Spell Casting")] //Side fireball and down blast and up blast (We don't neccesarily need this)
    [SerializeField] float manaSpellCost = 0.3f;
    [SerializeField] float timeBetweenCast = 0.5f;
    float timeSinceCast;
    [SerializeField] float spellDamage; //upspellexplosion and downspellfireball
    [SerializeField] float downSpellForce; // desolate dive

    [SerializeField] GameObject sideSpellFireball;
    [SerializeField] GameObject upSpellExplosion;
    [SerializeField] GameObject downSpellFireball;

    float castOrHealTimer;
    [Space(5)]*/

    [Header("Camera")]
    [SerializeField] private float playerFallSpeedThreshold = -10;

    private bool canFlash = true; //This is a damage effect. The player is supposed to flash when taking damage.

    public static PlayerController Instance;
    [HideInInspector] public PlayerStateList pState;
    private float gravity;
    private SpriteRenderer sr;


    private void Awake()
    {
        //Wake up player controls
        playerControls = new IA_Main();
        playerInput = GetComponent<PlayerInput>();

        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else 
        {
            Instance = this;
        }

        DontDestroyOnLoad(gameObject);

        Health = maxHealth;

        timeSinceAttack = timeBetweenAttack;
    }

    void OnEnable()
    {
        playerControls.Enable();
        playerControls.Player.Sprint.performed += StartSprint;
        playerControls.Player.Jump.performed += OnJump;
        playerControls.Player.Jump.canceled += OnJumpCanceled;
        playerControls.Player.Attack.performed += Attack;
    }

    void OnDisable()
    {
        playerControls.Disable();
        playerControls.Player.Sprint.performed -= StartSprint;
        playerControls.Player.Jump.performed -= OnJump;
        playerControls.Player.Jump.canceled -= OnJumpCanceled;
        playerControls.Player.Attack.performed -= Attack;
    }

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        anim = GetComponent<Animator>();
        pState = GetComponent<PlayerStateList>();
        gravity = rb.gravityScale;
        sr = GetComponent<SpriteRenderer>();
        //Mana = mana;
        //ERROR HERE BECAUSE NO MANA COMPONENT. We can either take this out or make mana
        //manaStorage.fillAmount = Mana;
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

        GetInputs();
        UpdateJumpVariables();
        UpdateCameraYDampForPlayerFall();

        RestoreTimeScale();

        if (pState.dashing) return;
        FlashWhileInvincible();
        Move();
        //Heal();
        //CastSpell();

        if(pState.healing) return;

        //Updates jumps only if not healing
        if (jumpBufferCounter > 0 && coyoteTimeCounter > 0 && !pState.jumping) //Essentially says if you're not jumping, try to, and are not out of Coyote Time, jump
            //The jump method MAKES jumpBufferCounter > 0 through UpdateJumpVariables
        {
            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
            pState.jumping = true;
        }

        //anim.SetBool("Jumping", !Grounded());

        Flip();

    }

    /*private void OnTriggerEnter2D(Collider2D _other) //for up and down cast spells
    {
        if (_other.GetComponent<EnemyController>() != null && pState.casting)
        {
            _other.GetComponent<EnemyController>().EnemyHit(spellDamage, (_other.transform.position - transform.position).normalized, -recoilYSpeed);
        }
    }*/ //Enemy doesn't exist yet, so this is technically effecting something that doesn't exist yet.

    //See Beyond, if we wanna do anything like that
    /*private void OnTriggerStay2D(Collider2D _other)  //Trigger for See Beyond
    {
        if (_other.CompareTag("Beyondible"))
        {
            SeeBeyond(_other);
        }
    }

    void SeeBeyond(Collider2D beyondible) //Destroys the beyondible and lets the player travel through.
    {
        if (Input.GetButtonDown("SeeBeyond")) 
        {
            Mana -= manaSpellCost;
            HitStopTime(0, 5, 0.5f);
            beyondible.gameObject.SetActive(false);
        }
    }*/

    private void FixedUpdate()
    {
        //These wrench control from the player in any of these 3 cases.

        if (pState.cutscene) return;

        if (pState.dashing) return;

        Recoil();
    }

    void GetInputs() //Takes player inputs, namely WASD type inputs right now.
    {
        xAxis = playerControls.Player.Move.ReadValue<Vector2>().x;
        yAxis = playerControls.Player.Move.ReadValue<Vector2>().y;

        /*if (Input.GetButton("Cast/Heal"))
        {
            castOrHealTimer += Time.deltaTime;
        }*/ 
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

    void UpdateCameraYDampForPlayerFall() //Makes the player's camera dynamic on falling.
    {
        //if falling past a certain speed threshold
        if (rb.linearVelocity.y < playerFallSpeedThreshold && !CameraManager.Instance.isLerpingYDamping && !CameraManager.Instance.hasLerpedYDamping) 
        {
            StartCoroutine(CameraManager.Instance.LerpYDamping(true));
        }

        //if standing still or moving up
        if (rb.linearVelocity.y >= 0 && !CameraManager.Instance.isLerpingYDamping && CameraManager.Instance.hasLerpedYDamping) 
        {
            //reset camera function
            CameraManager.Instance.hasLerpedYDamping = false;
            StartCoroutine(CameraManager.Instance.LerpYDamping(false));
        }
    }

    void StartSprint(InputAction.CallbackContext ctx) //The actual dash is the colorful coroutine that follows, but TL;DR, you can dash through certain things (Characters and objects)
    {
        if (canDash && !dashed && !pState.healing) 
        {
            StartCoroutine(Dash());
            dashed = true;
        }

        if (Grounded())
        { 
            dashed = false;
        }
    }
    
    IEnumerator Dash() //Calculation and application of dash direction and magnitude
    {
        canDash = false;
        pState.dashing = true;
        pState.invincible = true;
        //anim.SetTrigger("Dashing");
        rb.gravityScale = 0;
        int _dirX = pState.lookingRight ? 1 : -1;
        if(xAxis < 0.15 && xAxis > -0.15) { _dirX = 0; }
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
        gameObject.layer = LayerMask.NameToLayer("Default");
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

    void Attack(InputAction.CallbackContext ctx) //Wonder what this does. Checks time since attack, and sets the attack in the transform that's previously set
    {
        timeSinceAttack += Time.deltaTime;
        if (attack && timeSinceAttack >= timeBetweenAttack)
        {
            timeSinceAttack = 0;
            //anim.SetTrigger("Attacking");

            if (yAxis == 0 || yAxis < 0 && Grounded())
            {
                int _recoilLeftOrRight = pState.lookingRight ? 1 : -1;

                Hit(SideAttackTransform, SideAttackArea, ref pState.recoilingX, Vector2.right * _recoilLeftOrRight, recoilXSpeed);
                Instantiate(slashEffect, SideAttackTransform);
            }
            else if (yAxis > 0)
            {
                Hit(UpAttackTransform, UpAttackArea, ref pState.recoilingY, Vector2.up, recoilYSpeed);
                SlashEffectAtAngle(slashEffect, 90, UpAttackTransform);
            }
            else if (yAxis < 0 && !Grounded())
            {
                Hit(DownAttackTransform, DownAttackArea, ref pState.recoilingY, Vector2.down,  recoilYSpeed);
                SlashEffectAtAngle(slashEffect, -90, DownAttackTransform);
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

    public void TakeDamage(float _damage) //Damage and recoil are different. This method takes damage.
    {
        Health -= Mathf.RoundToInt(_damage);
        StartCoroutine(StopTakingDamage());
    }

    IEnumerator StopTakingDamage() //Makes the player invincible and works with animation and particles for damage.
    {
        pState.invincible = true;
        //GameObject _damageEffectParticles = Instantiate(damageEffect, transform.position, Quaternion.identity);
        //Destroy(_damageEffectParticles, 1.5f);
        //anim.SetTrigger("takeDamage");
        yield return new WaitForSeconds(1f);
        pState.invincible = false;
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

    void RestoreTimeScale() //Damage freezes time temporarily. This restores it. It's its own thing so it can be called in update
    {
        if (restoreTime)
        {
            if (Time.timeScale < 1)
            {
                Time.timeScale += Time.unscaledDeltaTime * restoreTimeSpeed;
            }
            else 
            {
                Time.timeScale = 1;
                restoreTime = false;
            }
        }
    }    
    
    public void HitStopTime(float _newTimeScale, int _restoreSpeed, float _delay) //Here's the time stop I was talking about when dealt damage.
    { 
        restoreTimeSpeed = _restoreSpeed;
        Time.timeScale = _newTimeScale;

        if (_delay > 0)
        {
            StopCoroutine(StartTimeAgain(_delay));
            StartCoroutine(StartTimeAgain(_delay));
        }
        else 
        {
            restoreTime = true;
        }
    }

    IEnumerator StartTimeAgain(float _delay)
    {
        yield return new WaitForSecondsRealtime(_delay);
        restoreTime = true;
    }

    public int Health //Handles the player's health as an int instead of a float, akin to Gungeon or HK.
    {
        get { return health; }
        set
        {
            if (health != value)
            {
                health = Mathf.Clamp(value, 0, maxHealth);

                if (onHealthChangedCallback != null) 
                {
                    onHealthChangedCallback.Invoke();
                }
            }
        }
    }

    /*void Heal() //HK healing based. We can take this out or not.
    {
        if (Input.GetButton("Cast/Heal") && castOrHealTimer > 0.05f && Health < maxHealth && Mana > 0 && Grounded() && !pState.dashing) 
        {
            //These three lines are community solutions to moving while healing.
            rb.linearVelocity = new Vector2(0, 0);
            anim.SetBool("Walking", false);
            anim.SetBool("Jumping", false);

            pState.healing = true;
            anim.SetBool("Healing", true);

            //healing
            healTimer += Time.deltaTime;
            if (healTimer >= timeToHeal) 
            {
                Health++;
                healTimer = 0;
            }

            //drain mana
            Mana -= Time.deltaTime * manaDrainSpeed;
        }
        else 
        {
            anim.SetBool("Healing", false);
            pState.healing = false;
            healTimer = 0;
        }
    }

    float Mana //HK mana based
    {
        get { return mana; }
        set
        {
            if (mana != value) 
            {
                mana = Mathf.Clamp(value, 0, 1);
                manaStorage.fillAmount = Mana;
            }
        }
    }*/

    /*void CastSpell() //HK spells based. Fireball, ground pound, and up explosion. This determines
    {
        if (Input.GetButtonUp("Cast/Heal") && castOrHealTimer <= 0.1f && timeSinceCast >= timeBetweenCast && Mana >= manaSpellCost) 
        {
            pState.casting = true;
            timeSinceCast = 0;
            StartCoroutine(CastCoroutine());
        }
        else 
        {
            timeSinceCast += Time.deltaTime;
        }

        if (!Input.GetButton("Cast/Heal"))
        {
            castOrHealTimer = 0;
        }

        if (Grounded())
        { 
            //disables down fireball
            downSpellFireball.SetActive(false);
        }

        if (downSpellFireball.activeInHierarchy) //The ground pound. Slams the player down onto the ground with the attack.
        {
            rb.linearVelocity += downSpellForce * Vector2.down;
        }
    }

    IEnumerator CastCoroutine()
    {
        anim.SetBool("Casting", true);
        yield return new WaitForSeconds(0.15f);

        //side cast
        if (yAxis == 0 || (yAxis < 0 && Grounded()))
        {
            GameObject _fireBall = Instantiate(sideSpellFireball, SideAttackTransform.position, Quaternion.identity);

            //flip fireball
            if (pState.lookingRight)
            {
                _fireBall.transform.eulerAngles = Vector3.zero;
            }
            else
            {
                _fireBall.transform.eulerAngles = new Vector2(_fireBall.transform.eulerAngles.x, 180); //if not facing right, flip fireball
            }
            pState.recoilingX = true;
        }

        else if (yAxis > 0) //if Holding up
        {
            Instantiate(upSpellExplosion, transform);
            rb.linearVelocity = Vector2.zero;
        }

        //down cast
        else if (yAxis < 0 && !Grounded()) //If holding down and not on the ground
        {
            downSpellFireball.SetActive(true);
        }

        Mana -= manaSpellCost;
        yield return new WaitForSeconds(0.35f);
        anim.SetBool("Casting", false);
        pState.casting = false;
    }*/

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

    void OnJump(InputAction.CallbackContext ctx) //Wonder what this does
    {
        jumping = true;

        if (!Grounded() && airJumpCounter < maxAirJumps) //Multijump! Can be set to any number of jumps, but isn't required.
        {
            pState.jumping = true;

            airJumpCounter++;

            rb.linearVelocity = new Vector3(rb.linearVelocity.x, jumpForce);
        }

        
    }

    void OnJumpCanceled(InputAction.CallbackContext ctx) 
    {
        jumping = false;
    }

    void UpdateJumpVariables()
    {
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
}
