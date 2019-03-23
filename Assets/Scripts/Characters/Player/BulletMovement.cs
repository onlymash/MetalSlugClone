﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{

    private Rigidbody2D rb;
    public float bulletForce = 3;
    public float lifeTime = 5;
    public float damageShot = 100;
    private float expireTime;
    private bool isSpawned;

    void OnEnable()
    {
        Init();
    }

    private void Init()
    {
        expireTime = lifeTime;
        Vector3 bulletDirection = transform.right;
        rb = GetComponent<Rigidbody2D>();
        rb.AddForce(bulletDirection * bulletForce, ForceMode2D.Impulse);
        isSpawned = true;
    }

    void Update()
    {
        if (!isSpawned)
            return;

        expireTime -= Time.deltaTime;
        if (expireTime <= 0)
        {
            isSpawned = false;
            BulletManager.GetNormalBulletPool().Despawn(this.gameObject);
        }
    }

    //Destroy the bulled when out of camera
    private void OnBecameInvisible()
    {
        BulletManager.GetNormalBulletPool().Despawn(this.gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Enemy"))
        {
            collision.gameObject.GetComponent<Health>().Hit(damageShot);
            AudioManager.PlayShotHitAudio();
            BulletManager.GetNormalBulletPool().Despawn(this.gameObject);
        }
        else if (collision.CompareTag("Building"))
        {
            collision.gameObject.GetComponent<Health>().Hit(damageShot);
            AudioManager.PlayShotHitAudio();
            BulletManager.GetNormalBulletPool().Despawn(this.gameObject);
        }
    }
}
