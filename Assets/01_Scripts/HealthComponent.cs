using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IOT_Delegates;

public class HealthComponent : MonoBehaviour
{
    [SerializeField, Min(0)] private float maxHP = 100.0f;
    private float _currentHP;
    
    [SerializeField, Min(0)] private float healingTimer = 1.0f;
    private float _currentHealingTimer;

    [SerializeField, Min(0)] private float healingSpeed = 25.0f;
    
    private bool _isDead = false;

    public NoParamsDelegate OnDeath;
    public NoParamsDelegate OnUpdatedHP;

    private void Awake()
    {
        _currentHP = maxHP;
    }

    private void Update()
    {
        HealOverTime();
    }

    public void ChangeHP(float amount)
    {
        if (_isDead)
            return;
        
        _currentHP += amount;
        _currentHP = Mathf.Clamp(_currentHP, 0, maxHP);
                
        if (amount <= 0)
            _currentHealingTimer = healingTimer;
        
        OnUpdatedHP?.Invoke();

        _isDead = _currentHP <= 0.0f;
        if (_isDead)
        {
            OnDeath?.Invoke();
            return;
        }
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public float HealthRatio()
    {
        return _currentHP / maxHP;
    }

    void HealOverTime()
    {
        if (_isDead)
            return;
        
        if (_currentHealingTimer > 0.0f)
        {
            _currentHealingTimer -= Time.deltaTime;
            return;
        }

        if (HealthRatio() >= 1.0f)
            return;
        
        ChangeHP(healingSpeed * Time.deltaTime);
    }
}
