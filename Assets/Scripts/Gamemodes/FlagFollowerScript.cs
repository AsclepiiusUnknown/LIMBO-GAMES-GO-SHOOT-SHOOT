using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagFollowerScript : MonoBehaviour
{
    [SerializeField] private Vector3 followOffset = new Vector3(0, 1, 0);
    private FlagControllerScript controller;
    private bool pickupAllowed = false;
    private GameObject follower = null;
    public GameObject Follower
    {
        get
        {
            return follower;
        }
    }

    private void Start()
    {
        controller = FindObjectOfType<FlagControllerScript>();
    }

    private void Update()
    {
        if (follower)
        {
            transform.position = follower.transform.position + followOffset;
        }
    }

    public void DisablePickup()
    {
        pickupAllowed = false;
    }
    public void EnablePickup()
    {
        pickupAllowed = true;
    }

    public void SetFollow(GameObject follow)
    {
        follower = follow;
    }

    public void Drop()
    {
        transform.position = follower.transform.position;
        EnablePickup();
    }

    public void ReturnToPos(Vector3 pos)
    {
        transform.position = pos;
        EnablePickup();
    }

    private void OnTriggerEnter(Collider other)
    {
        if (pickupAllowed && other.CompareTag("Player"))
        {
            controller.ReceivePickupFromFlag(this, other.gameObject);
        }
    }
}
