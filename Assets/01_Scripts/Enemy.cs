using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(NavMeshAgent))]
public class Enemy : MonoBehaviour
{
    private bool _playerIn = false;
    
    // ATTACK
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackDamage = 25.0f;
    [SerializeField] private float attackTimer = 2.0f;
    private float _currentAttackTimer;
    private bool _attacking = false;

    // COMPONENTS
    private Player _player;
    private NavMeshAgent _agentComponent;
    [SerializeField] private Animator modelAnimator;
    [SerializeField] private Transform idTrans;
    [SerializeField] private AnimatorEvents animatorEvents;
    
    private void Start()
    {
        _player = GameObject.FindObjectOfType<Player>();
        _agentComponent = this.GetComponent<NavMeshAgent>();

        _agentComponent.stoppingDistance = attackRange;
        _currentAttackTimer = attackTimer;

        if (!animatorEvents)
        {
            Debug.LogWarning("Missing reference to AnimatorEvents script.", this);
        }
        else
        {
            animatorEvents.OnAttack += AttackPlayer;
        }
    }

    private void Update()
    {
        Move();
        TickAttackTimer();
        AnimatorHandler();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!_player)
        {
            Debug.Log("Missing valid player reference.", this);
            return;
        }
        
        if (other.gameObject != _player.gameObject)
            return;

        _playerIn = true;
        _agentComponent.SetDestination(_player.transform.position);
        _player.SetCanWalk(false);
        _player.SetCurrentEnemy(this);
    }

    private void OnTriggerExit(Collider other)
    {
        if (!_player)
        {
            Debug.LogWarning("Missing valid player reference.", this);
            return;
        }
        
        if (other.gameObject != _player.gameObject)
            return;

        _playerIn = false;
        _player.SetCanWalk(true);
        _player.SetCurrentEnemy(null);
    }

    void Move()
    {
        _agentComponent.isStopped = !_playerIn || !_player.IsLightOn();
    }

    void TickAttackTimer()
    {
        if (!_playerIn || _attacking)
            return;

        if (!_player.IsLightOn())
            return;
        
        if (!_player)
        {
            Debug.LogWarning("Missing valid player reference.", this);
            return;
        }

        if (Vector3.Distance(this.transform.position, _player.transform.position) > attackRange)
            return;

        if (_currentAttackTimer > 0)
        {
            _currentAttackTimer -= Time.deltaTime;
            return;
        }

        _attacking = true;
        
        if (!modelAnimator)
        {
            Debug.LogWarning("Missing model animator reference!", this);
        }
        else
        {
            modelAnimator.SetTrigger("Attack");
        }
    }

    void AnimatorHandler()
    {
        if (!modelAnimator)
        {
            Debug.LogWarning("Missing model animator reference!", this);
            return;
        }
        
        modelAnimator.SetFloat("Speed", _agentComponent.velocity.magnitude / _agentComponent.speed);
    }

    void AttackPlayer()
    {
        _player.ChangeHP(-Mathf.Abs(attackDamage));
        _currentAttackTimer = attackTimer;
        _attacking = false;
    }

    public Vector3 GetIdPosition()
    {
        if (!idTrans)
        {
            return this.transform.position;
        }
        
        return idTrans.position;
    }
}