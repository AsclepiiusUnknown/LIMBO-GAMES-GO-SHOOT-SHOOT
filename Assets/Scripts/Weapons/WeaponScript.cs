using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    [SerializeField] private int maxAmmo;
    private int currentAmmo;
    [SerializeField] private int ammoPerShot = 1;
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float criticalDamage = 0f;
    [SerializeField] private float range = 100f;
    [SerializeField] private bool projectile;
    [SerializeField] private Transform firePosition;
    [SerializeField] private bool tracer = false;
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private float tracerLifetime = 0.1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float recoil = 1f;
    private DamageReceiverScript playerRef;
    private Camera playerCamera;
    private bool isInputAvailable;
    private int idleLoopCount = 0;

    private void Start()
    {
        UpdateAmmo(maxAmmo);
        isInputAvailable = true;
    }

    private void Update()
    {
        if (isInputAvailable)
        {
            CheckForInput();
        }
    }

    void CheckForInput()
    {
        if (Input.GetMouseButtonDown(0))
        {
            SetAnimatorTrigger("Fired");
            MakeInputUnavailable();
        }
        else if (Input.GetKeyDown(KeyCode.R))
        {
            SetAnimatorTrigger("Reload");
            MakeInputUnavailable();
        }
    }

    public void MakeInputAvailable()
    {
        isInputAvailable = true;
    }

    void MakeInputUnavailable()
    {
        isInputAvailable = false;
        idleLoopCount = 0;
    }

    void SetAnimatorTrigger(string triggerName)
    {
        Animator anim = GetComponent<Animator>();
        anim.SetTrigger(triggerName);
    }

    public virtual void FireAction()
    {
        RaycastHit hit = new RaycastHit();
        Ray rayC = new Ray(playerCamera.gameObject.transform.position, playerCamera.gameObject.transform.forward);
        bool prjFired = false;
        bool crit = false;
        if (Physics.Raycast(rayC, out hit, range * 3))
        {
            if (projectile)
            {
                SpawnProjectile(firePosition.transform.position, Quaternion.LookRotation(hit.point - firePosition.transform.position, Vector3.up), baseDamage);
                prjFired = true;
            }
            else if (hit.collider.GetComponent<DamageReceiverScript>() && hit.collider.GetComponent<DamageReceiverScript>().IsDamageAllowed(playerRef, false))
            {
                if (criticalDamage > 0 && hit.collider == hit.collider.GetComponent<DamageReceiverScript>().CriticalCollider)
                {
                    crit = true;
                }
                float dmg = CalculateDamage(Vector3.Distance(hit.point, playerCamera.gameObject.transform.position), crit);
                DealDamage(hit.collider.gameObject, dmg, crit);
            }
            if (tracer)
            {
                SpawnTracer(hit.point);
            }
        }
        else if (tracer)
        {
            SpawnTracer((playerCamera.gameObject.transform.forward * range * 3) + playerCamera.gameObject.transform.position);
        }
        if (projectile && !prjFired)
        {
            SpawnProjectile(firePosition.transform.position, Quaternion.LookRotation((playerCamera.gameObject.transform.forward * range * 3 + playerCamera.gameObject.transform.position) - firePosition.transform.position, Vector3.up), baseDamage);
        }

        //Recoil here: playerRef.ReceiveRecoil(recoil);

        UpdateAmmo(-ammoPerShot);
    }

    public void ReloadAction()
    {
        ReloadActionPartial(maxAmmo);
    }

    public void ReloadActionPartial(int ammoToReload)
    {
        UpdateAmmo(ammoToReload);
    }

    public void CheckForReloadComplete()
    {
        if (currentAmmo >= maxAmmo)
        {
            SetAnimatorTrigger("ReloadComplete");
        }
    }

    public void DealDamage(GameObject target, float damageToDeal, bool critical = false, bool showNumber = true)
    {
        if (target.GetComponent<DamageReceiverScript>())
        {
            target.GetComponent<DamageReceiverScript>().ReceiveDamage(damageToDeal, playerRef, critical, showNumber);
        }
    }

    void UpdateAmmo(int ammoToChange = 0)
    {
        currentAmmo = Mathf.Clamp(currentAmmo + ammoToChange, 0, maxAmmo);
        //Update HUD.
    }

    public void SpawnProjectile(Vector3 position, Quaternion rotation, float inDamage = 0f)
    {
        GameObject prj = Instantiate(projectilePrefab);
        prj.transform.position = position;
        prj.transform.rotation = rotation;

        prj.GetComponent<ProjectileScript>().SetUpProjectile(inDamage, criticalDamage, playerRef);
        prj.GetComponent<ProjectileScript>().BeginMovement();
    }

    float CalculateDamage(float distance = 0f, bool crit = false)
    {
        float dmg = baseDamage;
        if (distance > range)
        {
            dmg = Mathf.Clamp(dmg - ((distance - range) / (range * 2) * baseDamage), 1, baseDamage);
        }
        if (crit)
        {
            dmg += Mathf.Clamp(dmg * criticalDamage, 1, 1000);
        }
        return Mathf.Floor(dmg);
    }

    public void SpawnTracer(Vector3 endpoint)
    {
        SpawnTracer(endpoint, firePosition.transform.position);
    }

    public void SpawnTracer(Vector3 endpoint, Vector3 startpoint)
    {
        GameObject trace = Instantiate(tracerPrefab);
        if (trace.GetComponent<LineRenderer>())
        {
            trace.GetComponent<LineRenderer>().SetPosition(0, startpoint);
            trace.GetComponent<LineRenderer>().SetPosition(1, endpoint);
            Destroy(trace, tracerLifetime);
        }
    }

    public void SpawnShell(int shellIndex)
    {

    }

    public void PlayAudioClip(string clipName)
    {
        print("Imagine " + clipName + " is playing right now.");
    }

    public void LogIdle()
    {
        if (idleLoopCount >= 7)
        {
            idleLoopCount = 0;
            SetAnimatorTrigger("IdleTick");
        }
    }
}
