using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponSwapper : MonoBehaviour
{
    [SerializeField] private List<WeaponScript> weaponList = new List<WeaponScript>();
    private WeaponScript activeWeapon;

    private void Start()
    {
        for (int i = 0; i < weaponList.Count; i++)
        {
            if (weaponList[i].gameObject.activeInHierarchy)
            {
                activeWeapon = weaponList[i];
                break;
            }
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            ChangeWeapon(0);
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            ChangeWeapon(1);
        }
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            ChangeWeapon(2);
        }
    }

    private void ChangeWeapon(int IDToChange)
    {
        if (activeWeapon.IsInputAvailable && activeWeapon != weaponList[IDToChange])
        {
            activeWeapon.gameObject.SetActive(false);
            weaponList[IDToChange].gameObject.SetActive(true);
        }
    }
}
