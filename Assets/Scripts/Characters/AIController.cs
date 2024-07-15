using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


/// <summary>
/// Controls the AI Character using Navigation System
/// </summary>
[RequireComponent(typeof (NavMeshAgent))]
public class AIController : Humanoid
{

    #region Cached Objects
    private Rigidbody _rb;
    #endregion

    #region Fields

    [SerializeField] protected float _health = 100;


    public float health
    {
        get => _health;
        set
        {
            _health = value;
            if (value <= 0)
            {
                status = Status.DEAD;
            }
        }
    }

    public Status status
    {
        get => _status;
        set
        {
            if (_status == Status.DEAD)
            {
                return;
            }

            _status = value;

            switch (value)
            {
                case Status.DEAD:
                    Death();
                    break;
                case Status.NORMAL:
                case Status.PATROL:
                    _agent.speed = 1f;
                break;
                
                case Status.CHASING:
                    _agent.speed = 2f;
                break;
                case Status.PERSISTING:
                    _agent.speed = 2f;
                    _catchCor = StartCoroutine(CatchPersist());
                break;
                case Status.FALLEN:
                    StartCoroutine(Knocked(_characterManager.Scheme.knockedTime));
                break;
                case Status.RESETINGBONES:
                    break;
                case Status.STANDINGUP:
                    StandUp();                    
                break;
                    
            }
        }
    }

    private enum PatrolType
    {
        NONE,
        CHASE,
        PATH
    }

    [SerializeField] private PatrolType _patrolType;

    [SerializeField] private float _patrolRadius = 10f;
    [SerializeField] private Vector2 _minMaxIdleTime = new Vector2(1f, 5f);

    [SerializeField] private List<Transform> _wayPoints = new List<Transform>();
    [SerializeField] private int _pathStep = 0;

    [SerializeField] private float _persistentTime = 5f;


    private NavMeshAgent _agent;
    private AI_FieldOfView _fov;

    private Coroutine _destinationCor;
    private Coroutine _catchCor;

    private Transform _target;
    public Transform target
    {
        get => _target;
        set
        {
            _target = value;
        }
    }
    
    #endregion

    #region MonoBehaviour Methods

    override protected void Awake()
    {
        base.Awake();

        _rb = GetComponent<Rigidbody>();
        _agent = GetComponent<NavMeshAgent>();
        _fov = GetComponent<AI_FieldOfView>();
    }
    // Start is called before the first frame update
    void Start()
    {
        if (_status == Status.DEAD)
        {
            Death();
            return;
        }
        switch (_patrolType)
        {
            case PatrolType.NONE:
                break;
            case PatrolType.CHASE:
                InitializeChasing();
                break;
            case PatrolType.PATH:
                InitializePath();
                break;
        }

        _agent.isStopped = false;
    }

    // Update is called once per frame
    override protected void Update()
    {

        base.Update();



        if (_status == Status.BUSY || _status == Status.DEAD)
        {
            return;
        }

        _animator.SetFloat("Forward", _agent.velocity.sqrMagnitude / 2);
    }


    #endregion

    #region Public Methods

    public void InitializeChasing()
    {
        _destinationCor = StartCoroutine(SetDestination());
    }

    public void InitializePath()
    {
        if (_wayPoints.Count > 0)
        {
            _destinationCor = StartCoroutine(SetPath());
        }
        else
        {
            Debug.LogWarningFormat("The waypoints list on {0} gameObject is empty!", name);
        }
    }

    public void StartCatch()
    {
         if (_destinationCor != null)
         {    
             StopCoroutine(_destinationCor);
         }

        if (_catchCor == null || _status == Status.PERSISTING)
        {
            status = Status.CHASING;
            _catchCor = StartCoroutine(SetTarget());
        }
    }

    /// <summary>
    /// Choose a random location inside the NavMesh
    /// </summary>
    /// <param name="origin">Initial Position</param>
    /// <param name="radius">Radius Size</param>
    /// <returns></returns>
    public static Vector3 RandomNavMeshLocation(Vector3 origin, float radius)
    {
        Vector3 randomLocation = Random.insideUnitSphere * radius;
        randomLocation += origin;

        NavMeshHit hit;

        Vector3 finalPosition = Vector3.zero;

        if (NavMesh.SamplePosition(randomLocation, out hit, radius, 1))
        {
            finalPosition = hit.position;
        }

        return finalPosition;
    }

    public void Forget()
    {
        _animator.SetBool("Searching", false);
        Start();
    }
    public void SetAgentDestination(Vector3 destination)
    {
        _agent.destination = destination;
    }

    #endregion

    #region Private Methods


