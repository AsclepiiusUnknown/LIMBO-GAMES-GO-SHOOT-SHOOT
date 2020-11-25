using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagZoneScript : MonoBehaviour
{
    [SerializeField] private FlagFollowerScript connectedFlag;
    private FlagControllerScript controller;

    private void Start()
    {
        controller = FindObjectOfType<FlagControllerScript>();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && connectedFlag.Follower == other.gameObject)
        {
            controller.ReturnFlag(connectedFlag);
        }
    }
}
