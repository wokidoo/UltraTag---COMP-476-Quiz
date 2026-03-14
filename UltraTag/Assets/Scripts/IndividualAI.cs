using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

public enum UnitRelationship { friendly, enemy, target }
public enum UnitState { Seeking, Fleeing, Searching }

public class IndividualAI : MonoBehaviour
{
    [Header("Settings")]
    public bool debug = false;
    public float acceleration = 8.0f;

    [Header("State (readonly)")]
    public float speed = 2.0f;
    public bool speedIsBoosted = false;
    public UnitState unitState = UnitState.Searching;

    [Header("Animators")]
    public RuntimeAnimatorController _rockAnimator;
    public RuntimeAnimatorController _paperAnimator;
    public RuntimeAnimatorController _spockAnimator;
    public RuntimeAnimatorController _scissorsAnimator;
    public RuntimeAnimatorController _lizardAnimator;

    [Header("References")]
    public GroupAI _groupAI;
    public UnityEvent<GameObject> died;

    // Private references
    private Animator _animator;
    private Rigidbody _rigidbody;
    private Vector3 _steering = Vector3.zero;
    private GameObject _currentTarget = null;
    private bool _gotInRange = false;
    private GameObject _previousTarget = null;
    void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _animator  = GetComponentInChildren<Animator>();

