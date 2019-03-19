using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float jumpVelocity;
    public float hitUpVelocity;
    public float hitForwardVelocity;
    public float speed;
    public AudioSource jumpSound;//跳跃音效
    public GameObject jumpEffect;//跳跃效果

    private float moveInfo;
    private bool isJumping;
    private Rigidbody2D rigidbody2d;
    private Animator animator; 

    private void Jump()
    {
        isJumping = true;
        jumpEffect.transform.position = new Vector2(transform.position.x, jumpEffect.transform.position.y);
        jumpEffect.SetActive(true);
        jumpSound.Play();
        rigidbody2d.velocity = new Vector2(0f,jumpVelocity);
        animator.SetTrigger("Jump");
    }

    private void Hit()
    {
        isJumping = true;
        jumpEffect.transform.position = new Vector2(transform.position.x, jumpEffect.transform.position.y);
        jumpEffect.SetActive(true);
        jumpSound.Play();
        rigidbody2d.velocity = new Vector2(hitForwardVelocity, hitUpVelocity);
        animator.SetTrigger("Hit");
    }

    private void OnEnable()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        moveInfo = Input.GetAxisRaw("Horizontal") * speed;
        if (!isJumping)
        {
            if (Input.GetKey(KeyCode.Z))
            {
                Jump();
            }
            else if(Input.GetKey(KeyCode.X))
            {
                Hit();
            }
            else
            {
                rigidbody2d.velocity = new Vector2(moveInfo, 0f);
                if (moveInfo == 0)
                    animator.SetBool("HaveSpeed", false);
                else
                    animator.SetBool("HaveSpeed", true);
            }
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if(isJumping&&collision.gameObject.tag=="Ground")
        {
            jumpEffect.SetActive(false);
            isJumping = false;
            animator.SetTrigger("GetToGround");
        }
    }

    private void OnCollisionStay2D(Collision2D collision)//防止球与地面同时与player碰撞
    {
        if (isJumping && collision.gameObject.tag == "Ground")
        {
            jumpEffect.SetActive(false);
            isJumping = false;
            animator.SetTrigger("GetToGround");
        }
    }
}
