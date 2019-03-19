using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallSound : MonoBehaviour
{
    public AudioSource soundOfCommonCollision;
    public AudioSource soundOfTouchGround;

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ground")
            soundOfTouchGround.Play();
        else
            soundOfCommonCollision.Play();
    }
}
