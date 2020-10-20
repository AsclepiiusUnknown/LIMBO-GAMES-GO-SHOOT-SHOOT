using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileScript : MonoBehaviour
{
    private DamageReceiverScript owner;
    public DamageReceiverScript Owner
    {
        get
        {
            return owner;
        }
    }
    float damage;
    float criticalDamage;
    [SerializeField] private float speed = 10f;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private Collider mainCollider;

    public void SetUpProjectile(float inDmg, float inCrit, DamageReceiverScript inOwner)
    {
        damage = inDmg;
        criticalDamage = inCrit;
        owner = inOwner;
    }

    public void BeginMovement()
    {
        rb.velocity = transform.forward * speed;
        mainCollider.enabled = true;
    }
}
