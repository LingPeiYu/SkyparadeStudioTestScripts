using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GetScoreTrigger : MonoBehaviour
{
    public int numOfPlayer;
    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.tag == "Ball")
            GameManager._instance.CountScore(numOfPlayer);
    }
}
