using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class WeaponControllerOff : NetworkBehaviour
{
    Camera mainCamera;
    public float rotationSpeed;
    public float baseRotation;

    public NetworkVariable<bool> someoneIsShooting;
    public Transform vehicle;

    public GameObject bulletPrefab;
    public float bulletForce;
    public Transform firePoint;
    public bool canShoot;
    public float shootDelay;

    private void Start()
    {
        mainCamera = Camera.main;
        someoneIsShooting.Value = false;
        vehicle = transform.parent;
        canShoot = true;
    }

    private void Update()
    {
        if (someoneIsShooting.Value)
        {
            Aim();

            if (Input.GetButton("Fire1") && canShoot)
            {
                Shoot();
            }
        }
    }

    public void Aim()
    {
        var mouseScreenPos = Input.mousePosition;
        var startingScreenPos = mainCamera.WorldToScreenPoint(transform.position);
        mouseScreenPos.x -= startingScreenPos.x;
        mouseScreenPos.y -= startingScreenPos.y;

        float angle = Mathf.Atan2(mouseScreenPos.y, mouseScreenPos.x) * Mathf.Rad2Deg;
        //angle = Mathf.Clamp(angle, baseRotation - 60f, baseRotation + 60f);

        float rotationStep = rotationSpeed * Time.deltaTime;

        AimServerRpc(angle, rotationStep);
    }

    [ServerRpc(RequireOwnership = false)]
    public void AimServerRpc(float angle, float rotationStep)
    {
        transform.rotation = Quaternion.RotateTowards(transform.rotation, Quaternion.Euler(0, 0, angle), rotationStep);
    }

    public void Shoot()
    {
        canShoot = false;
        StartCoroutine(ShootCoroutine());
        ShootServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ShootServerRpc()
    {
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
        bullet.GetComponent<Bullet>().shooter = LevelManager.vehicle;
        bullet.GetComponent<NetworkObject>().Spawn(true);
        Rigidbody2D rb = bullet.GetComponent<Rigidbody2D>();
        rb.AddForce(firePoint.right * bulletForce, ForceMode2D.Impulse);
        
    }

    IEnumerator ShootCoroutine()
    {
        yield return new WaitForSeconds(shootDelay);
        canShoot = true;
    }

}

