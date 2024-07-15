using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// This class control the grabbable and objects and bodies 
/// </summary>
public class GrabbableObject : MonoBehaviour
{


    #region Cached Objects
    private Transform _transform;
    #endregion

    private Coroutine _updateCor;

    public enum Status
    {
        FREE,
        GRABBED
    }

    [SerializeField]
    private Status _status;

    public Status status { get => _status; set { _status = value; } }

    #region MonoBehaviout Methods
    private void Awake()
    {
        _transform = GetComponent<Transform>();
    }
    #endregion

    #region Public Methods
    public void UpdatePosition(Transform hotSpot, float followSpeed = 15f)
    {
        status = Status.GRABBED;
        if (_updateCor != null) StopCoroutine(_updateCor);
        _updateCor = StartCoroutine(FollowWithDelay(hotSpot, followSpeed));
    }

    public void DropItem()
    {
        _status = Status.FREE;
    }
    #endregion


    #region Coroutines

    /// <summary>
    /// Makes the object follow another with a delay
    /// </summary>
    /// <param name="hotSpot">Point to fit</param>
    /// <param name="followSpeed"></param>
    /// <returns></returns>
    IEnumerator FollowWithDelay(Transform hotSpot, float followSpeed)
    {



        while (_status == Status.GRABBED)
        {
            if(hotSpot != null)
            {

               _transform.position = new Vector3(
                    Mathf.Lerp(_transform.position.x, hotSpot.position.x, followSpeed * Time.deltaTime),
                    _transform.position.y,
                    Mathf.Lerp(_transform.position.z, hotSpot.position.z, followSpeed * Time.deltaTime)
                );
            }
            yield return new WaitForEndOfFrame();
        }
        _updateCor = null;

        yield return 0;
    }
    #endregion
}
