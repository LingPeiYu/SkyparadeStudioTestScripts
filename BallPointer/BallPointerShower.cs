using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallPointerShower : MonoBehaviour
{
    public GameObject ballPointArrow;
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.tag == "Ball")
        {
            float direction = collision.GetComponent<Rigidbody2D>().velocity.y;//获得方向
            if (direction > 0)
                ballPointArrow.SetActive(true);
            else
                ballPointArrow.SetActive(false);
        }
    }
}
