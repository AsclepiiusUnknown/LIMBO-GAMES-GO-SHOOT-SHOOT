using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WeaponSwapper : MonoBehaviour
{
    [SerializeField] private List<WeaponScript> weaponList = new List<WeaponScript>();
    private WeaponScript activeWeapon;
    public TextMeshProUGUI clipAmount;
    public TextMeshProUGUI bagAmount;
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

        UpdateUI(clipAmount, bagAmount, activeWeapon);
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
            UpdateUI(clipAmount, bagAmount, activeWeapon);
        }

        if (GetComponentInParent<NetworkPlayer>() == null)
            Debug.LogError("Cannot find Network Player.");
        GetComponentInParent<NetworkPlayer>().SwitchWeapon(IDToChange);
    }

    public static void UpdateUI(TextMeshProUGUI _clip, TextMeshProUGUI _bag, WeaponScript _activeWeapon)
    {
        _clip.text = _activeWeapon.CurrentAmmo.ToString();
        _bag.text = _activeWeapon.maxAmmo.ToString();
    }
}