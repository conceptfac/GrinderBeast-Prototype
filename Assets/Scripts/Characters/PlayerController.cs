using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using System.Reflection;
using Unity.VisualScripting;
using static HUDManager;

public class PlayerController : Humanoid
{

    #region Fields



    public float health
    {
        get => PlayerStats.health;
        set
        {
            PlayerStats.health = value;
            if (value <= 0)
            {
                status = Status.DEAD;
            }
//            HUDManager.instance.healthBar.fillAmount = value / 100;
        }
    }

    public Status status
    {
        
        get => _status;
        set
        {
            if (_status == Status.DEAD)
            {
                Death();
                return;
            }
            _status = value;
        }
    }

    [SerializeField] private float _sprintSpeed = 1f;
    [SerializeField] private float _walkSpeed = 2f;
    [SerializeField] private float _runSpeed = 6f;

    [SerializeField] private float _turnRate = 0.2f;
    private float _turnVal;

    [SerializeField] private float _jumpForce = 3f;

    [SerializeField] private float _gravity = 10f;


    [SerializeField] private GameObject _coinBurst;

    [SerializeField] private BoxCollider _collider;

    [Header("Grab System")]
    private bool _carrying;
    [SerializeField] private float _grabRange = 1.5f;
    [SerializeField] private Transform _targetHotSpot;
    [SerializeField] private float _throwForce = 30f;

    [SerializeField] private Transform _target;
    [SerializeField] private GrabbableObject _grabItem;
    [SerializeField] private List<GameObject> _grabItems = new List<GameObject>();

    private Vector3 _firstGrabPos;
    private Vector3 _currentGrabPos;
    private int _grabItemsIndex = 0;


    private Transform _interact;


    private Vector3 direction = Vector3.zero;


    private bool _canJump = false;
    private bool _jumping = false;

    private bool _canAttack = true;


    private Coroutine _sequenceCoroutine;


    #endregion

    #region CachedComponents

    private CharacterController _characterController;
    private Rigidbody _rb;
    #endregion

    #region MonoBehaviour Methods

    override protected void Awake()
    {
        base.Awake();
        _characterController = GetComponent<CharacterController>();
        _collider = GetComponent<BoxCollider>();

    }

    // Start is called before the first frame update
     void Start()
    {
        //Sets the HUD Buttons actions
        HUDManager.instance.actionClick = ActionClick;
        HUDManager.instance.interactClick= InteractClick;
        HUDManager.instance.jumpClick = JumpClick;
        HUDManager.instance.carryClick = CarryClick;
        HUDManager.instance.dropClick = DropClick;

        if (health <= 0)
            health = 100;
        else
            health = PlayerStats.health;

        _animator.SetFloat("IdleID", _characterManager.Scheme.idleID);
        _animator.SetFloat("WalkID", _characterManager.Scheme.walkID);
        _animator.SetFloat("JogID", _characterManager.Scheme.jogID);
        _animator.SetFloat("RunID", _characterManager.Scheme.runID);
    }

