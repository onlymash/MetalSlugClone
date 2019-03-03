﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletMovement : MonoBehaviour
{

    private Rigidbody2D rb;
    public float bulletForce = 3;
    public float lifeTime = 5;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Vector3 bulletDirection = transform.right;

        rb.AddForce(bulletDirection * bulletForce, ForceMode2D.Impulse);
    }

    void Update()
    {
        lifeTime -= Time.deltaTime;
        if (lifeTime <= 0)
            Destroy(this.gameObject);
    }

    //Destroy the bulled when out of camera
    private void OnBecameInvisible()
    {
        Destroy(gameObject);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.tag == "Enemy")
        {
            collision.gameObject.GetComponent<EnemyControl>().hit();
            Destroy(gameObject);
        }
    }
}
