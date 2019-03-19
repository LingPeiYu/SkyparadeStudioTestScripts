using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Block : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other)
    {
        if(other.tag=="Ball")
        {
            //count score
        }
    }
}
