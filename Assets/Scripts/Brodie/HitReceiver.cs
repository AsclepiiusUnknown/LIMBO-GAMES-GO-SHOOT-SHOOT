using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace LIMBO.Weaponary
{
    //Parent script for all damagable object/beings allowing for taking damage
    public class HitReceiver : MonoBehaviour
    {
        [System.Serializable]
        public class DeathEvent : UnityEvent<TargetJoint2D> { }


    }
}