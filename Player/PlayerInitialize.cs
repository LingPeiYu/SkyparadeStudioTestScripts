using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInitialize : MonoBehaviour
{
    public Vector2 startPosition;

    public void Initialize()
    {
        transform.position = startPosition;
    }
}
