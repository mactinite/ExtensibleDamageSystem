using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace mactinite.ExtensibleDamageSystem
{
    /// <summary>
    /// Damageable that simply tracks health and fires events.
    /// Forms the base for the ExtensibleDamageSystem system.
    /// Implements the IDamageable interface. 
    /// </summary>
    public class Damageable : MonoBehaviour, IDamageable
    {
        public float health = 100;
        public Action<Vector2, float> OnDamage;
        public Action<Vector2> OnDestroyed;
        public Func<float, float> OnProcessDamage;

        private float iTimer = 0;
        public float iTime = 0.08f;

        Transform IDamageable.transform { get => transform;}

        public virtual void Update()
        {
            if (iTimer > 0)
            {
                iTimer -= Time.deltaTime;
            }
        }


        public void Damage(float damage)
        {
            DamageAt(damage, transform.position);
        }

        public void DamageAt(float damage, Vector2 at)
        {
            if (iTimer > 0) return;
            // let extensions pre-process damage. Iterate through registered processors and execute the method
            float dmg = damage;
            if (OnProcessDamage != null)
            {
                foreach (Func<float, float> subscriber in OnProcessDamage.GetInvocationList())
                {
                    dmg = subscriber(dmg);
                }
            }

            // apply damage and invoke events
            if (health - dmg <= 0)
            {
                health = 0;
                // emit destroyed event and let extensions handle reactions like recycling or destroying.
                OnDamage?.Invoke(at, dmg);
                OnDestroyed?.Invoke(at);
            }
            else
            {
                health -= dmg;
                // same as destroyed event, but for damage. Extensions can handle things like spawning effects.
                OnDamage?.Invoke(at, dmg);
            }
            iTimer = iTime;
        }
    }
}
