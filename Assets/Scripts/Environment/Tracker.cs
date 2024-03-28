using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tracker : MonoBehaviour
{
    // Start is called before the first frame update
    [SerializeField] private GameObject player;
    void Start()
    {
        // transform.LookAt(player.transform);
    }

    // Update is called once per frame
    void Update()
    {
        // transform.LookAt(player.transform, Vector3.left);
    }
}
