using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAI : MonoBehaviour
{
    public NavMeshAgent agent;
    // Add particle, use when robot is defeated -------
    public ParticleSystem collisionParticleSystem;

    public Transform player;
    public Transform head;
    public Transform lookObject;
    public GameObject gun;

    //Stats
    public float health;
    [SerializeField]
    public float maxHealth;

    [SerializeField]
    public GameObject healthBarUI;
    public Slider slider;

    //Check for Ground/Obstacles
    public LayerMask whatIsGround, whatIsPlayer;

    //Patroling
    public Vector3 walkPoint;
    public bool walkPointSet;
    public float walkPointRange;

    //Attack Player
    public float timeBetweenAttacks;
    bool alreadyAttacked;

    //States
    public bool isDead;
    public float sightRange, attackRange;
    public bool playerInSightRange, playerInAttackRange;

    private void Awake() {
        player = GameObject.Find("Slime").transform;
        agent = GetComponent<NavMeshAgent>();
        health = maxHealth;
        slider.value = CalculateHealth();
    }
    private void Update()
    {
        if (!isDead)
        {
            slider.value = CalculateHealth();
            //Check if Player in sightrange
            playerInSightRange = Physics.CheckSphere(transform.position, sightRange, whatIsPlayer);

            //Check if Player in attackrange
            playerInAttackRange = Physics.CheckSphere(transform.position, attackRange, whatIsPlayer);

            if (!playerInSightRange && !playerInAttackRange) Patrolling();
            if (playerInSightRange && !playerInAttackRange) ChasePlayer();
            if (playerInAttackRange && playerInSightRange) AttackPlayer();
        }
    }

    private float CalculateHealth() {
        return health / maxHealth;
    }
    
    private void Patrolling()
    {
        if (isDead) return;

        head.LookAt(lookObject);
        if (!walkPointSet) SearchWalkPoint();

        //Calculate direction and walk to Point
        if (walkPointSet){
            agent.SetDestination(walkPoint);
        }

        //Calculates DistanceToWalkPoint
        Vector3 distanceToWalkPoint = transform.position - walkPoint;

        //Walkpoint reached
        if (distanceToWalkPoint.magnitude < 1f)
            walkPointSet = false;
    }

    private void SearchWalkPoint()
    {   
        //Set random coordinates (x, z) to patroll in. 
        //Temp randomization; Future implementation will include bounds
        //as well as returning to those bounds if player is out of tracking range.
        float randomZ = Random.Range(-walkPointRange, walkPointRange);
        float randomX = Random.Range(-walkPointRange, walkPointRange);

        walkPoint = new Vector3(transform.position.x + randomX, transform.position.y, transform.position.z + randomZ);

        if (Physics.Raycast(walkPoint,-transform.up, 2,whatIsGround))
        walkPointSet = true;
    }
    private void ChasePlayer()
    {
        if (isDead) return;

        head.LookAt(player);
        agent.SetDestination(player.position);
    }

    private void AttackPlayer()
    {
        if (isDead) return;
        
        //No implementation yet
        head.LookAt(player);
    }

    // Robot takes damages and will be defeat by player
    // Add "death effect" or particles when robot is defeated
    public void TakeDamage(int damage)
    {
        health -= damage;
        
        if(health <= 0)
        {
            //collisionParticleSystem.Play();
            
            this.isDead = true;
            Destroy(gameObject);
        }
    }

#region Gizmos
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, sightRange);
    }
#endregion
}
