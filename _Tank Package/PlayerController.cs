using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerController : MonoBehaviour
{
    TankSystem tank;

    public float movementSpeed, turnSpeed;

    public Slider healthBar;

    // Start is called before the first frame update
    void Start()
    {
        tank = GetComponent<TankSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        tank.Move(Input.GetAxis(tank.movementAxisName) * movementSpeed);
        tank.Turn(Input.GetAxis(tank.rotateAxisName) * turnSpeed);

        if (Input.GetButtonDown(tank.shootButton))
        {
            tank.GetReadyToShoot();
        }
        else if (Input.GetButton(tank.shootButton))
        {
            tank.ChargePower();
        }
        else if (Input.GetButtonUp(tank.shootButton))
        {
            tank.Shoot();
        }

        healthBar.value = tank.currentHealth;
    }

    private void OnTriggerEnter(Collider other)
    {
        // Medkit item that heal you
        if (other.CompareTag("Heal"))
        {
            Destroy(other.gameObject);
            tank.GiveEffect(TankSystem.Effect.Heal, 0.5f);
        }
        // Barrier item that make you 90% take less damage
        else if (other.CompareTag("Barrier"))
        {
            Destroy(other.gameObject);
            tank.GiveEffect(TankSystem.Effect.Barrier, 10);
        }
        // Speed item that make you run faster
        else if (other.CompareTag("Speed"))
        {
            Destroy(other.gameObject);
            tank.GiveEffect(TankSystem.Effect.Speed, 10);
        }
        // Bomb, a very surprise item
        else if (other.CompareTag("Dynamite"))
        {
            Destroy(other.gameObject);
            tank.GiveEffect(TankSystem.Effect.Dynamite, 99999999);
        }
    }
}