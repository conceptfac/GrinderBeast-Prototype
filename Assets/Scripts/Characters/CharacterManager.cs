using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;


/// <summary>
/// This class controls the character features
/// </summary>
[RequireComponent(typeof(AudioSource))]
public class CharacterManager : MonoBehaviour
{
    #region CachedObjects
    private Transform _transform;
    #endregion

    #region Fields
    [SerializeField] private CharacterConfig _scheme;
    [Header("Player Hit Colliders")]
    public CapsuleCollider leftHandHit;
    public CapsuleCollider rightHandHit;


    private AudioSource _audio;
    public CharacterConfig Scheme { get { return _scheme; } }

    #endregion

    #region MonoBehaviour Methods

    void Awake()
    {
        _transform = GetComponent<Transform>();
        _audio = GetComponent<AudioSource>();
    }

    private void Start()
    {
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Play SFX by "id"
    /// </summary>
    /// <param name="id">0: JumpStart, 1: JumpEnd</param>
    public void PlaySFX(int id)
    {
        AudioClip clip = null;

        switch (id)
        {
            case 0:
                clip = _scheme.jumpStart;
                break;
            case 1:
                clip = _scheme.jumpEnd;
                break;
            case 2:
                clip = _scheme.punchSounds[0];
                break;
            case 3:
                clip = _scheme.punchSounds[1];
                break;
            case 4:
                clip = _scheme.punchSounds[2];
                break;
            case 5:
                clip = _scheme.punchSounds[3];
                break;
            default:
                break;
        }

        _audio.PlayOneShot(clip);
    }


    /// <summary>
    /// Cast out an VFX by the player
    /// </summary>
    /// <param name="burstObj"></param>
    /// <param name="position"></param>
    private void Burst(GameObject burstObj, Vector3 position)
    {
            GameObject burst = Instantiate(burstObj, position, Quaternion.identity);
    }

    #endregion

    #region Animation Events
    /// <summary>
    /// Play a step SFX by id - Called by Locomotion animations
    /// </summary>
    /// <param name="id">0:Right Foot; 1:Left Foot</param>
    public void Step(int id)
    {
        _audio.PlayOneShot(_scheme.footSteps[Random.Range(0, _scheme.footSteps.Length)]);
    }


    /// <summary>
    /// Called by punches animations
    /// </summary>
    /// <param name="status">Hit collider active 0:false; 1:true.</param>
    virtual public void LeftHit(int status)
    {
                leftHandHit.enabled = status == 1;
                if (status == 1)
                    PlaySFX(2);
    }

    /// <summary>
    /// Called by punches animations
    /// </summary>
    /// <param name="status">Hit collider active 0:false; 1:true.</param>
    virtual public void RightHit(int status)
    {
        rightHandHit.enabled = status == 1;
        if (status == 1)
            PlaySFX(3);
    }


    /// <summary>
    /// Called by Death Animation - Player lost coins after dead
    /// </summary>
    public void CoinBurst(GameObject burstObj)
    {
        //       if (status == Status.DEAD) LevelManager.coins = 0;
        GameObject burst = Instantiate(burstObj, _transform.position, Quaternion.identity);
    }

    #endregion
}