    override protected void Update()
    {
        base.Update();

        if (_status == Status.BUSY || _status == Status.DEAD || _status == Status.FALLEN || _status == Status.RESETINGBONES || _status == Status.STANDINGUP)
        {
            direction = Vector3.zero;
            return;
        }

        if (_transform.position.y < -4)
        {
            status = Status.DEAD;
            return;
        }

        float speed = 1;
        speed = (Input.GetKey(KeyCode.LeftShift) && status != Status.BLOCKING) ? _runSpeed :  (Input.GetKey(KeyCode.LeftAlt) ? _sprintSpeed : _walkSpeed);


        Vector3 move = Vector3.zero;

        Vector3 inputMove = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));
        Vector3 joyMove = new Vector3(HUDManager.instance.joystick.Horizontal, 0, HUDManager.instance.joystick.Vertical);

        move = (inputMove == Vector3.zero) ? joyMove : inputMove;



        direction.x = move.x * speed;
        direction.z = move.z * speed;

        if (Input.GetKey(KeyCode.K))
            Death();

        if (Input.GetButtonDown("Jump") && _characterController.isGrounded)
        {
            _canJump = true;
        }

        if (Input.GetKey(KeyCode.G))
        {
            DropClick();
        }

        _animator.SetBool("Crouch", Input.GetKey(KeyCode.LeftControl));


        if (_characterController.isGrounded && _jumping)
        {
            _jumping = false;
            _characterManager.PlaySFX(1);
        }

        //Grab and Carry System
        Collider[] colliders = Physics.OverlapSphere(_transform.position, _grabRange);
        foreach (var col in colliders)
        {

            if (col.CompareTag("Ragdoll")) {

                if (col.transform.root.TryGetComponent<GrabbableObject>(out _grabItem))
                {
                    if(_grabItem.status == GrabbableObject.Status.FREE)
                    {
                        HUDManager.instance.carryButton.gameObject.SetActive(true);
                        HUDManager.instance.dropButton.gameObject.SetActive(false);
                        break;
                    }
                }
            }else
            if (col.CompareTag("Interactable"))
            {
                MeatGrinder grinder;

                if(col.transform.root.TryGetComponent<MeatGrinder>(out grinder))
                if(grinder.status == MeatGrinder.Status.NORMAL)
                {
                    _interact = col.transform.root;
                    HUDManager.instance.actionButton.gameObject.SetActive(false);
                    HUDManager.instance.interactButton.gameObject.SetActive(true);
                    break;

                }

            }
            _interact = null;
                HUDManager.instance.actionButton.gameObject.SetActive(true);
                HUDManager.instance.interactButton.gameObject.SetActive(false);
                HUDManager.instance.carryButton.gameObject.SetActive(false);

                HUDManager.instance.dropButton.gameObject.SetActive(_grabItems.Count > 0);

        }

    }

    // Update is called once per frame
    protected override void FixedUpdate()
    {
        base.FixedUpdate();

        if (_status == Status.BUSY) return;

        if (!_characterController.isGrounded)
        {
            direction.y -= _gravity * Time.fixedDeltaTime; //Physics.gravity.y * Time.deltaTime;
        }
        else
        {
            if (_canJump)
            {
                direction.y = _jumpForce;
                _canJump = false;
                _jumping = true;
                _characterManager.PlaySFX(0);
            }
        }

        Vector3 directionVerify = new Vector3(direction.x, 0, direction.z);

        if (directionVerify.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.SmoothDampAngle(_transform.eulerAngles.y, targetAngle, ref _turnVal, _turnRate);

            _transform.rotation = Quaternion.Euler(0, angle, 0);
        }

        _characterController.Move(direction * Time.deltaTime);
        
        Vector3 characterVelocity = _characterController.velocity;
        Vector3 forwardVelocity  = new Vector3(characterVelocity.x, 0, characterVelocity.z);


        _animator.SetFloat("Forward", Mathf.Abs(forwardVelocity.magnitude));
        _animator.SetBool("OnGround", _characterController.isGrounded);
        _animator.SetFloat("Jump", _characterController.velocity.y);


        //Verify if is falling from a fatal height so activates the ragdoll
        if (status != Status.DEAD && ragDoll && !_characterController.isGrounded)
        {
            RaycastHit hit;
            Ray downRay = new Ray(transform.position, -Vector3.up);

            // Cast a ray straight downwards.
            if (Physics.Raycast(downRay, out hit))
            {

                if (hit.distance > _characterManager.Scheme.fatalHeight)
                {
                    _animator.enabled = false;  
                    _collider.enabled = true;
                }

            }
        }

    }

    override protected void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
    }

    override protected void OnTriggerExit(Collider other)
    {
        base.OnTriggerExit(other);
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// ActionClick is Called when player clicks on ACTION button HUD
    /// </summary>
    private void ActionClick()
    {
        Attack(AttackType.PUNCH);
    }

    /// <summary>
    /// InteractClick is Called when player clicks on INTERACT button HUD
    /// </summary>
    private void InteractClick()
    {
        //Grinder Machine
        if(_interact != null)
        {
            MeatGrinder grinder;
            if(_interact.TryGetComponent<MeatGrinder>(out grinder)){
                grinder.StartMachine();
            }
        }
    }

    /// <summary>
    /// JumpClick is Called when player clicks on JUMP button HUD
    /// </summary>
    private void JumpClick(bool down)
    {
        _canJump = down;
    }

    /// <summary>
    /// CarryClick is Called when player clicks on GRAB button HUD
    /// </summary>

    private void CarryClick()
    {


        if(_grabItem != null)
        {

            AIController aI = _grabItem.GetComponent<AIController>();
            _target = _grabItem.transform;

            if (aI != null)
            {
                    
                aI.SetRagdoll(true,true);
                Animator AIAnimator = aI.GetComponent<Animator>();
                _grabItem.GetComponent<Collider>().enabled = false;
                _grabItem.GetComponent<Rigidbody>().isKinematic = true;
                _grabItem.GetComponent<Rigidbody>().useGravity = false;
                AIAnimator.enabled = false;
                aI.RagdollReset();
            }
            _grabItems.Add(_grabItem.gameObject);

            HUDManager.instance.carryButton.gameObject.SetActive(false);

            if (_grabItems.Count == 1) {
                status = Status.BUSY;
                _animator.SetTrigger("Pick");
                return;
            }
            _carrying = true;
            _animator.SetBool("Carrying", _carrying);
            _target.gameObject.transform.position = _currentGrabPos;
            _currentGrabPos = new Vector3(_target.transform.position.x, _grabItems[_grabItemsIndex].transform.position.y + .6f, _target.transform.position.z);
            _target.gameObject.GetComponent<GrabbableObject>().UpdatePosition(_grabItems[_grabItemsIndex].transform);
            _grabItemsIndex++;

        }
    }


    /// <summary>
    /// DropClick is Called when player clicks on DROP button HUD
    /// </summary>
    private void DropClick()//Drop a Grabbing Item
    {
        HUDManager.instance.dropButton.gameObject.SetActive(false);
        status = Status.BUSY;
        _carrying = false;
        _animator.SetBool("Carrying", _carrying);
        _animator.SetTrigger("Throw");
    }
    #endregion

    #region Public Methods




    #endregion

    #region Overrided Methods


    protected override void Death()
    {
        base.Death();

//        LevelManager.lives -= 1;
//        LevelManager.RestartLevel(LevelManager.RESTART_DEATH_TIME,"DEATH_MESSAGE");

    }

    public override void Attack(AttackType attackType)
    {
        if (_canAttack) {
            base.Attack(attackType);
            status = Status.ATTACKING;
            switch (attackType)
            {
                case AttackType.PUNCH:
                    _animator.SetBool("Punch", true);
                    if (_sequenceCoroutine != null)
                        StopCoroutine(_sequenceCoroutine);
                    _sequenceCoroutine = StartCoroutine(MeleeCombo("Punch"));
                break;
            }
        }
    }

    public override void StopAttack()
    {
        base.StopAttack();


        if(status == Status.ATTACKING)
           status = Status.NORMAL;

    }
    #endregion

    #region Corountines

    IEnumerator MeleeCombo(string param)
    {
        _canAttack = false;
        yield return new WaitForSeconds(.2f);
        _animator.SetBool(param, false);
        _canAttack = true;
       
    }

    #endregion

    #region Animation Events
    public void CarryOn()
    {
        _firstGrabPos = _targetHotSpot.GetComponent<MeshRenderer>().bounds.max;
        _currentGrabPos = new Vector3(_target.transform.position.x, _firstGrabPos.y, _target.position.z);
        _target.gameObject.transform.position = _currentGrabPos;
        _currentGrabPos = new Vector3(_target.transform.position.x, _firstGrabPos.y + .5f, _target.transform.position.z);
        _target.gameObject.GetComponent<GrabbableObject>().UpdatePosition(_targetHotSpot, 100f);

    }

    public void ResumeAction()
    {
        status = Status.NORMAL;
        _carrying = (_grabItems.Count > 0);
 
        _animator.SetBool("Carrying", _carrying);


    }

    public void Throw()
    {
        foreach (var item in _grabItems)
        {
            item.GetComponent<GrabbableObject>().DropItem();
            AIController aI = item.GetComponent<AIController>();
            if (aI != null)
            {
                //                aI.SetRagdoll(false);
                aI.SetRagdoll(true);
                //                item.GetComponent<Collider>().isTrigger = false;
                item.GetComponent<Collider>().enabled = true;
                Rigidbody aIrb = item.GetComponent<Rigidbody>();
                aIrb.isKinematic = false;
                aIrb.useGravity = true;
                aI.RagdollImpulse((_transform.forward + _transform.up), _throwForce);

            }
        }
        //Reset GrabList
        _target = null;
        _grabItem = null;
        _grabItems.Clear();
        _firstGrabPos = _currentGrabPos = Vector3.zero;
        _grabItemsIndex = 0;
    }
    #endregion
}