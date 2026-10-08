/*
    TrainingDrone.cs
    - Handles the training drones in the TrainingEnvironment scene
    Contributor(s): Jake Schott
    Last Updated: 10/4/2026
*/

using Unity.Netcode;
using UnityEngine;
using System.Collections;

public class TrainingDrone : NetworkBehaviour, IDamageable, IPhaserTargetable, ITorpedoTargetable
{
    //CLASS CONSTANTS
    private static float DETECTION_RANGE = 500.0f;
    private static float MOVEMENT_RANGE = 250.0f; //how far the drone will try to get to the ship before stopping movement
    private static float MOVEMENT_ANGLE = 5.0f; //degree difference between look vector and approach vector before it will start moving towards ship
    private static float ROTATION_SPEED = 0.5f;
    private static float MOVE_SPEED = 7.5f;
    private static Color EXPLOSION_COLOR = Color.red;
    private static float FIRE_TIME = 1.0f;
    private static float PHASER_DAMAGE = 5.0f; //how much damage each phaser hit does to ship
    private static float PHASER_COOLDOWN_TIME = 1.0f; //time in seconds between phaser fires

    public GameObject drone_flashing_indicators;
    public GameObject drone_phaser_source;
    public Transform drone_phaser_origin;
    public AudioSource drone_phaser_sound;
    public LineRenderer drone_line_renderer;

    private Transform target_ship;
    private GameObject current_target;
    private bool permanently_disabled = false;
    private float health = 15.0f;
    private Coroutine phaser_fire_coroutine = null;
    private Coroutine drone_flash_coroutine = null;

    public void initializeDrone()
    {
        target_ship = ReferenceAssistor.Instance.spaceship.transform;
        StartCoroutine(droneController());
        drone_flash_coroutine = StartCoroutine(droneFlash(ReferenceAssistor.Instance.lit_red, 1.0f, false));
    }

    private void lookAndMoveTowardsShip(float distance_to_ship)
    {
        //determine direction to ship
        Vector3 target_direction = (target_ship.position - transform.position).normalized;

        //face the ship over time
        Quaternion target_rotation = Quaternion.LookRotation(target_direction);
        transform.rotation = Quaternion.Slerp(transform.rotation, target_rotation, Time.deltaTime * ROTATION_SPEED);

        //move towards the ship if above movement range
        if (distance_to_ship > MOVEMENT_RANGE && Quaternion.Angle(transform.rotation, target_rotation) < MOVEMENT_ANGLE)
        {
            transform.position = Vector3.MoveTowards(transform.position, target_ship.position, Time.deltaTime * MOVE_SPEED);
        }
    }

    IEnumerator droneController()
    {
        while (target_ship != null && permanently_disabled == false)
        {
            float distance_to_ship = Vector3.Distance(transform.position, target_ship.position);

            if (phaser_fire_coroutine == null && distance_to_ship <= DETECTION_RANGE)
            {
                //check for collision point (and if hit something that isn't our target on the way there, then set the target to null to stop damage)
                if (Physics.Raycast(new Ray(drone_phaser_origin.transform.position, drone_phaser_origin.transform.forward), out RaycastHit hit, Minefield.DETECTION_RANGE, LayerMask.GetMask("ShipColliders")))
                {
                    Vector3 beam_end = hit.point;
                    current_target = hit.collider.gameObject;
                    //check if hit ship
                    if (current_target.GetComponent<ShipCollider>() != null)
                    {
                        firePhaserRPC(beam_end);
                    }
                }
            }

            if (distance_to_ship <= DETECTION_RANGE)
            {
                lookAndMoveTowardsShip(distance_to_ship);
            }

            yield return null;
        }
    }

