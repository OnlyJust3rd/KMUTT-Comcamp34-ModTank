using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TankSystem : MonoBehaviour
{
    [Header("Tank Movement")]
    public int m_PlayerNumber = 1;              // Used to identify which tank belongs to which player.  This is set by this tank's manager.
    public float m_Speed = 12f;                 // How fast the tank moves forward and back.
    public float m_TurnSpeed = 180f;            // How fast the tank turns in degrees per second.
    public AudioSource m_MovementAudio;         // Reference to the audio source used to play engine sounds. NB: different to the shooting audio source.
    public AudioClip m_EngineIdling;            // Audio to play when the tank isn't moving.
    public AudioClip m_EngineDriving;           // Audio to play when the tank is moving.
    public float m_PitchRange = 0.2f;           // The amount by which the pitch of the engine noises can vary.

    [HideInInspector]
    public string movementAxisName;          // The name of the input axis for moving forward and back.
    [HideInInspector]
    public string rotateAxisName;              // The name of the input axis for turning.
    private Rigidbody m_Rigidbody;              // Reference used to move the tank.
    private float m_MovementInputValue;         // The current value of the movement input.
    private float m_TurnInputValue;             // The current value of the turn input.
    private float m_OriginalPitch;              // The pitch of the audio source at the start of the scene.
    private ParticleSystem[] m_particleSystems; // References to all the particles systems used by the Tanks

    private void Awake()
    {
        m_Rigidbody = GetComponent<Rigidbody>();

        // Instantiate the explosion prefab and get a reference to the particle system on it.
        m_ExplosionParticles = Instantiate(m_ExplosionPrefab).GetComponent<ParticleSystem>();

        // Get a reference to the audio source on the instantiated prefab.
        m_ExplosionAudio = m_ExplosionParticles.GetComponent<AudioSource>();

        // Disable the prefab so it can be activated when it's required.
        m_ExplosionParticles.gameObject.SetActive(false);
    }


    private void OnEnable()
    {
        // When the tank is turned on, make sure it's not kinematic.
        m_Rigidbody.isKinematic = false;

        if (GetComponent<PlayerController>()) GetComponent<PlayerController>().enabled = true;
        if (GetComponent<TankPlayer>()) GetComponent<TankPlayer>().enabled = true;

        // Also reset the input values.
        m_MovementInputValue = 0f;
        m_TurnInputValue = 0f;

        // We grab all the Particle systems child of that Tank to be able to Stop/Play them on Deactivate/Activate
        // It is needed because we move the Tank when spawning it, and if the Particle System is playing while we do that
        // it "think" it move from (0,0,0) to the spawn point, creating a huge trail of smoke
        m_particleSystems = GetComponentsInChildren<ParticleSystem>();
        for (int i = 0; i < m_particleSystems.Length; ++i)
        {
            m_particleSystems[i].Play();
        }

        // When the tank is enabled, reset the tank's health and whether or not it's dead.
        currentHealth = m_StartingHealth;
        m_Dead = false;

        // Update the health slider's value and color.
        SetHealthUI();

        // When the tank is turned on, reset the launch force and the UI
        m_CurrentLaunchForce = m_MinLaunchForce;
        m_AimSlider.value = m_MinLaunchForce;
    }


    private void OnDisable()
    {
        if (GetComponent<PlayerController>()) GetComponent<PlayerController>().enabled = false;
        if (GetComponent<TankPlayer>()) GetComponent<TankPlayer>().enabled = false;

        resistEffect.SetActive(false);
        speedEffect.SetActive(false);

        // When the tank is turned off, set it to kinematic so it stops moving.
        m_Rigidbody.isKinematic = true;

        // Stop all particle system so it "reset" it's position to the actual one instead of thinking we moved when spawning
        for (int i = 0; i < m_particleSystems.Length; ++i)
        {
            m_particleSystems[i].Stop();
        }
    }


    private void Start()
    {
        // The axes names are based on player number.
        movementAxisName = "Vertical" + m_PlayerNumber;
        rotateAxisName = "Horizontal" + m_PlayerNumber;

        // Store the original pitch of the audio source.
        m_OriginalPitch = m_MovementAudio.pitch;

        // The fire axis is based on the player number.
        shootButton = "Fire" + m_PlayerNumber;

        // The rate that the launch force charges up is the range of possible forces by the max charge time.
        m_ChargeSpeed = (m_MaxLaunchForce - m_MinLaunchForce) / m_MaxChargeTime;
    }


    private void Update()
    {
        // ShootControl();
        m_AimSlider.value = m_MinLaunchForce;

        // Store the value of both input axes.
        m_MovementInputValue = Input.GetAxis(movementAxisName);
        m_TurnInputValue = Input.GetAxis(rotateAxisName);

        m_FillImage.color = SetHealthUI();

        EngineAudio();
    }


    private void EngineAudio()
    {
        // If there is no input (the tank is stationary)...
        if (Mathf.Abs(m_MovementInputValue) < 0.1f && Mathf.Abs(m_TurnInputValue) < 0.1f)
        {
            // ... and if the audio source is currently playing the driving clip...
            if (m_MovementAudio.clip == m_EngineDriving)
            {
                // ... change the clip to idling and play it.
                m_MovementAudio.clip = m_EngineIdling;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
        else
        {
            // Otherwise if the tank is moving and if the idling clip is currently playing...
            if (m_MovementAudio.clip == m_EngineIdling)
            {
                // ... change the clip to driving and play.
                m_MovementAudio.clip = m_EngineDriving;
                m_MovementAudio.pitch = Random.Range(m_OriginalPitch - m_PitchRange, m_OriginalPitch + m_PitchRange);
                m_MovementAudio.Play();
            }
        }
    }


    private void FixedUpdate()
    {
        // Adjust the rigidbodies position and orientation in FixedUpdate.
        //Move(m_MovementInputValue * m_Speed);
        //Turn(m_TurnSpeed * m_TurnInputValue);
    }


    public void Move(float moveSpeed)
    {
        // Create a vector in the direction the tank is facing with a magnitude based on the input, speed and the time between frames.
        Vector3 movement = transform.forward * moveSpeed * Time.deltaTime * speedFactor;

        // Apply this movement to the rigidbody's position.
        m_Rigidbody.MovePosition(m_Rigidbody.position + movement);
    }


    public void Turn(float turnSpeed)
    {
        // Determine the number of degrees to be turned based on the input, speed and time between frames.
        float turn = turnSpeed * Time.deltaTime * speedFactor;

        // Make this into a rotation in the y axis.
        Quaternion turnRotation = Quaternion.Euler(0f, turn, 0f);

        // Apply this rotation to the rigidbody's rotation.
        m_Rigidbody.MoveRotation(m_Rigidbody.rotation * turnRotation);
    }

    // ##########################################################################################################################################################################################################################################

    [Header("Tank Health")]
    public float m_StartingHealth = 100f;               // The amount of health each tank starts with.
    public Slider m_Slider;                             // The slider to represent how much health the tank currently has.
    public Image m_FillImage;                           // The image component of the slider.
    public Color m_FullHealthColor = Color.green;       // The color the health bar will be when on full health.
    public Color m_ZeroHealthColor = Color.red;         // The color the health bar will be when on no health.
    public GameObject m_ExplosionPrefab;                // A prefab that will be instantiated in Awake, then used whenever the tank dies.


    private AudioSource m_ExplosionAudio;               // The audio source to play when the tank explodes.
    private ParticleSystem m_ExplosionParticles;        // The particle system the will play when the tank is destroyed.
    [HideInInspector]
    public float currentHealth;                      // How much health the tank currently has.
    private bool m_Dead;                                // Has the tank been reduced beyond zero health yet?

    public void TakeDamage(float amount)
    {
        // Reduce current health by the amount of damage done.
        currentHealth -= amount * resistFactor;

        // Change the UI elements appropriately.
        // SetHealthUI();

        // If the current health is at or below zero and it has not yet been registered, call OnDeath.
        if (currentHealth <= 0f && !m_Dead)
        {
            OnDeath();
        }
    }


    private Color SetHealthUI()
    {
        // Set the slider's value appropriately.
        // m_Slider.value = m_CurrentHealth;

        // Interpolate the color of the bar between the choosen colours based on the current percentage of the starting health.
        // m_FillImage.color = Color.Lerp(m_ZeroHealthColor, m_FullHealthColor, m_CurrentHealth / m_StartingHealth);

        if (currentHealth > 60) return Color.green;
        else if (currentHealth > 30) return Color.yellow;
        else return Color.red;
    }


    private void OnDeath()
    {
        // Set the flag so that this function is only called once.
        m_Dead = true;

        // Move the instantiated explosion prefab to the tank's position and turn it on.
        m_ExplosionParticles.transform.position = transform.position;
        m_ExplosionParticles.gameObject.SetActive(true);

        // Play the particle system of the tank exploding.
        m_ExplosionParticles.Play();

        // Play the tank explosion sound effect.
        m_ExplosionAudio.Play();

        // Turn the tank off.
        gameObject.SetActive(false);
    }

    // ##########################################################################################################################################################################################################################################

    [Header("Tank Shooting")]
    // public int m_PlayerNumber = 1;              // Used to identify the different players.
    public Rigidbody m_Shell;                   // Prefab of the shell.
    public Transform m_FireTransform;           // A child of the tank where the shells are spawned.
    public Slider m_AimSlider;                  // A child of the tank that displays the current launch force.
    public AudioSource m_ShootingAudio;         // Reference to the audio source used to play the shooting audio. NB: different to the movement audio source.
    public AudioClip m_ChargingClip;            // Audio that plays when each shot is charging up.
    public AudioClip m_FireClip;                // Audio that plays when each shot is fired.
    public float m_MinLaunchForce = 15f;        // The force given to the shell if the fire button is not held.
    public float m_MaxLaunchForce = 30f;        // The force given to the shell if the fire button is held for the max charge time.
    public float m_MaxChargeTime = 0.75f;       // How long the shell can charge for before it is fired at max force.

    [HideInInspector]
    public string shootButton;                // The input axis that is used for launching shells.
    private float m_CurrentLaunchForce;         // The force that will be given to the shell when the fire button is released.
    private float m_ChargeSpeed;                // How fast the launch force increases, based on the max charge time.
    [HideInInspector]
    public bool isReadyToFire;                       // Whether or not the shell has been launched with this button press.

    //public void ShootControl()
    //{
    //    // The slider should have a default value of the minimum launch force.
        

    //    // If the max force has been exceeded and the shell hasn't yet been launched...
    //    //if (m_CurrentLaunchForce >= m_MaxLaunchForce && !m_Fired)
    //    //{
    //    //    // ... use the max force and launch the shell.
    //    //    m_CurrentLaunchForce = m_MaxLaunchForce;
    //    //    Fire();
    //    //}
    //    // Otherwise, if the fire button has just started being pressed...
    //    if (Input.GetButtonDown(m_FireButton))
    //    {

    //    }
    //    // Otherwise, if the fire button is being held and the shell hasn't been launched yet...
    //    else if (Input.GetButton(m_FireButton) && !m_Fired)
    //    {

    //    }
    //    // Otherwise, if the fire button is released and the shell hasn't been launched yet...
    //    else if (Input.GetButtonUp(m_FireButton) && !m_Fired)
    //    {
    //        // ... launch the shell.
    //        Fire();
    //    }
    //}

    public void GetReadyToShoot()
    {
        // ... reset the fired flag and reset the launch force.
        isReadyToFire = true;
        m_CurrentLaunchForce = m_MinLaunchForce;

        // Change the clip to the charging clip and start it playing.
        m_ShootingAudio.clip = m_ChargingClip;
        m_ShootingAudio.Play();
    }

    public void ChargePower()
    {
        if (!isReadyToFire) return;

        // Increment the launch force and update the slider.
        if (m_CurrentLaunchForce < m_MaxLaunchForce) m_CurrentLaunchForce += m_ChargeSpeed * Time.deltaTime;

        m_AimSlider.value = m_CurrentLaunchForce;
    }


    public void Shoot()
    {
        if (!isReadyToFire) return;

        // Set the fired flag so only Fire is only called once.
        isReadyToFire = false;

        // Create an instance of the shell and store a reference to it's rigidbody.
        Rigidbody shellInstance =
            Instantiate(m_Shell, m_FireTransform.position, m_FireTransform.rotation) as Rigidbody;

        // Set the shell's velocity to the launch force in the fire position's forward direction.
        shellInstance.velocity = m_CurrentLaunchForce * m_FireTransform.forward;

        // Change the clip to the firing clip and play it.
        m_ShootingAudio.clip = m_FireClip;
        m_ShootingAudio.Play();

        // Reset the launch force.  This is a precaution in case of missing button events.
        m_CurrentLaunchForce = m_MinLaunchForce;
    }

    public enum Effect
    {
        Heal,
        Speed,
        Barrier,
        Dynamite
    }

    public void GiveEffect(Effect effectToGive, float power)
    {
        if(effectToGive == Effect.Heal)
        {
            if (currentHealth < m_StartingHealth) currentHealth += m_StartingHealth * power;

            if (currentHealth > m_StartingHealth) currentHealth = m_StartingHealth;
        }
        else if (effectToGive == Effect.Speed)
        {
            StartCoroutine(SpeedBuff(power));
        }
        else if (effectToGive == Effect.Barrier)
        {
            StartCoroutine(Resistant(power));
        }
        else if (effectToGive == Effect.Dynamite)
        {
            CommitSudoku(power);
        }
    }

    private float speedFactor = 1, resistFactor = 1;
    public GameObject speedEffect, resistEffect;
    public GameObject tankModel;

    private IEnumerator SpeedBuff(float duration)
    {
        speedFactor = 1.4f;
        speedEffect.SetActive(true);

        yield return new WaitForSeconds(duration);

        speedFactor = 1;
        speedEffect.SetActive(false);
    }

    private IEnumerator Resistant(float duration)
    {
        resistFactor = .1f;
        resistEffect.SetActive(true);

        yield return new WaitForSeconds(duration);

        resistFactor = 1;
        resistEffect.SetActive(false);
    }

    private void CommitSudoku(float dmg)
    {
        TakeDamage(dmg);
    }
}
