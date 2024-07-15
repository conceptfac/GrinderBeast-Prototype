using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls the AI FOV to search the player
/// </summary>
public class AI_FieldOfView : MonoBehaviour
{

    #region Fields

    public float radius = 2f;
    
    [Range(0, 360)]
    public float angle = 80f;

    public LayerMask targetMask;
    public LayerMask obstacleMask;

    public Transform target;

    private Transform _transform;
    private AIController _aiController;

    #endregion

    #region MonoBehaviour Methods

    private void Awake()
    {
        _transform = GetComponent<Transform>();
        _aiController = GetComponent<AIController>();
    }

    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(FOVCoroutine());
    }

    #endregion

    #region Coroutines

    private IEnumerator FOVCoroutine()
    {
        WaitForSeconds wait = new WaitForSeconds(0.2f);

        while (true)
        {
            yield return wait;

            Collider[] objectsInRange = Physics.OverlapSphere(_transform.position, radius, targetMask);

            if (objectsInRange.Length > 0)
            {
                Transform tempTarget = objectsInRange[0].transform;
                Vector3 directionToTarget = (tempTarget.position - _transform.position).normalized;

                if (Vector3.Angle(_transform.forward, directionToTarget) < angle / 2)
                {
                    float distanceToTarget = Vector3.Distance(_transform.position, tempTarget.position);

                    if (Physics.Raycast(_transform.position, directionToTarget, distanceToTarget, obstacleMask))
                    {
                        target = null;
                    }
                    else 
                    {
                        PlayerController player = tempTarget.GetComponent<PlayerController>();

                        if (player)
                        {
                            if(player.status == Status.NORMAL || player.status == Status.ATTACKING || player.status == Status.BLOCKING)
                            {
                                target = tempTarget;
                                if (_aiController.status == Status.NORMAL || _aiController.status == Status.PATROL)
                                {
                                    _aiController.StartCatch();
                                }

                            }
                        }
                    }
                }
                else
                {
                    target = null;
                }
            }
            else
            {
                target = null;
            }
        }
    }

    #endregion
}
