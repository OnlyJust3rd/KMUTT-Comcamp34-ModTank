using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class TankPlayer : MonoBehaviour
{
    // For referencing TankSystem script component
    TankSystem tank;

    // Use for movement speed and turning speed
    public float movementSpeed = 12;
    public float turnSpeed = 180;

    // UI to show tank's HP to user
    public Slider hpBar;

    // Start is called before the first frame update
    void Start()
    {
        // Assign TankSystem script component to tank variable
        tank = GetComponent<TankSystem>();
    }

    // Update is called once per frame
    void Update()
    {
        /*
         * Tank movement system
         * using tank.Move() to move forward and backward
         * take a movement speed (float) as parameter
         * 
         * Input.GetAxis(tank.movementAxisName) is direction input from player
         * It becomes 1 when "W" key is pressed. -1 when "S" key is pressed.
         * 0 when both key are not pressed.
         * 
         */
        tank.Move(Input.GetAxis(tank.movementAxisName) * movementSpeed);
        /*
         * Tank turning system
         * using tank.Turn() to turn right and left
         * take a turning speed (float) as parameter
         * 
         * Input.GetAxis(tank.rotateAxisName) is direction input from player
         * It becomes 1 when "D" key is pressed. -1 when "A" key is pressed.
         * 0 when both key are not pressed.
         * 
         */
        tank.Turn(Input.GetAxis(tank.rotateAxisName) * turnSpeed);

        // Shooting system
        // Get ready to shoot / mark starting point when player start press button
        if (Input.GetButtonDown(tank.shootButton))
        {
            tank.GetReadyToShoot();
        }
        // Charge power to shoot compared to starting point when player hold button
        if (Input.GetButton(tank.shootButton))
        {
            tank.ChargePower();
        }
        // Release bullet with charged power when player release button
        if (Input.GetButtonUp(tank.shootButton))
        {
            tank.Shoot();
        }

        // User Interface (UI) system. Shows tank's hp to user
        hpBar.value = tank.currentHealth;
    }

    // Feature that missing from lecture because we don't have enough time to learn this
    // this is Item feature you can pick item with this code
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