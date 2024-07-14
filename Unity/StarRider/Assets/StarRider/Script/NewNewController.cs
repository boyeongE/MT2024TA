using Mono.Cecil;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class NewNewController : MonoBehaviour
{
    private InputManager IM;
    private GameObject wheelMeshes, wheelColliders;
    private WheelCollider[] wheels = new WheelCollider[4];
    private GameObject[] wheelMesh = new GameObject[4];
    private GameObject centerOfMass;
    private new Rigidbody rigidbody;

    [Header("Variables")]
    public float driftFrictionMultiplier = 2f;
    public float driftFriction = 0f;
    public float KPH;       // km per hour
    public float brakePower;
    public float downForceValue = 50f;
    public int motorTorque = 200;
    public float steeringMax = 4f;
    public float thrust = 1000f;

    private WheelFrictionCurve forwardFriction, sidewayFriction;
    private float tempo;

    private float maxDriftGauge = 50f;
    private float driftGaugeIncrement = 10f;
    public float driftGauge = 0f;
    public bool isBoosting = false;
    public int boosterNum = 0;
    private float boostTime = 2f;  // booster duration
    private float boostTimer = 0f;  // booster timer

    [Header("Debug")]
    public float[] slip = new float[4];

    void Start()
    {
        GetObjects();
    }

    private void FixedUpdate()
    {
        AddDownForce();
        AnimateWheels();
        MoveKart();
        SteerKart();
        GetFriction();
        AdjustTraction();
        CheckWheelSpin();
        UpadateDriftGauge();
    }

    private void MoveKart()
    {
        for (int i = 0; i < wheels.Length; i++) {
            wheels[i].motorTorque = IM.vertical * motorTorque;
        }

        KPH = rigidbody.velocity.magnitude * 3.6f;

        if (IM.handbrake) {
            for (int i = 0; i < wheels.Length; i++) {
                wheels[i].brakeTorque = 300 * 50f * Time.deltaTime;
            }
        }
        else {
            for (int i = 0; i < wheels.Length; i++) {
                wheels[i].brakeTorque = 0;
            }
        }

        if (isBoosting)
        {
            //rigidbody.AddForce(-Vector3.forward * thrust);
            rigidbody.AddForce(transform.forward * 5000);

            boostTimer += Time.deltaTime;
            if (boostTimer >= boostTime)
            {
                isBoosting = false;
                boostTimer = 0f;
            }
        }
        else if (IM.boosting && boosterNum > 0)
        {
            print("booster!!!!");
            isBoosting = true;
            boosterNum--;
            boostTimer = 0f;
        }
    }

    private void SteerKart()
    {
        for (int i = 0; i < wheels.Length - 2; i++) {
            wheels[i].steerAngle = IM.horizontal * steeringMax;
        }
    }

    private void AnimateWheels()
    {
        Vector3 wheelPosition = Vector3.zero;
        Quaternion wheelRotation = Quaternion.identity;

        for (int i = 0; i < wheels.Length; i++)
        {
            wheels[i].GetWorldPose(out wheelPosition, out wheelRotation);
            wheelMesh[i].transform.position = wheelPosition;
            wheelMesh[i].transform.rotation = wheelRotation;
        }
    }

    private void GetObjects()
    {
        IM = GetComponent<InputManager>();
        rigidbody = GetComponent<Rigidbody>();

        wheelColliders = GameObject.Find("WheelColliders");
        wheels[0] = wheelColliders.transform.Find("FR").gameObject.GetComponent<WheelCollider>();
        wheels[1] = wheelColliders.transform.Find("FL").gameObject.GetComponent<WheelCollider>();
        wheels[2] = wheelColliders.transform.Find("BR").gameObject.GetComponent<WheelCollider>();
        wheels[3] = wheelColliders.transform.Find("BL").gameObject.GetComponent<WheelCollider>();

        wheelMeshes = GameObject.Find("WheelMeshes");
        wheelMesh[0] = wheelMeshes.transform.Find("FR").gameObject;
        wheelMesh[1] = wheelMeshes.transform.Find("FL").gameObject;
        wheelMesh[2] = wheelMeshes.transform.Find("BR").gameObject;
        wheelMesh[3] = wheelMeshes.transform.Find("BL").gameObject;

        centerOfMass = GameObject.Find("CenterOfMass");
        rigidbody.centerOfMass = centerOfMass.transform.localPosition;
    }

    private void AddDownForce()
    {
        rigidbody.AddForce(-transform.up * downForceValue * rigidbody.velocity.magnitude);
    }

    private void GetFriction()
    {
        for (int i = 0; i < wheels.Length; i++)
        {
            WheelHit wheelHit;
            wheels[i].GetGroundHit(out wheelHit);

            slip[i] = wheelHit.sidewaysSlip;
        }
    }

    private void AdjustTraction()
    {
        if (!IM.drifting)
        {
            forwardFriction = wheels[0].forwardFriction;
            sidewayFriction = wheels[0].sidewaysFriction;

            forwardFriction.extremumValue = forwardFriction.asymptoteValue = ((KPH * driftFrictionMultiplier) / 300) + 1;
            sidewayFriction.extremumValue = sidewayFriction.asymptoteValue = ((KPH * driftFrictionMultiplier) / 300) + 1;

            for (int i = 0; i < wheels.Length; i++)
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewayFriction;
            }
        }
        else if (IM.drifting)
        {
            forwardFriction = wheels[0].forwardFriction;
            sidewayFriction = wheels[0].sidewaysFriction;

            float velocity = 0f;
            forwardFriction.extremumValue = forwardFriction.asymptoteValue = Mathf.SmoothDamp(forwardFriction.asymptoteValue, driftFriction, ref velocity, 0.05f * Time.deltaTime);
            sidewayFriction.extremumValue = sidewayFriction.asymptoteValue = Mathf.SmoothDamp(sidewayFriction.asymptoteValue, driftFriction, ref velocity, 0.05f * Time.deltaTime);

            for (int i = 2; i < 4; i++) 
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewayFriction;
            }

            forwardFriction.extremumValue = forwardFriction.asymptoteValue = 1.5f;
            sidewayFriction.extremumValue = sidewayFriction.asymptoteValue = 1.5f;

            for (int i = 0; i < 2; i++) 
            {
                wheels[i].forwardFriction = forwardFriction;
                wheels[i].sidewaysFriction = sidewayFriction;
            }
        }
    }

    private void CheckWheelSpin()
    {
        float blind = 0.28f;

        //if (Input.GetKey(KeyCode.LeftShift))
        //    rigidbody.AddForce(transform.forward * 15000);
        if (IM.drifting)
        {
            for (int i = 0; i < 4; i++) {
                WheelHit wheelHit;
                wheels[i].GetGroundHit(out wheelHit);
                if (wheelHit.sidewaysSlip > blind || wheelHit.sidewaysSlip < -blind) {
                    //applyBooster(wheelHit.sidewaysSlip);
                }
            }
        }

        for (int i = 0; i < 4; i++) {
            WheelHit wheelHit;

            wheels[i].GetGroundHit(out wheelHit);

            if (wheelHit.sidewaysSlip < 0)
            {
                tempo = (1 + -IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip * driftFrictionMultiplier);
                if (tempo < 0.5) tempo = 0.5f;
            }
            if (wheelHit.sidewaysSlip > 0)
            {
                tempo = (1 + IM.horizontal) * Mathf.Abs(wheelHit.sidewaysSlip * driftFrictionMultiplier);
                if (tempo < 0.5) tempo = 0.5f;
            }
            if (wheelHit.sidewaysSlip > .99f || wheelHit.sidewaysSlip < -.99f)
            {
                float velocity = 0f;
                driftFriction = Mathf.SmoothDamp(driftFriction, tempo * 3, ref velocity, 0.1f * Time.deltaTime);
            }
            else
            {
                driftFriction = tempo;
            }
        }
    }

    private void UpadateDriftGauge()
    {
        if (IM.drifting)
        {
            driftGauge += driftGaugeIncrement * Time.deltaTime;

            if (driftGauge >= maxDriftGauge)
            {
                boosterNum++;
                driftGauge = 0f;
            }
        }
    }
}
