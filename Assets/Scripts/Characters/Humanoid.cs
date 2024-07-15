using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Experimental.AI;

/// <summary>
/// A generic hierarchy Humanoid Class 
/// </summary>
public class Humanoid : MonoBehaviour 
{
    #region Cached Components
    protected Animator _animator;
    protected CharacterManager _characterManager;
    #endregion


    #region Fields

    [SerializeField] protected Status _status;

    [SerializeField]
    protected bool ragDoll = false;

    [SerializeField]
    protected bool lootLoss = false;

    protected Transform _transform;

    bool _gettingHit = false;


    private Vector3 _latestPosition;
    private Quaternion _latestRotation;

    private float _currentTime = 0;
    private double _currentPacketTime = 0;
    private double _lastPacketTime = 0;
    private Vector3 _lastPacketPosition = Vector3.zero;
    private Quaternion _lastPacketRotation = Quaternion.identity;

    protected Coroutine _ragdollCor;

    protected Transform _ragdollRoot;
    protected Rigidbody[] _ragdollRigidBodies;
    protected CharacterJoint[] _ragdollJoints;
    protected Collider[] _ragdollColliders;

    protected BoneTransform[] _standUpBones;
    protected BoneTransform[] _ragdollBones;
    protected Transform[] _bones;

    protected float _elapsedResetBonesTime;
    protected float _upDown;

    #endregion


    #region MonoBehaviour Methods

