using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwapper : MonoBehaviour
{
    [SerializeField] private List<WeaponScript> weaponList = new List<WeaponScript>();
    private WeaponScript activeWeapon;
    NetworkPlayer networkPlayer;
    LIMBO.Movement.PlayerMovement playerMovement;

    private void Start()
    {
        networkPlayer = GetComponentInParent<NetworkPlayer>();
        playerMovement = GetComponent<LIMBO.Movement.PlayerMovement>();

        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i].gameObject.activeInHierarchy)
            {
                activeWeapon = weaponList[i];
                networkPlayer.weapon = activeWeapon;
                break;
            }
        }
    }

    private void Update()
    {
        if (!playerMovement.IsSetup)
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            // print("This should get here.");
            ChangeWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeWeapon(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3) && weaponList.Count > 2)
        {
            ChangeWeapon(2);
        }
    }

    public void ChangeWeapon(int IDToChange, bool _isLocalPlayer)
    {
        if (!_isLocalPlayer)
            ChangeWeapon(IDToChange);
    }

    private void ChangeWeapon(int IDToChange)
    {
        if (activeWeapon.IsInputAvailable && activeWeapon != weaponList[IDToChange])
        {
            activeWeapon.gameObject.SetActive(false);
            weaponList[IDToChange].gameObject.SetActive(true);
            activeWeapon = weaponList[IDToChange];
            networkPlayer.weapon = activeWeapon;
        }

        if (GetComponentInParent<NetworkPlayer>() == null)
            Debug.LogError(" ");
        GetComponentInParent<NetworkPlayer>().SwitchWeapon(IDToChange);
    }
}