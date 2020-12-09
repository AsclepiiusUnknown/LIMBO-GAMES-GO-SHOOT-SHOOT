using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FlagControllerScript : NetworkBehaviour
{
    [SerializeField] private FlagFollowerScript blueFlag;
    [SerializeField] private FlagFollowerScript redFlag;
    private Vector3 blueFlagStartPos;
    private Vector3 redFlagStartPos;

    private void Start()
    {
        blueFlagStartPos = blueFlag.transform.position;
        redFlagStartPos = redFlag.transform.position;
        blueFlag.EnablePickup();
        redFlag.EnablePickup();
    }

    public void CheckDeadPlayerForFlag(GameObject player)
    {
        if (blueFlag.Follower == player)
        {
            blueFlag.Drop();
        } 
        else if (redFlag.Follower == player)
        {
            redFlag.Drop();
        }
    }

    public void ReturnFlag(FlagFollowerScript flag)
    {
        if (flag == blueFlag)
        {
            blueFlag.ReturnToPos(blueFlagStartPos);
        }
        else
        {
            redFlag.ReturnToPos(redFlagStartPos);
        }
    }

    public void ReceivePickupFromFlag(FlagFollowerScript flag, GameObject pickuper)
    {
        flag.DisablePickup();
        flag.SetFollow(pickuper);
    }

    public void ReceivePickupFromFlag(FlagFollowerScript flag, uint pickuperID)
    {
        flag.DisablePickup();
        foreach (NetworkIdentity id in FindObjectsOfType<NetworkIdentity>())
        {
            if (id.netId == pickuperID)
            {
                flag.SetFollow(id.gameObject);
            }
        }
    }
}