        if (_groupAI == null) 
            FindGroupAI();
        _UpdateAnimator();
        speed = _groupAI.aggressiveness;
    }

    void Update()
    {
        if (!IsGrounded())
            return;

        UpdateState();
        ApplySteering();
        ResetSteering();
    }
    void UpdateState()
    {
        switch (unitState)
        {
            case UnitState.Searching: 
                UpdateSearching();
                break;
            case UnitState.Seeking:
                UpdateSeeking();
                break;
            case UnitState.Fleeing:
                UpdateFleeing();
                break;
        }
    }

    void UpdateSearching()
    {
        GameObject nearestEnemy = FindNearestInRange(_groupAI.GetEnemyTags(), 5f);
        if (nearestEnemy != null)
        {
            TransitionToFlee(nearestEnemy);
            return;
        }

        GameObject nearestTarget = FindNearestTarget();
        if (nearestTarget != null)
            TransitionToSeek(nearestTarget);
    }

   void UpdateSeeking()
    {
        GameObject nearestEnemy = FindNearestInRange(_groupAI.GetEnemyTags(), 5f);
        if (nearestEnemy != null){
            TransitionToFlee(nearestEnemy);
            return;
        }

        AvoidObstacles();
        
        if (_gotInRange == false && _currentTarget != null && Vector3.Distance(_currentTarget.transform.position,this.transform.position) < 7.5)
                _gotInRange = true;
        // If unit got in range of target but then went out of range, change target
        if (_currentTarget != null && Vector3.Distance(transform.position, _currentTarget.transform.position) > 7.5f && _gotInRange == true){
            _previousTarget = _currentTarget;
            _currentTarget = null;
        }

        // If no current target, find the nearest one (excluding the previous target that went out of range)
        if (_currentTarget == null)
            _currentTarget = FindNearestTarget(new List<GameObject> {_previousTarget});

        if (_currentTarget != null)
            AddSteering(_currentTarget.transform.position - transform.position);
        else
            _previousTarget = null;
            unitState = UnitState.Searching;
    }

    void UpdateFleeing()
    {
        GameObject nearestEnemy = FindNearestInRange(_groupAI.GetEnemyTags(), 7.5f);
        if (nearestEnemy == null)
        {
            unitState = UnitState.Searching;
            return;
        }

        if (Vector3.Distance(transform.position, nearestEnemy.transform.position) < 5f)
            StartCoroutine(ApplySpeedBoost());

        GameObject nearestPad = FindNearestInRange(new string[]{}, 5f, "JumpPad");
        if (nearestPad != null)
        {
            AddSteering((nearestPad.transform.position - transform.position) * 2f);
            return;
        }

        AvoidObstacles();
        AddSteering(transform.position - nearestEnemy.transform.position);
    }
    void TransitionToSeek(GameObject target)
    {
        unitState = UnitState.Seeking;
        _currentTarget = target;
        _previousTarget = null;
        _gotInRange = false;
        AddSteering(target.transform.position - transform.position);
    }

    void TransitionToFlee(GameObject enemy)
    {
        unitState = UnitState.Fleeing;
        _currentTarget = null;
        _previousTarget = null;
        _gotInRange = false;
        AddSteering(transform.position - enemy.transform.position);
    }
    void AddSteering(Vector3 direction)
    {
        _steering += Vector3.ProjectOnPlane(direction, Vector3.up).normalized;
    }

    void ApplySteering()
    {
        if (_steering != Vector3.zero)
        {
            Vector3 desiredVelocity = _steering.normalized * speed;
            desiredVelocity.y = _rigidbody.velocity.y;
            Vector3 smoothedVelocity = Vector3.Lerp(_rigidbody.velocity, desiredVelocity, acceleration * Time.deltaTime);
            smoothedVelocity.y = _rigidbody.velocity.y;
            _rigidbody.velocity = smoothedVelocity;
        }
        else
        {
            Vector3 decelerated = Vector3.Lerp(_rigidbody.velocity, new Vector3(0f, _rigidbody.velocity.y, 0f), acceleration * Time.deltaTime);
            decelerated.y = _rigidbody.velocity.y;
            _rigidbody.velocity = decelerated;
        }

        if (debug)
            Debug.DrawLine(transform.position, transform.position + _steering.normalized * 2f, Color.red, Time.deltaTime);
    }

    void AvoidObstacles()
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, 5f, LayerMask.GetMask("Obstacle"));
        foreach (Collider col in colliders)
        {
            AddSteering(transform.position - col.transform.position);
        }
    }

    void ResetSteering()
    {
        _steering = Vector3.zero;
        if (!speedIsBoosted)
            speed = _groupAI.aggressiveness;
    }

    GameObject FindNearestInRange(string[] tags, float range, string layerName = "Unit")
    {
        Collider[] colliders = Physics.OverlapSphere(transform.position, range, LayerMask.GetMask(layerName));
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (Collider col in colliders)
        {
            if (col.gameObject == this.gameObject) 
                continue;

            if (tags.Length > 0 && !tags.Contains(col.gameObject.tag)) 
                continue;

            float dist = Vector3.Distance(transform.position, col.transform.position);
            if (dist < minDist)
            {
                minDist = dist;
                nearest = col.gameObject;
            }
        }
        return nearest;
    }

    GameObject FindNearestTarget(List<GameObject> exclude = null)
    {
        string[] tags = _groupAI.GetTargetTags();
        GameObject[] targets = tags.SelectMany(t => GameObject.FindGameObjectsWithTag(t)).ToArray();
        GameObject nearest = null;
        float minDist = float.MaxValue;

        foreach (GameObject target in targets)
        {
            if (target.layer != LayerMask.NameToLayer("Unit"))
                continue;
            if (exclude != null && exclude.Contains(target))
                continue;
            float dist = Vector3.Distance(transform.position, target.transform.position);
            if (dist < minDist){
                minDist = dist; nearest = target;
            }
        }
        return nearest;
    }

    IEnumerator ApplySpeedBoost()
    {
        if (speedIsBoosted) 
            yield break;
        speedIsBoosted = true;
        speed = _groupAI.aggressiveness + 3.0f;
        yield return new WaitForSeconds(3f);
        speed = _groupAI.aggressiveness;
        yield return new WaitForSeconds(3f);
        speedIsBoosted = false;
    }
    bool IsGrounded()
    {
        return Physics.Raycast(transform.position, Vector3.down, 1.5f, LayerMask.GetMask("Ground"));
    }

    public void SetUnitType(string type)
    {
        this.gameObject.tag = type;
        _UpdateAnimator();
    }

    public string GetUnitType() => this.gameObject.tag;

    void FindGroupAI()
    {
        foreach (GameObject go in GameObject.FindGameObjectsWithTag(this.gameObject.tag))
        {
            GroupAI g = go.GetComponent<GroupAI>();
            if (g != null){
                _groupAI = g; 
                return; 
            }
        }
    }

    private void _UpdateAnimator()
    {
        if (_animator == null) return;
        switch (this.gameObject.tag)
        {
            case "rock":
                _animator.runtimeAnimatorController = _rockAnimator;
                break;
            case "paper":
                _animator.runtimeAnimatorController = _paperAnimator;
                break;
            case "scissor":
                _animator.runtimeAnimatorController = _scissorsAnimator;
                break;
            case "spock":
                _animator.runtimeAnimatorController = _spockAnimator;
                break;
            case "lizard":
                _animator.runtimeAnimatorController = _lizardAnimator;
                break;
        }
    }

    public void KillUnit()
    {
        died.Invoke(this.gameObject);
        Destroy(this.gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (_groupAI == null) return;
        GameObject other = collision.gameObject;
        if (other.layer == LayerMask.NameToLayer("Unit")){
            if (_groupAI.GetRelationship(other.tag) == UnitRelationship.enemy)
                KillUnit();
        }
    }
}