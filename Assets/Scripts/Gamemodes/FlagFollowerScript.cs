using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Mirror;

public class FlagFollowerScript : NetworkBehaviour
{
    [SerializeField] private Vector3 followOffset = new Vector3(0, 1, 0);
    private FlagControllerScript controller;
    private bool pickupAllowed = true;
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

    [Server]
    private void OnTriggerEnter(Collider other)
    {
        if (pickupAllowed && other.CompareTag("Player") && other.GetComponentInParent<NetworkIdentity>())
        {
            RpcReceiveCall(other.GetComponentInParent<NetworkIdentity>().netId);
        }
    }

    [ClientRpc]
    public void RpcReceiveCall(uint networkID)
    {
        print("Tehaehaeh");
        controller.ReceivePickupFromFlag(this, networkID);
    }
}
