using UnityEngine;

[Header("Movement Settings")]
[Tooltip("Enemy movement speed")]
[SerializeField] public float moveSpeed = 2.5f;
    
[Tooltip("Radius of wandering circle")]
[SerializeField] private float wanderRadius = 3f;

[Header("Attack Settings")]
[Tooltip("Damage dealt by attack")]
[SerializeField] public int attackDamage = 1;
    
[Tooltip("Cooldown between attacks")]
[SerializeField] private float attackCooldown = 1f; 