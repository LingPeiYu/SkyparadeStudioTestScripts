using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPointerMove : MonoBehaviour
{
    public Transform ballTransform;
    private void Update()
    {
        transform.position = new Vector2(ballTransform.position.x, transform.position.y);
    }
}
