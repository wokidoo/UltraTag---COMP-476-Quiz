using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Obstacle : MonoBehaviour
{
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Unit"))
        {
            collision.gameObject.GetComponent<IndividualAI>().KillUnit();
        }
    }
}
