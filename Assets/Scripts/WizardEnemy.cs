using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class WizardEnemy : GrabThrow
{
    public int numHits;
    public float idleTime;
    public float updatePlayerPathTime;
    public float walkRadius;
    public EnemyVisionSphere visionSphere;
    public NavMeshAgent navmeshAgent;
    public Animator animator;
    public GameObject axe;
    public GameObject moveAwayRadius;

    private Vector3 walkDirection;
    private float timer;
    private EnemyState state;

    public enum EnemyState { IDLE, WALK, GRABBED, PLAYER_DETECTED, DIZZY };

    // Start is called before the first frame update
    void Start()
    {
        rb.isKinematic = true;
        rb.useGravity = false;
        timer = idleTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (state != EnemyState.PLAYER_DETECTED && state != EnemyState.GRABBED && state != EnemyState.DIZZY)
        {
            if (visionSphere.PlayerDetected())
            {
                navmeshAgent.isStopped = false;
                timer = updatePlayerPathTime;
                animator.SetTrigger("enter_jog");
                state = EnemyState.PLAYER_DETECTED;
            }

        }

        if (state == EnemyState.IDLE)
        {
            timer -= Time.deltaTime;

            if (timer < 0)
            {
                navmeshAgent.isStopped = false;
                state = EnemyState.WALK;
                animator.SetTrigger("enter_jog");
                Walk();
            }
        }

        if (state == EnemyState.WALK)
        {
            if (Vector3.Distance(transform.position, walkDirection) < 0.05)
            {
                navmeshAgent.isStopped = true;
                state = EnemyState.IDLE;
                timer = idleTime;
                animator.SetTrigger("enter_idle");
            }
        }

        if (state == EnemyState.GRABBED)
        {

        }

        if (state == EnemyState.PLAYER_DETECTED)
        {
            timer -= Time.deltaTime;
            Vector3 playerPosition = visionSphere.PlayerPosition();
            if (playerPosition == Vector3.zero)
            {
                timer = idleTime;
                state = EnemyState.IDLE;
                navmeshAgent.isStopped = true;
                animator.SetTrigger("enter_idle");
            }
            else if (timer < 0 && playerPosition != Vector3.zero)
            {
                timer = updatePlayerPathTime;
                navmeshAgent.destination = GetNavmeshPlayerPosition(playerPosition);
            }

            if (playerPosition != Vector3.zero && Vector3.Distance(transform.position, playerPosition) < 0.05f)
            {
                StartCoroutine(Pause(1.25f));
                animator.SetTrigger("enter_attack");
            }

        }

        if (state == EnemyState.DIZZY)
        {

        }


    }

    public override void MoveToHandInit()
    {
        navmeshAgent.enabled = false;
        rb.isKinematic = false;
        state = EnemyState.GRABBED;
        animator.SetTrigger("enter_falling");
        rb.detectCollisions = true;
        base.MoveToHandInit();

    }

    public void GetHitByAxe()
    {
        StartCoroutine(Pause(1.75f));
        GetHit(1);
    }

    private void GetHit(int damage)
    {
        animator.SetTrigger("enter_hit");
        numHits -= damage;

        if (numHits <= 0)
        {
            animator.SetTrigger("enter_dizzy");
            navmeshAgent.isStopped = true;
            state = EnemyState.DIZZY;
        }
    }

    void Walk()
    {
        walkDirection = GetRandomDirection();
        navmeshAgent.destination = walkDirection;
    }

    Vector3 GetRandomDirection()
    {
        Vector3 randomDirection = transform.position + Random.insideUnitSphere * walkRadius;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, walkRadius, 1);
        return hit.position;
    }

    Vector3 GetNavmeshPlayerPosition(Vector3 playerPosition)
    {
        Vector3 navmeshPlayerPosition = Vector3.zero;
        NavMeshHit hit;
        if (NavMesh.SamplePosition(playerPosition, out hit, walkRadius, 1))
        {
            navmeshPlayerPosition = hit.position;
        }
        return navmeshPlayerPosition;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Throwable") && collision.gameObject.GetComponent<GrabThrow>().grabbed)
        {
            GetHit(5);
        }
    }

    private IEnumerator Pause(float pauseTime)
    {
        navmeshAgent.isStopped = true;
        timer = 100f;
        yield return new WaitForSeconds(pauseTime);
        

        if (state != EnemyState.GRABBED && state != EnemyState.DIZZY)
        {
            navmeshAgent.isStopped = false;
            if (visionSphere.PlayerDetected())
            {
                state = EnemyState.PLAYER_DETECTED;
                timer = updatePlayerPathTime;
                navmeshAgent.destination = GetNavmeshPlayerPosition(visionSphere.PlayerPosition());
            }
            else
            {
                state = EnemyState.IDLE;
                timer = idleTime;
            }
        }
       
    }

    public void FindPositionAwayFromPlayer(Vector3 playerPosition)
    {

    }

    public void TurnOnAxeCollision()
    {
        axe.GetComponent<AxeCollision>().TurnOnCollider();
    }

    public void TurnOffAxeCollision()
    {
        axe.GetComponent<AxeCollision>().TurnOffCollider();
    }
}
