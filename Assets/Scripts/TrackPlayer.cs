using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class TrackPlayer : MonoBehaviour
{
    public Transform player;
    void Awake()
    {
        player = GameObject.Find("Slime").transform;
    }

    // Update is called once per frame
    void Update()
    {
        this.transform.LookAt(player);
    }
}
