using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HitDetection : MonoBehaviour
{
    public ParticleSystem collisionParticleSystem;
    public bool once = true;

    private void OnCollisionEnter(Collision collision)
    {
        if(collision.gameObject.TryGetComponent<EnemyAI>(out EnemyAI enemyComponent) && once)
        {
            
            enemyComponent.TakeDamage(1);
            collisionParticleSystem.Play();

            once = false;
        }
    }

}
