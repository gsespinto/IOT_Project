using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using IOT_Delegates;

public class HealthComponent : MonoBehaviour
{
    [SerializeField, Min(0.0001f)] private float maxHP = 100.0f;
    private float _currentHP;
    private bool _isDead = false;

    public NoParamsDelegate OnDeath;

    private void Awake()
    {
        _currentHP = maxHP;
    }

    public void ChangeHP(float amount)
    {
        if (_isDead)
            return;
        
        _currentHP += amount;
        _currentHP = Mathf.Clamp(_currentHP, 0, maxHP);

        _isDead = _currentHP <= 0.0f;
        if (_isDead)
            OnDeath?.Invoke();
    }

    public bool IsDead()
    {
        return _isDead;
    }

    public float HealthRatio()
    {
        return _currentHP / maxHP;
    }
}
