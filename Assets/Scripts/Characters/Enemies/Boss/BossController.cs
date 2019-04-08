﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BossController : MonoBehaviour
{

    [Header("Enemy information")]
    GameObject followPlayer;
    public float speed = 0.5f;
    public float attackDamage = 25f;
    public bool isMovable = true;
    public AudioClip deathClip;
    private Health health;
    private float maxHealth = 10000;
    private BlinkingSprite blinkingSprite;
    public GameObject projSpawner;
    private float spawnOffsetUp = 0.05f;

    public GameObject boat;
    private bool isSpawned = false;

    [Header("Throwable")]
    public GameObject normalFire;
    public GameObject heavyBomb;
    public bool canThrow = true;

    [Header("Enemy activation")]
    public const float CHANGE_SIGN = -1;

    private Rigidbody2D rb;
    private Animator animator;



    [Header("Time shoot")]
    private float shotTime = 0.0f;
    public float fireDelta = 0.5f;
    private float nextFire = 2f;

    [Header("Time sprint")]
    //private float sprintTime = 0.0f;
    //public float sprintDelta = 10f;
    //private float nextSprint = 20f;
    public Parallaxing parallax;
    public RunningTarget runningTarget;

    // Start is called before the first frame update
    void Start()
    {

        animator = GetComponent<Animator>();
       
        followPlayer = GameManager.GetPlayer();
        registerHealth();
        rb = GetComponent<Rigidbody2D>();
        blinkingSprite = GetComponent<BlinkingSprite>();
        
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (GameManager.IsGameOver())
            return;

        if (!isSpawned && boat.transform.position.x >= 44.6f)
        {
            isSpawned = true;
            parallax.setActive(false);
            AudioManager.StartBossAudio();
            StartCoroutine(Spawn());
        }

        if (isSpawned) {
            if (health.IsAlive())
            {
                float playerDistance = transform.position.x - followPlayer.transform.position.x;
                if (rb && isMovable)
                {
                    rb.isKinematic = false;
                    rb.MovePosition(rb.position + new Vector2(CHANGE_SIGN * Mathf.Sign(playerDistance) * speed, rb.position.y - 0.1f) * Time.deltaTime);
                }
                /*
                if(Random.Range(0, 10) <= 1) //20% chance of sprint
                {
                    sprintTime = sprintTime + Time.deltaTime;

                    if(sprintTime > nextSprint)
                    {
                        nextSprint = sprintTime + sprintDelta;

                        StartCoroutine(Sprint());

                        nextSprint = nextSprint - sprintTime;
                        sprintTime = 0.0f;
                    }
                    StartCoroutine(Sprint());
                }*/
                if (!(health.GetHealth() <= maxHealth / 2) && this.transform.position.y >= -.9f)
                {
                    shotTime = shotTime + Time.deltaTime;

                    if (shotTime > nextFire)
                    {
                        nextFire = shotTime + fireDelta;

                        StartCoroutine(WaitFire(normalFire));

                        nextFire = nextFire - shotTime;
                        shotTime = 0.0f;
                    }
                }
                else if (this.transform.position.y >= -.9f && health.GetHealth() <= maxHealth / 2)
                {
                    shotTime = shotTime + Time.deltaTime;

                    if (shotTime > nextFire)
                    {
                        nextFire = shotTime + fireDelta;

                        StartCoroutine(WaitFire(heavyBomb));

                        nextFire = nextFire - shotTime;
                        shotTime = 0.0f;
                    }
                }
            }
        }
    }

    private void registerHealth()
    {
        health = GetComponent<Health>();


        // register health delegate
        health.onDead += OnDead;
        health.onHit += OnHit;
        if(health.GetHealth() <= maxHealth / 2)
        {
            StartCoroutine(HalfHealth());
        }
    }

    private void OnDead(float damage)
    {
        StartCoroutine(Die());
    }

    private void OnHit(float damage)
    {
        GameManager.AddScore(damage);
        blinkingSprite.Play();
    }

    private IEnumerator Die()
    {
        animator.SetBool("isDying", true);
        if (rb)
            rb.isKinematic = true;
        if (GetComponent<BoxCollider2D>())
        {
            GetComponent<BoxCollider2D>().enabled = false;
        }
        else if (GetComponent<CapsuleCollider2D>())
        {
            GetComponent<CapsuleCollider2D>().enabled = false;
        }

        yield return new WaitForSeconds(1.8f);
        Destroy(gameObject);
    }

    public void setFollow(GameObject follow)
    {
        followPlayer = follow;
    }

   
    private IEnumerator  Spawn()
    {
        yield return new WaitForSeconds(7f);

        while (this.transform.position.y < -.1f)
        {
            this.transform.position = new Vector3( this.transform.position.x, this.transform.position.y + spawnOffsetUp, this.transform.position.z);
            yield return new WaitForSeconds(.1f);
        }

        rb.simulated = true;
        CameraManager.AfterBossSpawn();
        runningTarget.SetRunning(true);

    }

    private IEnumerator HalfHealth()
    {
        animator.SetBool("isHalfHealth", true);
        yield return new WaitForSeconds(1f);
    }

    private void OnTriggerEnter2D(Collider2D collider)
    {
        if(collider != null)
        {
            if (collider.CompareTag("Player"))
            {
                followPlayer.GetComponent<Health>().Hit(attackDamage);
                followPlayer.GetComponent<Rigidbody2D>().AddForce(new Vector3(3f, 0f));
            }

            if (collider.CompareTag("Walkable"))
            {
                GameObject bridge = collider.gameObject;
                StartCoroutine(DestroyBridge(bridge));

            }

        }
        
    }

    private IEnumerator WaitFire(GameObject throwableObj)
    {
        yield return new WaitForSeconds(0.1f);
        Instantiate(throwableObj, projSpawner.transform.position, projSpawner.transform.rotation);
        yield return new WaitForSeconds(0.15f);
    }

    private IEnumerator DestroyBridge(GameObject bridge)
    {
        yield return new WaitForSeconds(0.25f);
        bridge.GetComponent<Collider2D>().enabled = false;
        bridge.GetComponent<Animator>().SetBool("onDestroy", true);
        yield return new WaitForSeconds(1.2f);
        bridge.GetComponent<Animator>().SetBool("onDestroy", false);
        Destroy(bridge);
    }

    private IEnumerator Sprint()
    {
        rb.isKinematic = true;
        yield return new WaitForSeconds(0.5f);
        rb.isKinematic = false;
        speed = 2f;
        yield return new WaitForSeconds(0.5f);
        speed = 0.5f;
    }
}
