using System.Collections;
using System.Collections.Generic;
//using System.IO.
using UnityEngine;
using UnityEngine.UI;

public class MouseSteer : MonoBehaviour
{
    public BikeController bike;
    public Text textFPS;
    [Range(0.1f, 1f)]
    public float timeScale;
    public bool useMouse;
    [Tooltip("If true, steer control, otherwise lean control.")]
    public bool useSteer;
    [Header("Steer")]
    [Range(-30, 30)]
    public float steer;
    public float currentSteer;
    [Header("Lean")]
    [Range(-45, 45)]
    public float targetLean;
    public float currentLean;
    [Tooltip("m/s")]
    [Header("Velocity")]
    [Range(0, 40)]
    public float velocity;
    public float currentVelocity;
    [Header("Slip")]
    public float forward0;
    public float forward1;
    public float sideways0;
    public float sideways1;

    private float maxSteer;
    private Rigidbody rb;
    private float prevPos;
    private float maxLean = 15;
    private int fps;
    private float fpsTime;

    private float startingSteer;
    private float startingLean;
    private float startingVelocity;
    void Start()
    {
        //QualitySettings.vSyncCount = 0;
        //Application.targetFrameRate = 60;

        bike.Init();
        rb = bike.GetRigidbody();
        rb.inertiaTensor = new Vector3(34, 27.25f, 11.25f);
        maxSteer = bike.maxSteer;
        prevPos = Input.mousePosition.y;
        fpsTime = Time.realtimeSinceStartup;

        startingSteer = steer;
        startingLean = targetLean;
        startingVelocity = velocity;
    }
    private void FixedUpdate()
    {
        Time.timeScale = timeScale;
        setVelo();
        if (useSteer)
            setSteer();
        else
            setLean();

        currentLean = getLean();
        currentSteer = bike.frontCollider.steerAngle;
        currentVelocity = rb.velocity.magnitude;

        if (Input.GetKey(KeyCode.R))
        {
            steer = startingSteer;
            targetLean = startingLean;
            velocity = startingVelocity;
            bike.reset();
        }
    }
    void Update()
    {
        if (Input.GetKey(KeyCode.P))
            Debug.Break();
        if (Input.GetKey(KeyCode.Space))
        {
            steer = 0;
            targetLean = 0;
        }
        bike.frontCollider.GetGroundHit(out WheelHit hit0);
        bike.rearCollider.GetGroundHit(out WheelHit hit1);
        forward0 = hit0.forwardSlip;
        forward1 = hit1.forwardSlip;
        sideways0 = hit0.sidewaysSlip;
        sideways1 = hit1.sidewaysSlip;

        fps++;
        if (Time.realtimeSinceStartup - fpsTime > 1)
        {
            textFPS.text = "FPS " + fps;
            fps = 0;
            fpsTime = Time.realtimeSinceStartup;
        }
    }
    private void setSteer()
    {
        if (useMouse)
            steer += Input.GetAxis("Mouse X") * 0.53f;
        steer += Input.GetAxis("Horizontal");
        steer = Mathf.Clamp(steer, -bike.maxSteer, bike.maxSteer);
        bike.setSteer(steer);
    }
    private void setLean()
    {
        if (useMouse)
            targetLean -= Input.GetAxis("Mouse X") * 3.0f;
        targetLean -= Input.GetAxis("Horizontal");
        targetLean = Mathf.Clamp(targetLean, -bike.maxLean, bike.maxLean);
        bike.setLean(targetLean);
    }
    private void setVelo()
    {
        velocity += Input.GetAxis("Vertical");
        if (useMouse)
            velocity += Input.mouseScrollDelta.y;
        velocity = Mathf.Clamp(velocity, 0, 40);

        Vector3 localV = transform.InverseTransformVector(rb.velocity);
        float diff = velocity - localV.z;
        float a = Mathf.Clamp(diff * 0.1f, -1, 1);
        bike.SetAcceleration(a);
    }
    private float getLean()
    {
        float v = transform.localEulerAngles.z;
        if (v > 180)
            v -= 360;
        return v;
    }
}