    IEnumerator phaserFire(Vector3 beam_end)
    {
        drone_line_renderer.enabled = true;

        //give warning
        ReferenceAssistor.Instance.module_handlers[1].GetComponent<ThreatDetectors>().adjustPhaserWarningTime(FIRE_TIME + 1.2f);

        //slight delay
        yield return new WaitForSeconds(0.5f);

        //apply damage
        current_target.GetComponent<ShipCollider>().damage(PHASER_DAMAGE, IDamageable.DamageType.EnemyPhaser);

        drone_line_renderer.SetPosition(1, beam_end);
        drone_phaser_sound.pitch = 1.25f;
        drone_phaser_sound.Play();

        //play animation
        float activeTime = FIRE_TIME;
        float activeHalftime = activeTime * 0.5f;
        float timeRemaining = activeTime;
        while (timeRemaining > 0.0f)
        {
            timeRemaining = Mathf.Max(0.0f, timeRemaining - Time.deltaTime);

            float beamWidth = Mathf.Lerp(0.0f, 2.5f, Mathf.Lerp(0.0f, 1.0f, Mathf.PingPong(timeRemaining, activeHalftime) / activeHalftime));
            drone_line_renderer.startWidth = beamWidth;
            drone_line_renderer.endWidth = beamWidth;
            drone_line_renderer.SetPosition(0, drone_phaser_origin.transform.position);

            yield return null;
        }

        //disable phaser
        drone_line_renderer.enabled = false;

        //cooldown
        yield return new WaitForSeconds(PHASER_COOLDOWN_TIME);

        phaser_fire_coroutine = null;
    }

    IEnumerator droneFlash(Material on_material, float flash_interval, bool include_phaser_source)
    {
        while (true)
        {
            drone_flashing_indicators.GetComponent<Renderer>().material = ReferenceAssistor.Instance.pure_black;
            if (include_phaser_source == true)
            {
                drone_phaser_source.GetComponent<Renderer>().material = ReferenceAssistor.Instance.pure_black;
            }
            yield return new WaitForSeconds(flash_interval);

            drone_flashing_indicators.GetComponent<Renderer>().material = on_material;
            if (include_phaser_source == true)
            {
                drone_phaser_source.GetComponent<Renderer>().material = on_material;
            }
            yield return new WaitForSeconds(flash_interval);
        }
    }

    public void damage(float damage, IDamageable.DamageType damage_type)
    {
        //make sure not already destroyed
        if (GetComponent<NetworkObject>() == null || GetComponent<NetworkObject>().IsSpawned == false)
        {
            return;
        }

        //if hit by ion torpedo, permanently disable the mine 
        if (damage_type == IDamageable.DamageType.IonTorpedo)
        {
            if (permanently_disabled == false)
            {
                permanentlyDisableRPC();
            }
            return;
        }

        //else, damage the mine
        if (health > 0.0f)
        {
            health -= damage;
            if (health <= 0.0f)
            {
                if (GetComponent<NetworkObject>() != null && GetComponent<NetworkObject>().IsSpawned == true)
                {
                    GetComponent<NetworkObject>().Despawn(true);
                }
                ReferenceAssistor.Instance.effects_handler.createExplosion(transform.position, 25.0f, false, EXPLOSION_COLOR);
                Destroy(this);
            }
        }
    }

    public bool getPhaserTargetable(IDamageable.DamageType damage_type)
    {
        return true;
    }

    public bool getTorpedoTargetable(IDamageable.DamageType damage_type)
    {
        return true;
    }


    [Rpc(SendTo.Everyone)]
    private void firePhaserRPC(Vector3 beam_end)
    {
        if (phaser_fire_coroutine != null)
        {
            StopCoroutine(phaser_fire_coroutine);
        }

        phaser_fire_coroutine = StartCoroutine(phaserFire(beam_end));
    }

    [Rpc(SendTo.Everyone)]
    private void permanentlyDisableRPC()
    {
        permanently_disabled = true;
        if (drone_flash_coroutine != null)
        {
            StopCoroutine(drone_flash_coroutine);
        }
        if (phaser_fire_coroutine != null)
        {
            StopCoroutine(phaser_fire_coroutine);
            phaser_fire_coroutine = null;
            drone_phaser_origin.gameObject.SetActive(false);
        }
        drone_flash_coroutine = StartCoroutine(droneFlash(ReferenceAssistor.Instance.lit_green, 0.2f, true));
        health = 0.1f;
    }
}