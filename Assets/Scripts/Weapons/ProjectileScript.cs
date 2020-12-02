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
    [SerializeField] private bool sticky = false;

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

    private void OnCollisionEnter(Collision collision)
    {
        // Check and deal damage
        if (collision.collider.CompareTag("Player"))
        {
            if (collision.collider.GetComponent<DamageReceiverScript>().IsDamageAllowed(owner) && collision.collider.GetComponentInParent<NetworkPlayer>())
            {
                owner.GetComponentInParent<NetworkPlayer>().ReplicateDamageToPlayer(collision.collider.GetComponentInParent<NetworkPlayer>().netIdentity.netId, damage);
                Destroy(gameObject);
            }
        }
        else if (sticky)
        {
            rb.velocity = Vector3.zero;
            rb.isKinematic = true;
            transform.rotation = Quaternion.LookRotation(collision.contacts[0].normal * -1, Vector3.up);
            if (GetComponent<Animator>())
            {
                GetComponent<Animator>().SetTrigger("Stick");
            }
            Destroy(gameObject, 10f);
            mainCollider.enabled = false;
        }
        else
        {
            Destroy(gameObject);
        }
    }
}
