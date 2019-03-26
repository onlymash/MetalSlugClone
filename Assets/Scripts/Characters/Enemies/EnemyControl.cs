﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyControl : MonoBehaviour
{
    [Header("Enemy information")]
    GameObject followPlayer;
    public float speed = 0.5f;
    public float attackDamage = 10f;
    public bool isMovable = true;
    public AudioClip deathClip;
    private Health health;
    private BlinkingSprite blinkingSprite;


    [Header("Throwable")]
    public GameObject throwableObj;
    public bool canThrow = false;

    [Header("Enemy activation")]
    public float activationDistance = 1.8f;
    public float attackDistance = 0.5f;
    public const float CHANGE_SIGN = -1;

    private Rigidbody2D rb;
    private Animator animator;
    public bool facingRight = false;

    //Enemy gravity
    private bool collidingDown = false;
    Vector2 velocity = Vector2.zero;

    [Header("Time shoot")]
    private float shotTime = 0.0f;
    public float fireDelta = 0.5f;
    private float nextFire = 0.5f;

    private void Start()
    {
        followPlayer = GameManager.GetPlayer();
        animator = GetComponent<Animator>();
        rb = GetComponent<Rigidbody2D>();
        blinkingSprite = GetComponent<BlinkingSprite>();
        registerHealth();
    }

    public void setFollow(GameObject follow)
    {
        followPlayer = follow;
    }

    private void registerHealth()
    {
        health = GetComponent<Health>();
        // register health delegate
        health.onDead += OnDead;
        health.onHit += OnHit;
    }

    private void Update()
    {
        if (GameManager.IsGameOver())
            return;
    }

    void FixedUpdate()
    {
        if (GameManager.IsGameOver())
            return;

        //transform.Rotate(new Vector3(0, -90, 0), Space.Self);//correcting the original rotation


        if (health.IsAlive())
        {
            animator.SetBool("isFalling", !collidingDown);

            float playerDistance = transform.position.x - followPlayer.transform.position.x;
            if (playerDistance < activationDistance)
            {
                if (Mathf.Abs(playerDistance) <= attackDistance)
                {
                    //Attack player - Primary attack (near)
                    animator.SetBool("isAttacking", true);
                    animator.SetBool("isAttacking_2", false);

                    if (rb)
                        rb.isKinematic = true;


                    shotTime = shotTime + Time.deltaTime;

                    if (shotTime > nextFire)
                    {
                        nextFire = shotTime + fireDelta;

                        if (canThrow)
                        {
                            //Instantiate throwable
                        }
                        else
                        {
                            followPlayer.GetComponent<Health>().Hit(attackDamage);
                        }

                        nextFire = nextFire - shotTime;
                        shotTime = 0.0f;
                    }
                }
                else
                {
                    //Move to the player
                    if (rb && isMovable)
                    {
                        rb.isKinematic = false;
                        if (collidingDown)
                        {
                            rb.MovePosition(rb.position + new Vector2(CHANGE_SIGN * Mathf.Sign(playerDistance) * speed, rb.position.y) * Time.deltaTime);
                        }
                        else
                        {
                            //velocity.y -= 9.81f * Time.deltaTime;
                            //rb.MovePosition(new Vector2(transform.position.x, velocity.y));
                            rb.MovePosition(rb.position + new Vector2(CHANGE_SIGN * Mathf.Sign(playerDistance) * speed, rb.position.y - 0.1f) * Time.deltaTime);
                        }

                        animator.SetBool("isWalking", true);
                        animator.SetBool("isAttacking", false);
                        animator.SetBool("isAttacking_2", false);
                    }
                    else
                    {
                        //Attack player - secondary attack (far) - used by soldiers granate
                        animator.SetBool("isAttacking_2", true);
                        animator.SetBool("isAttacking", false);

                        /*if (rb)
                            rb.isKinematic = true;*/

                        rb.isKinematic = false;
                        
                        shotTime = shotTime + Time.deltaTime;

                        if (shotTime > nextFire)
                        {
                            nextFire = shotTime + fireDelta;

                            nextFire = nextFire - shotTime;
                            shotTime = 0.0f;
                        }
                    }


                }
            }

            //Flip enemy
            if (playerDistance < 0 && !facingRight)
            {
                Flip();
            }
            else if (playerDistance > 0 && facingRight)
            {
                Flip();
            }
        }

    }

    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        facingRight = !facingRight;
    }

    private void OnDead(float damage)
    {
        StartCoroutine(Die());
    }

    private void OnHit(float damage)
    {
        animator.SetTrigger("isHitten");

        GameManager.AddScore(damage);
        blinkingSprite.Play();
    }

    private IEnumerator Die()
    {
        PlayDeathAudio();
        animator.SetBool("isDying", true);
        if (rb)
            rb.isKinematic = true;
        GetComponent<BoxCollider2D>().enabled = false;

        yield return new WaitForSeconds(1f);
        Destroy(gameObject);
    }

    private void PlayDeathAudio()
    {
        if (deathClip)
            AudioManager.PlayEnemyDeathAudio(deathClip);
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.collider.CompareTag("Walkable"))
        {
            collidingDown = true;
        }
    }

    private void OnCollisionExit(Collision collision)
    {
        if (collision.collider.CompareTag("Walkable"))
        {
            collidingDown = false;
        }
    }
}
