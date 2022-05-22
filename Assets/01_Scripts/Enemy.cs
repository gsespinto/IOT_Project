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
    private const int SPACE = 10;
    
    private bool _playerIn = false;
    
    [Header("ATTACK")]
    [SerializeField] private float attackRange = 2.0f;
    [SerializeField] private float attackDamage = 25.0f;
    [SerializeField] private float attackTimer = 2.0f;
    [SerializeField, Range(0, 19)] private int frequency = 20;
    [SerializeField] private string name = "";
    private float _currentAttackTimer;
    private bool _attacking = false;
    [SerializeField] private float idTime = 1.0f;
    private bool isDead = false;
    private Vector3 deathPos;
    
    [Space(SPACE)]
    [Header("COMPONENTS")]
    private Player _player;
    private NavMeshAgent _agentComponent;
    [SerializeField] private GameObject monsterModel;
    [SerializeField] private GameObject spiritModel;
    [SerializeField] private Animator modelAnimator;
    [SerializeField] private Transform idTrans;
    [SerializeField] private AnimatorEvents animatorEvents;
    [SerializeField] private GameObject transformationVFX;
    [SerializeField] private GameObject deathVFX;
    
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
        
        ShowMonster(false, false);
        animatorEvents.OnBuzz += Buzz;
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
        bool canWalk = isDead || (_playerIn && _player.IsLightOn());
        _agentComponent.isStopped = !canWalk;
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

    public float IdTime
    {
        get { return idTime; }
    }

    public int Frequency
    {
        get { return frequency; }
    }
    
    public string Name
    {
        get { return name; }
    }

    public void ShowMonster(bool show, bool showVFX = true)
    {
        if (monsterModel.activeInHierarchy == show)
            return;
        
        monsterModel.SetActive(show);
        spiritModel.SetActive(!show);

        if (showVFX)
        {
            GameObject.Instantiate(transformationVFX, this.transform.position + Vector3.up * _agentComponent.height / 2, this.transform.rotation);
            _player.inputComponent.Buzz(20);
        }
    }

    public void GetAttacked(float attackFrequency)
    {
        if (attackFrequency != frequency)
        {
            Debug.Log("Not using the correct attack frequency!");
            return;
        }

        Vector3 targetPos = deathPos;
        _agentComponent.SetDestination(targetPos);
        
        _playerIn = false;
        _player.SetCanWalk(true);
        _player.SetCurrentEnemy(null);
        isDead = true;
        ShowMonster(false);
        
        StartCoroutine(KillMonster(5.0f));
    }

    IEnumerator KillMonster(float killTime)
    {
        while (true)
        {
            yield return new WaitForSeconds(killTime);
            GameObject.Instantiate(deathVFX, this.transform.position + Vector3.up * _agentComponent.height / 2, this.transform.rotation);
            _player.inputComponent.Buzz(20);
            Destroy(this.gameObject);
            yield return null;
        }
    }

    public void SetDeathPos(Vector3 pos)
    {
        deathPos = pos;
    }

    void Buzz()
    {
        _player.inputComponent.Buzz(frequency);
    }
}
