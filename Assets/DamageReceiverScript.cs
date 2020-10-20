using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DamageReceiverScript : MonoBehaviour
{
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

    public bool IsDamageAllowed(DamageReceiverScript inDamager, bool selfDamage = false)
    {
        if ((teamID != inDamager.TeamID || teamID == -1 || (selfDamage && inDamager == this) || receiveFromAll) && damageReceivable)
        {
            return true;
        }
        return false;
    }

    public virtual void ReceiveDamage(float damage, DamageReceiverScript inDamager, bool showCrit = false, bool showDamageNumber = true)
    {
        damage = Mathf.Floor(damage);
        /*
        if (inDamager.GetType() == typeof(Player) && GetComponent<Collider>() && showDamageNumber)
        {
            (inDamager as Player).hudRef.ShowDamageNumber(GetComponent<Collider>().bounds.center, damage, showCrit);
        }
        */
        print(damage + " damage received from " + inDamager);
    }
}
