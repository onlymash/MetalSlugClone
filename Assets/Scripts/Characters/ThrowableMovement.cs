﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThrowableMovement : MonoBehaviour
{
    [Header("Throwable Details")]
    private float throwableDamage = 300;
    public float throwableForce = 2.5f;

    public enum LauncherType
    {
        Player,
        Enemy
    };
    public LauncherType launcher = LauncherType.Player;

    public enum ThrowableType
    {
        Grenade,
        EnemyGrenade,
        Vomit,
        Bubble
    };
    public ThrowableType throwable = ThrowableType.Grenade;

    public bool canExplode = true;


    private Animator throwableAnimator;
    private Rigidbody2D rb;

    Vector3 throwableDirection;
    private Vector2 startingPoint;
    private Vector2 controlPoint;
    private Vector2 endingPoint;
    private bool hasHit;
    private bool isSpawned;

    private void Start()
    {
        throwableAnimator = GetComponent<Animator>();
    }

    void OnEnable()
    {
        Init();
    }

    void Init()
    {
        rb = GetComponent<Rigidbody2D>();
        switch (rb.rotation)
        {
            case 0:
                throwableDirection = Quaternion.AngleAxis(45, Vector3.forward) * Vector3.right;
                break;
            case 180:
                throwableDirection = Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.left;
                break;
            case -90:
                throwableDirection = Quaternion.AngleAxis(-45, Vector3.forward) * Vector3.left;
                break;
            case 90:
                throwableDirection = Quaternion.AngleAxis(45, Vector3.forward) * Vector3.right;
                break;
        }

        rb.gravityScale = .5f;
        rb.rotation = 0;
        rb.AddForce(throwableDirection * throwableForce, ForceMode2D.Impulse);
        hasHit = false;
        isSpawned = true;
    }

    private void Despawn()
    {
        if (!isSpawned)
            return;


        isSpawned = false;

        if (throwable == ThrowableType.Grenade)
        {
            //Is a Grenade
            BulletManager.GetGrenadePool().Despawn(this.gameObject);
        }
        else
        {
            //Is an enemy throwable
            Destroy(gameObject);
        }
        
    }

    //Destroy the bulled when out of camera
    private void OnBecameInvisible()
    {
        Despawn();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (hasHit)
            return;

        if (GameManager.CanTriggerGrenade(collision.tag) && !(collision.tag == "Player" && launcher == LauncherType.Player) && !(collision.tag == "Enemy" && launcher == LauncherType.Enemy))
        {
            hasHit = true;
            if (canExplode)
            {
                StartCoroutine(Explosion(collision));
            }
            else
            {
                ResetMovement(collision);
                Despawn();
            }
        }
    }

    private IEnumerator Explosion(Collider2D collision)
    {
        AudioManager.PlayGrenadeHitAudio();
        throwableAnimator.SetBool("hasHittenSth", true);

        ResetMovement(collision);

        yield return new WaitForSeconds(1.7f);
        throwableAnimator.SetBool("hasHittenSth", false);
        Despawn();
    }

    private void ResetMovement(Collider2D collision)
    {
        collision.GetComponent<Health>()?.Hit(throwableDamage);

        rb.angularVelocity = 0;
        rb.gravityScale = 0;
        rb.velocity = Vector2.zero;
    }
}