    virtual protected void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == _transform) return;
        if (other.CompareTag("Hit") && _status != Status.DEAD && !_gettingHit && _status != Status.FALLEN)
        {
            _gettingHit = true;
            GetHit(other);
            StartCoroutine(ResetHit());
        }
    }
    virtual protected void OnTriggerExit(Collider other) { 
    }

    virtual protected void Awake() {
        
        //Sets chached components variables
        _transform = GetComponent<Transform>();
        _animator = GetComponent<Animator>();
        _characterManager = GetComponent<CharacterManager>();


        //Configs RagDoll
        _ragdollRoot = _animator.GetBoneTransform(HumanBodyBones.Hips);
        _ragdollRigidBodies = _ragdollRoot.GetComponentsInChildren<Rigidbody>();
        _ragdollJoints = _ragdollRoot.GetComponentsInChildren<CharacterJoint>();
        _ragdollColliders = _ragdollRoot.GetComponentsInChildren<Collider>();

        _bones = _ragdollRoot.GetComponentsInChildren<Transform>();
        _standUpBones = new BoneTransform[_bones.Length];
        _ragdollBones = new BoneTransform[_bones.Length];

        for (int i = 0; i < _bones.Length; i++)
        {
            _standUpBones[i] = new BoneTransform();
            _ragdollBones[i] = new BoneTransform();
        }

        StartBoneTransforms();

       // SetRagdoll(false);
    }

    virtual protected void Update()
    {
    }

    virtual protected void FixedUpdate()
    {
    }

    #endregion

    #region Coroutines
    private IEnumerator ResetHit()
    {
        yield return new WaitForEndOfFrame();
        _gettingHit = false;
    }

    #endregion

    #region Methods
    virtual public void StopAttack(int id) { }

    virtual protected void GetHit(Collider aggressor)
    {
        Debug.Log(aggressor.transform.name);
         Instantiate(_characterManager.Scheme._bloodSplat, aggressor.bounds.center, Quaternion.identity);
        _characterManager.PlaySFX(5);
    }
    virtual protected void Death() {
        if (ragDoll)
            SetRagdoll(true);
        else
            _animator.SetBool("Dead", true);
    }

    virtual public void RagdollImpulse(Vector3 direction, float force) {
        Rigidbody[] bones = GetComponentsInChildren<Rigidbody>();

        foreach (Rigidbody bone in bones)
        {

            bone.AddForce(direction * force, ForceMode.Impulse);
        }
    }

    public void SetRagdoll(bool status, bool knematic = false)
    {

        foreach (CharacterJoint joint in _ragdollJoints) { joint.enableCollision = status; }
        foreach(Collider col in _ragdollColliders) { col.enabled = status; }
        foreach(Rigidbody rb in _ragdollRigidBodies)
        {
            rb.velocity = Vector3.zero;
            rb.detectCollisions = status;
            rb.useGravity = status;
            rb.isKinematic = knematic;  
        }
        _animator.enabled = !status;
    }

    /// <summary>
    /// Fix when ragdoll leaves the parent rigidbody position
    /// </summary>
    protected void AlignRagdollPosition()
    {
        Vector3 originalPosition = _ragdollRoot.position;
        _transform.position = _ragdollRoot.position;

        Quaternion originalRotation = _ragdollRoot.rotation;
        _transform.rotation = _ragdollRoot.rotation;

        if (Physics.Raycast(_transform.position, Vector3.down, out RaycastHit hit)) {
            _transform.position = new Vector3(_transform.position.x, hit.point.y, _transform.position.z);
        }

        _ragdollRoot.position = originalPosition;
        _ragdollRoot.rotation = originalRotation;
    }
    
    /// <summary>
    /// Fix when ragdoll leaves the parent rigidbody rotation (DEPRECATED)
    /// </summary>
    protected void AlignRagDollRotation()
    {
        Vector3 originalPosition = _ragdollRoot.position;
        Quaternion originalRotation = _ragdollRoot.rotation;

        Vector3 desiredDirection = _ragdollRoot.up * -1;

        desiredDirection.y = 0;
        desiredDirection.Normalize();

        Quaternion fromToRotation = Quaternion.FromToRotation(_transform.forward, desiredDirection);
        _transform.rotation *= fromToRotation;

        _ragdollRoot.position = originalPosition;
        _ragdollRoot.rotation = originalRotation;
    }
    #endregion

    #region Animation Events

    virtual public void Attack(AttackType attackType)
    {

    }

    virtual public void StopAttack() {
               
        _animator.SetBool("Attack", false);
    }

    virtual protected void StandUp()
    {
        SetRagdoll(false);
        _status = Status.STANDINGUP;

        _animator.SetInteger("StandId", _upDown >= 0 ? 1 : 2);
        _animator.SetTrigger("Stand");
        
        //_animator.Play(_upDown >= 0 ? "StandUp" : "StandUpCrawl");

    }


    #endregion

    protected class BoneTransform
    {
        public Vector3 position {  get; set; }
        public Quaternion rotation { get; set; }
    }

    /// <summary>
    /// Fill the paramter array with ragDoll bones
    /// </summary>
    /// <param name="bones"></param>
    protected void PopulateBoneTransforms(BoneTransform[] bones)
    {
        for (int i = 0; i < _bones.Length; i++)
        {
            bones[i].position = _bones[i].localPosition;
            bones[i].rotation = _bones[i].localRotation;
        }
    }



    /// <summary>
    /// Starts a PopulateBoneTransforms 
    /// </summary>
    private void StartBoneTransforms()
    {


        Vector3 positionBefore = _transform.position;
        Quaternion rotationBefore = _transform.rotation;

        foreach (var clip in _animator.runtimeAnimatorController.animationClips)
        {

            if(clip.name == "HumanoidStandUp")
            {
                clip.SampleAnimation(gameObject, 0);
                PopulateBoneTransforms(_standUpBones);
                break;
            }
        }

        _transform.position = positionBefore;
        _transform.rotation = rotationBefore;
    }

    public void RagdollReset()
    {
        _transform.position = _ragdollRoot.position;
        _ragdollRoot.position = _transform.position;
    }

    /// <summary>
    /// Creates a smooth transition betwen ragdol and animation  
    /// </summary>
    private void ResetBones(){
        _elapsedResetBonesTime += Time.deltaTime;
        float percent = _elapsedResetBonesTime / .5f;

        for (int i = 0; i < _bones.Length; i++)
        {
            _bones[i].localPosition = Vector3.Lerp(_ragdollBones[i].position, _standUpBones[i].position, percent);
            _bones[i].localRotation = Quaternion.Lerp(_ragdollBones[i].rotation, _standUpBones[i].rotation, percent);
        }
        if (percent >= 1f)
        {
            PopulateBoneTransforms(_ragdollBones);
            StandUp();
        }
    }


}
