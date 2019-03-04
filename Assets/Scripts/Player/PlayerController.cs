﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    public float maxSpeed = 1f;
    public float maxJump = 1f;
    private bool isGrounded = false;

    //Sprite orientation
    private bool facingRight = true;
    private bool wasCrounching = false;
    private bool wasFiring = false;

    //Marco Controller
    public Animator topAnimator;
    public Animator bottomAnimator;
    public GameObject Up;

    private Rigidbody2D rb;

    // Time shoot
    private float shotTime = 0.0f;
    public float fireDelta = 0.5f;
    private float nextFire = 0.5f;

    //Time jump
    private float jumpTime = 0.0f;
    public float jumpDelta = 0.5f;
    private float nextJump = 0.5f;

    //Bullet
    public GameObject projectile;

    //Bullet spawner
    public GameObject projSpawner;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        Fire();
        MoveHorizontally();
        MoveVertically();
        Jump();
        Crouch();

        FlipShoot();   
    }

    void FixedUpdate()
    {
 
    }

   void Fire()
    {
        shotTime = shotTime + Time.deltaTime;

        if (Input.GetButton("Fire1"))
        {
            if (!wasFiring)
            {
                topAnimator.SetBool("isFiring", true);
                bottomAnimator.SetBool("isFiring", true);

                if (shotTime > nextFire)
                {
                    nextFire = shotTime + fireDelta;

                    StartCoroutine(WaitFire());

                    nextFire = nextFire - shotTime;
                    shotTime = 0.0f;
                }

                wasFiring = true;
            }
            else
            {
                topAnimator.SetBool("isFiring", false);
                bottomAnimator.SetBool("isFiring", false);
            }
        }
        else
        {
            topAnimator.SetBool("isFiring", false);
            bottomAnimator.SetBool("isFiring", false);
            wasFiring = false;
        }
    }

    void MoveHorizontally()
    {
        float moveH = Input.GetAxis("Horizontal");

        if (moveH != 0 && !(bottomAnimator.GetBool("isCrouched") && topAnimator.GetBool("isFiring")))
        {
            rb.velocity = new Vector2(moveH * maxSpeed, rb.velocity.y);
            topAnimator.SetBool("isWalking", true);
            bottomAnimator.SetBool("isWalking", true);

            //Flip sprite orientantion if the user is walking right or left
            if (moveH > 0 && !facingRight)
            {
                //Moving right
                Flip();
            }
            else if (moveH < 0 && facingRight)
            {
                //Moving left
                Flip();
            }
        }
        else
        {
            topAnimator.SetBool("isWalking", false);
            bottomAnimator.SetBool("isWalking", false);
        }
    }

    void MoveVertically()
    {
        float moveV = Input.GetAxis("Vertical");

        if (moveV != 0)
        {
            //Yes

            //bottomAnimator.SetBool("isWalking", true);

            //Flip sprite orientantion if the user is walking right or left
            if (moveV > 0)
            {
                //Moving UP
                topAnimator.SetBool("isLookingUp", true);
            }
            else if (moveV < 0)
            {
                //Moving down
            }
        }
        else
        {
            //No
            if (topAnimator.GetBool("isLookingUp"))
            {
                topAnimator.SetBool("isLookingUp", false);
            }
        }
    }

    void Jump()
    {

        jumpTime = jumpTime + Time.deltaTime;

        if (Input.GetButton("Jump") && isGrounded && !bottomAnimator.GetBool("isCrouched"))
        {
            if (jumpTime > nextJump)
            {
                rb.AddForce(new Vector3(0, maxJump, 0), ForceMode2D.Impulse);
                topAnimator.SetBool("isJumping", true);
                bottomAnimator.SetBool("isJumping", true);
                isGrounded = false;

                nextJump = jumpTime + jumpDelta;
                nextJump = nextJump - jumpTime;
                jumpTime = 0.0f;
            }
        }
    }

    void Crouch()
    {
        if (Input.GetButton("Crouch") && !Input.GetButton("Jump") && (!(bottomAnimator.GetBool("isWalking") && !wasCrounching) || !bottomAnimator.GetBool("isWalking")))
        {
            topAnimator.SetBool("isCrouched", true);
            bottomAnimator.SetBool("isCrouched", true);

            if (isGrounded)
            {
                StartCoroutine(WaitCrouch());
            }

            if (!wasCrounching)
            {
                maxSpeed -= 0.4f;
                projSpawner.transform.position = new Vector3(projSpawner.transform.position.x, projSpawner.transform.position.y - 0.14f, 0);
            }

            wasCrounching = true;
        }
        else
        {
            topAnimator.SetBool("isCrouched", false);
            bottomAnimator.SetBool("isCrouched", false);

            if (isGrounded)
            {
                Up.SetActive(true);
            }

            if (wasCrounching)
            {
                maxSpeed += 0.4f;
                projSpawner.transform.position = new Vector3(projSpawner.transform.position.x, projSpawner.transform.position.y + 0.14f, 0);
            }
            wasCrounching = false;
        }
    }

    //Flip sprite
    void Flip()
    {
        Vector3 scale = transform.localScale;
        scale.x *= -1;
        transform.localScale = scale;

        //transform.localEulerAngles = transform.eulerAngles + new Vector3(0, 180, -2 * transform.eulerAngles.z);
        facingRight = !facingRight;
    }

    void FlipShoot()
    {
        if (topAnimator.GetBool("isLookingUp") && facingRight)
        {
            //Fire up
            projSpawner.transform.localEulerAngles = new Vector3(0, 0, 90);
        } else if (topAnimator.GetBool("isLookingUp") && !facingRight) {
            //Fire up
            projSpawner.transform.localEulerAngles = new Vector3(0, 0, 270);
        } else if (topAnimator.GetBool("isCrouched") && !isGrounded && facingRight)
        {
            //Fire down
            projSpawner.transform.localEulerAngles = new Vector3(0, 0, 270);
        }
        else if (topAnimator.GetBool("isCrouched") && !isGrounded && !facingRight)
        {
            //Fire down
            projSpawner.transform.localEulerAngles = new Vector3(0, 0, 90);
        }
        else if (facingRight)
        {
            //Fire right
            projSpawner.transform.localEulerAngles = new Vector3(0, 0, 0);
        }
        else
        {
            //Fire left
            projSpawner.transform.localEulerAngles = new Vector3(0, 0, 180);
        }
    }

    void OnCollisionStay2D(Collision2D col)
    {
        if (col.gameObject.tag == "Walkable")
        {
            isGrounded = true;
            topAnimator.SetBool("isJumping", false);
            bottomAnimator.SetBool("isJumping", false);
        }
    }

    private IEnumerator WaitFire()
    {
        yield return new WaitForSeconds(0.2f);
        Instantiate(projectile, projSpawner.transform.position, projSpawner.transform.rotation);
    }

    private IEnumerator WaitCrouch()
    {
        yield return new WaitForSeconds(0.25f);
        Up.SetActive(false);
    }
}
