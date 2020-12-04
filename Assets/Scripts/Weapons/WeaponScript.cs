using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponScript : MonoBehaviour
{
    public int maxAmmo;
    private int currentAmmo;
    [HideInInspector] public int CurrentAmmo { get { return currentAmmo; } }
    [SerializeField]
    private int ammoPerShot = 1;
    [SerializeField] private float baseDamage = 1f;
    [SerializeField] private float criticalDamage = 0f;
    [SerializeField] private float range = 100f;
    [SerializeField] private float extendedRange = 200f;
    [SerializeField] private float inaccuracy = 0.01f;
    [SerializeField] private int shotCount = 1;
    [SerializeField] private bool projectile;
    [SerializeField] private Transform firePosition;
    [SerializeField] private bool tracer = false;
    [SerializeField] private GameObject tracerPrefab;
    [SerializeField] private float tracerLifetime = 0.1f;
    [SerializeField] private bool shell = false;
    [SerializeField] private List<Transform> shellPoints = new List<Transform>();
    [SerializeField] private GameObject shellPrefab;
    [SerializeField] private float shellLifetime = 4f;
    [SerializeField] private float shellVelocity = 1f;
    [SerializeField] private GameObject projectilePrefab;
    [SerializeField] private float recoil = 1f;
    [SerializeField] private DamageReceiverScript playerRef;
    private NetworkPlayer playerNetworkRef;
    private Camera playerCamera;
    private bool isInputAvailable;
    public bool IsInputAvailable { get { return isInputAvailable; } }
    private int idleLoopCount = 0;
    WeaponSwapper swapper;

    private void Start()
    {
        UpdateAmmo(maxAmmo);
        isInputAvailable = true;
        playerCamera = playerRef.GetComponentInChildren<Camera>();
        playerNetworkRef = playerRef.NetPlayerRef;
        swapper = GetComponentInParent<WeaponSwapper>();
WeaponSwapper.UpdateUI(swapper.clipAmount,swapper.bagAmount,this);
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
        if (playerNetworkRef.isLocalPlayer)
        {
            if (Input.GetMouseButtonDown(0))
            {
                RequestFire();
            }
            else if (Input.GetKeyDown(KeyCode.R))
            {
                RequestReload();
            }
        }
    }

    void RequestFire()
    {
        if (isInputAvailable)
        {
            if (currentAmmo > 0 || maxAmmo < 0)
            {
                SetAnimatorTrigger("Fired");
                MakeInputUnavailable();
            }
            else
            {
                RequestReload();
            }
        }
    }

    void RequestReload()
    {
        if (isInputAvailable && currentAmmo != maxAmmo)
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

    Vector3 CalculateInaccuracy()
    {
        Vector3 nVec = new Vector3(Random.Range(-inaccuracy, inaccuracy), Random.Range(-inaccuracy, inaccuracy), 0);
        Vector3 tVec = nVec + playerCamera.transform.forward;
        return tVec;
    }

    /*
    public void FireAction(bool _isLocalPlayer)
    {
        if (!_isLocalPlayer)
            FireAction();
    }
    */

    public virtual void FireAction()
    {
        for (int i = 0; i < shotCount; i++)
        {
            RaycastHit hit = new RaycastHit();
            Vector3 inacc = CalculateInaccuracy();
            Ray rayC = new Ray(playerCamera.transform.position, inacc);
            bool prjFired = false;
            bool crit = false;
            if (Physics.Raycast(rayC, out hit, range + extendedRange))
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
                    float dmg = CalculateDamage(Vector3.Distance(hit.point, playerCamera.transform.position), crit);
                    DealDamage(hit.collider.gameObject, dmg);
                }
                if (tracer)
                {
                    SpawnTracer(hit.point);
                }
            }
            else if (tracer)
            {
                SpawnTracer((inacc * (range + extendedRange)) + playerCamera.transform.position);
            }
            if (projectile && !prjFired)
            {
                SpawnProjectile(firePosition.transform.position, Quaternion.LookRotation((inacc * (range + extendedRange) + playerCamera.transform.position) - firePosition.transform.position, Vector3.up), baseDamage);
            }
        }
        //Recoil here: playerRef.ReceiveRecoil(recoil);
        UpdateAmmo(-ammoPerShot);

WeaponSwapper.UpdateUI(swapper.clipAmount,swapper.bagAmount,this);
        //GetComponentInParent<NetworkPlayer>().Shoot();
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
WeaponSwapper.UpdateUI(swapper.clipAmount,swapper.bagAmount,this);
        }
    }

    public void DealDamage(GameObject target, float damageToDeal)
    {
        if (target.GetComponent<DamageReceiverScript>())
        {
            if (FindObjectOfType<GameNetworkManager>())
            {
                playerNetworkRef.ReplicateDamageToPlayer(target.GetComponent<DamageReceiverScript>().NetPlayerRef.netIdentity.netId, damageToDeal);
            }
            else
            {
                target.GetComponent<DamageReceiverScript>().ReceiveDamage(damageToDeal);
            }
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
            dmg = Mathf.Clamp(dmg - ((distance - range) / extendedRange * baseDamage), 1, baseDamage);
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
        GameObject shl = Instantiate(shellPrefab, shellPoints[shellIndex].position, shellPoints[shellIndex].rotation);
        if (shl.GetComponent<Rigidbody>())
        {
            shl.GetComponent<Rigidbody>().velocity = shellPoints[shellIndex].forward * shellVelocity;
        }
        Destroy(shl, shellLifetime);
    }

    public void PlayAudioClip(string clipName)
    {
        print("Imagine the sound '" + clipName + "' is playing right now.");
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
