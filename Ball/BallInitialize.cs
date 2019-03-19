using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallInitialize : MonoBehaviour
{
    public float initializeSpeeed;
    public float pauseTime;

    private Rigidbody2D rigidbody2d;
    private float gScale;
    private Vector3 startPosition;

    public IEnumerator InitializeWithDelay(int turn)
    {
        rigidbody2d.velocity = Vector2.zero;
        transform.position = startPosition; 
        rigidbody2d.gravityScale = 0;
        yield return new WaitForSeconds(pauseTime);
        rigidbody2d.gravityScale = gScale;

        if (turn%2==0)
        {
            rigidbody2d.velocity = Vector2.left * initializeSpeeed;
        }
        else
        {
            rigidbody2d.velocity = Vector2.left * (-initializeSpeeed);
        }
    }

    private void OnEnable()
    {
        rigidbody2d = GetComponent<Rigidbody2D>();
        gScale = rigidbody2d.gravityScale;
        startPosition = transform.position;
    }
}
