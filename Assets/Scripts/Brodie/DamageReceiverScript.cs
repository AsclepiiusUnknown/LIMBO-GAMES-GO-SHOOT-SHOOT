using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class DamageReceiverScript : MonoBehaviour
{
    [SerializeField] NetworkPlayer netPlayerRef;
    public NetworkPlayer NetPlayerRef { get { return netPlayerRef; } }
    [SerializeField] float maxHealth = 100f;
    private float currentHealth;
    public static List<NetworkStartPosition> spawns;
    public static int spawnIndex = 0;
    [SerializeField] int teamID = 0;
    public int TeamID
    {
        get
        {
            return teamID;
        }
    }
    [SerializeField] private bool receiveFromAll;
    bool damageReceivable = true;
    public bool DamageReceivable
    {
        get
        {
            return damageReceivable;
        }
        set
        {
            damageReceivable = value;
        }
    }
    [SerializeField] private Collider criticalCollider;
    public Collider CriticalCollider
    {
        get
        {
            return criticalCollider;
        }
    }

    private void Start()
    {
        currentHealth = maxHealth;

        if (netPlayerRef.isLocalPlayer)
            GatherSpawnPoints();
    }

    void GatherSpawnPoints()
    {
        spawns = new List<NetworkStartPosition>(FindObjectsOfType<NetworkStartPosition>());
        print(spawns.Count);
    }

    public bool IsDamageAllowed(DamageReceiverScript inDamager, bool selfDamage = false)
    {
        return true;
        // Removed for simplicity, opposite of jank
        /* 
        if ((teamID != inDamager.TeamID || teamID == -1 || (selfDamage && inDamager == this) || receiveFromAll) && damageReceivable)
        {
            return true;
        }
        return false; 
        */
    }

    public virtual void ReceiveDamage(float damage)
    {
        damage = Mathf.Floor(damage);
        currentHealth -= damage;
        if (currentHealth <= 0)
        {
            print("Player is now dead.");
            Death();
        }
        else
        {
            print("Player took " + damage + " damage.");
        }
    }

    private void Death()
    {
        if (netPlayerRef.isLocalPlayer)
        {
            Respawn(GetComponent<CharacterController>());
        }
    }

    public void Respawn(CharacterController _controller)
    {
        _controller.enabled = false;
        _controller.transform.position = spawns[spawnIndex].transform.position;
        spawnIndex++;
        if (spawnIndex >= spawns.Count)
            spawnIndex = 0;
        _controller.enabled = true;
        netPlayerRef.MaxHealthPlayer();
    }

    public void HealDamage(float damage)
    {
        currentHealth = Mathf.Clamp(currentHealth + damage, 0, maxHealth);
    }

    public void SetHealthToMax()
    {
        HealDamage(maxHealth);
    }
}