    override protected void Death()
    {
        base.Death();


        if (_catchCor != null)
            StopCoroutine(_catchCor);
        if (_destinationCor != null)
            StopCoroutine(_destinationCor);
        _agent.destination = _transform.position;
        _agent.isStopped = true;
        _agent.enabled = false;


        GetComponent<Rigidbody>().useGravity = false; ;
       // GetComponent<CapsuleCollider>().isTrigger = true;

        gameObject.AddComponent(typeof(GrabbableObject));

        //LevelManager.CleanBuffer(gameObject, 10);
    }

    private void Attack(Transform target)
    {
        status = Status.ATTACKING;
        _animator.SetBool("Attacking", true);
    }

    #endregion

    #region Overriden Methods

    public override void StopAttack(int id)
    {
        base.StopAttack(id);

        if (_target)
        {
            PlayerController player = _target.GetComponent<PlayerController>();

            if (player)
            {
                if (player.status == Status.NORMAL || player.status == Status.ATTACKING || player.status == Status.BLOCKING)
                {
                   status = Status.PERSISTING;
                    return;
                }
            }


        }

        Start();
    }


    protected override void GetHit(Collider aggressor)
    {

        base.GetHit(aggressor);

        RagdollImpulse(aggressor.transform.forward, 15f);
        
            status = Status.FALLEN;
        Vector2 punchStrength = aggressor.transform.root.GetComponent<CharacterManager>().Scheme.punchStrength;
        health -= Random.Range(punchStrength.x,punchStrength.y);

        Transform newTarget = aggressor.transform.root;
        
        
        if (_target != newTarget)
            _target = newTarget;
    }

    #endregion

    #region Coroutines

    private IEnumerator SetDestination()
    {
        if (_status == Status.DEAD)
        {
            yield break;
        }

        status = (Random.Range (0, 2) == 0) ? Status.NORMAL : Status.PATROL;

        if (_status == Status.PATROL)
        {
            _agent.destination =   RandomNavMeshLocation(_transform.position, _patrolRadius);

            //_photonView.RPC("SetAgentDestination", RpcTarget.AllBuffered, RandomNavMeshLocation(_transform.position, _patrolRadius));


            yield return new WaitForSeconds(0.2f);

            while (_agent.remainingDistance > 0.1f)
            {
                yield return new WaitForSeconds(0.2f);
            }

            status = Status.NORMAL;
        } 
        else 
            if (_status == Status.NORMAL)
        {
            yield return new WaitForSeconds(Random.Range(_minMaxIdleTime.x, _minMaxIdleTime.y));   
        }
        _destinationCor =  StartCoroutine(SetDestination());
    }

    private IEnumerator SetPath()
    {
        _agent.destination = _wayPoints[_pathStep].position;
        yield return new WaitForSeconds(0.2f);

        while (_agent.remainingDistance > 0.1f)
        {
            yield return new WaitForSeconds(0.2f);
        }

        yield return new WaitForSeconds(Random.Range(_minMaxIdleTime.x, _minMaxIdleTime.y));

        _pathStep++;

        if (_pathStep  >= _wayPoints.Count)
        {
            _pathStep = 0;
        }

        _destinationCor = StartCoroutine(SetPath());
    }

    private IEnumerator SetTarget()
    {
        if (_status != Status.CHASING)
            yield break;

        WaitForSeconds wait = new WaitForSeconds(.2f);

        _animator.SetBool("Searching", false);
        _agent.isStopped = false;

        target = _fov.target;

        while (_fov.target)
        {
            _agent.destination = target.position;

            if (Vector3.Distance(_transform.position, target.position) < .5f)
            {
                Attack(target);
        
                _catchCor = null;
                yield break;
            }


            yield return wait;
        }

        status = Status.PERSISTING;

    }

    private IEnumerator CatchPersist()
    {

        WaitForSeconds wait = new WaitForSeconds(.2f);

        float timer = 0;

        while (timer < (_persistentTime))
        {
            _agent.destination = target.position;
            timer += 0.2f;

            if (_fov.target)
            {
                status = Status.PATROL;
                _catchCor = null;
                yield break;
            }

            yield return wait;
        }

        status = Status.NORMAL;
        _agent.isStopped = true;

       //Give Up
        _catchCor = null;

        _animator.SetBool("Searching", true);

    }



    private IEnumerator Knocked(float time)
    {
        if(_catchCor != null)
        StopCoroutine(_catchCor);
        if(_destinationCor != null)
        StopCoroutine(_destinationCor);

        _agent.isStopped = true;
        SetRagdoll(true);
  
        yield return new WaitForSeconds(time);
        if(_status == Status.DEAD)
            yield break;

        _upDown = Vector3.Dot(_ragdollRoot.forward, Vector3.up);

        AlignRagdollPosition();
        //AlignRagDollRotation(); (DEPRECATED)
        PopulateBoneTransforms(_ragdollBones);
        StandUp();
        _elapsedResetBonesTime = 0;
    }

    #endregion



    #region Animation Methods
    public void ResumeAction()
    {
        _animator.SetInteger("StandId", 0);
        Start();
    }
    #endregion
}
