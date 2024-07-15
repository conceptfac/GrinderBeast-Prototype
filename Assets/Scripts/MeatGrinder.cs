using System.Collections;
using System.Collections.Generic;
using UnityEngine;


/// <summary>
/// This class controls the MeatGrinder
/// </summary>
public class MeatGrinder : MonoBehaviour
{

    #region Cached Objects
    [SerializeField] private AudioSource _machineAudio;
    [SerializeField] private AudioSource _grinderAudio;
    #endregion

    #region Fields
    public enum Status
    {
        NORMAL,
        BUSY
    }
    [SerializeField] private Status _status;
    public Status status { get => _status; }

    [SerializeField] private GameObject _item;
    [SerializeField] private Transform _burstHotspot;
    
    private Coroutine _grinderCor;
    
    #endregion


    #region MonoBehaviour Methods

    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {

    }


    private void OnTriggerStay(Collider other)
    {
        if (_item == null)
            _item = other.transform.root.gameObject;
    }

    private void OnTriggerExit(Collider other)
    {
        if (_item != null)
            if (_item == other.transform.root.gameObject)
                _item = null;
    }
    #endregion

    #region Public Methods
    public void StartMachine()
    {
        if (_status == Status.BUSY)
            return;

        
        if(_grinderCor != null)
            StopCoroutine( _grinderCor );
        _grinderCor = StartCoroutine(SwitchMachineOn());

    }
    #endregion

    #region Coroutines
    private IEnumerator SwitchMachineOn()
    {
        GameObject burstFX = null;
        _status = Status.BUSY;

        if (!_machineAudio.isPlaying)
            _machineAudio.Play();

        yield return new WaitForSeconds(3f);
        
        while(_item != null)
        {
            GrabbableObject item;
            if (_item.TryGetComponent<GrabbableObject>(out item))
            {
                //Prevent to destroy player grabbing items in an eventual triggering  
                if (item.status == GrabbableObject.Status.FREE)
                {
                    AIController aI;
                    if (_item.TryGetComponent<AIController>(out aI))
                    {
                        aI.SetRagdoll(false);
                    }

                    if (!_grinderAudio.isPlaying)
                        _grinderAudio.Play();

                    yield return new WaitForSeconds(1f);

                    CharacterManager manager;
                    if (_item.TryGetComponent<CharacterManager>(out manager))
                    {
                        burstFX = Instantiate(manager.Scheme.coinBurst, _burstHotspot.position, Quaternion.identity);
                    }
                    LevelManager.coins += 100;
                    DestroyImmediate(_item.gameObject);
                }
                else
                    _item = null;
            }



            yield return new WaitForSeconds(2f);
        }


        while (_item==null && _machineAudio.isPlaying) yield return new WaitForSeconds(.1f);
        _status = Status.BUSY;

        _grinderCor = null;
        if (burstFX != null)
            Destroy(burstFX);
        _status =Status.NORMAL;

    }
    #endregion
}
