using System.Collections;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// Bike balance by steering in Unity
/// </summary>
public class BikeController : MonoBehaviour
{
    public WheelCollider frontCollider;
    public WheelCollider rearCollider;
    public Transform frontImage;
    public Transform rearImage;
    public float maxSteer = 25;
    public float maxLean = 45;
    public bool manualControl;
    [Header("Debug")]
    public float balanceSteer;
    public float currentLean;
    public float safeLean;

    private Wheel wheelFront;
    private Wheel wheelRear;
    private Rigidbody rb;
    private Geometry geometry;
    private float maxTorque;
    
    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Vector3 startingVelocity;
    private Vector3 startingAV;


    void Start()
    {
        Init();
    }
    void Update()
    {
        wheelFront.update();
        wheelRear.update();
    }

    public void Init()
    {
        if (wheelFront != null)
            return;
        wheelFront = new Wheel(frontCollider, frontImage);
        wheelRear = new Wheel(rearCollider, rearImage);

        rb = GetComponent<Rigidbody>();

        frontCollider.GetWorldPose(out Vector3 pos1, out Quaternion rot1);
        rearCollider.GetWorldPose(out Vector3 pos2, out Quaternion rot2);
        float wheelbase = (pos1 - pos2).magnitude;
        float h = rb.centerOfMass.y;
        Vector3 offset = rb.centerOfMass - pos2;
        offset.y = 0;
        geometry = new Geometry(wheelbase, h, offset.magnitude, transform);

        startingPosition = transform.position;
        startingRotation = transform.rotation;
        startingVelocity = rb.velocity;
        startingAV = rb.angularVelocity;

        float maxForce = -(rb.mass / 2 + rearCollider.mass) * Physics.gravity.y * 0.5f; // 0.3
        maxTorque = maxForce * rearCollider.radius;
    }
    public Rigidbody GetRigidbody()
    {
        return rb;
    }
    /// <summary>
    /// The steer angle required to maintain balance at the current speed and the current lean.
    /// </summary>
    /// <returns>degrees</returns>
    public float GetBalanceSteer()
    {
        float lean = getLean();
        float v = transform.InverseTransformVector(rb.velocity).z;
        float steer = geometry.getSteer(lean, v);

        balanceSteer = steer;
        currentLean = lean;

        return steer;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">[-1, 1]</param>
    public void SetAcceleration(float value)
    {
        float force = value * 9.81f * (rb.mass + rearCollider.mass);
        float torque = force * rearCollider.radius * 1.0f;
        torque = Mathf.Clamp(torque, -maxTorque, maxTorque);
        rearCollider.motorTorque = torque;
    }
    /// <summary>
    /// 
    /// </summary>
    /// <param name="value">degrees</param>
    public void SetSteerDirectly(float value)
    {
        frontCollider.steerAngle = value;
    }
    /// <summary>
    /// Brings the bike closer to the desired lean by slightly off balance.
    /// </summary>
    /// <param name="targetLean">degrees</param>
    public void setLean(float targetLean)
    {
        targetLean = Mathf.Clamp(targetLean, -maxLean, maxLean);

        // A safe lean is a lean that does not require too much steering.
        float v = transform.InverseTransformVector(rb.velocity).z;
        safeLean = Mathf.Abs(geometry.getLean(maxSteer * 0.7f, v));
        targetLean = Mathf.Clamp(targetLean, -safeLean, safeLean);

        float diff = targetLean - getLean();
        float newSteer = GetBalanceSteer();

        // Let's deviate a little from the balance position to get closer to the required lean.
        float delta = Mathf.Clamp(diff * 1.0f, -20, 20);
        newSteer += delta;
        
        newSteer += damper() * 2; // We need a damper to keep the bike from swing.

        newSteer = Mathf.Clamp(newSteer, -maxSteer, maxSteer);
        SetSteerDirectly(newSteer);
    }
    /// <summary>
    /// Brings the steer angle closer to the required steer by a small deviation from the balance steer.
    /// </summary>
    /// <param name="targetSteer">degrees</param>
    public void setSteer(float targetSteer)
    {
        targetSteer = Mathf.Clamp(targetSteer, -maxSteer, maxSteer);

        // A safe steer angle is a angle that does not require too much lean.
        float v = transform.InverseTransformVector(rb.velocity).z;
        float safeSteer = Mathf.Abs(geometry.getSteer(maxLean * 0.9f, v));
        targetSteer = Mathf.Clamp(targetSteer, -safeSteer, safeSteer);

        // We need to steer in the opposite direction to the target.
        // This will lean the bike in the direction of the turn.
        float balanceSteer = GetBalanceSteer();
        float diff = targetSteer - balanceSteer;
        float delta = diff * 1.0f;
        float newSteer = balanceSteer - delta;

        newSteer = Mathf.Clamp(newSteer, -maxSteer - 10, maxSteer + 10);
        newSteer += damper();// We need a damper to keep the bike from swing.
        SetSteerDirectly(newSteer);
    }
    private float damper()
    {
        Vector3 av = transform.InverseTransformVector(rb.angularVelocity);
        float damper = -av.z * 0.2f * rb.velocity.magnitude;
        damper = Mathf.Clamp(damper, -10, 10);
        return damper;
    }
    private float getLean()
    {
        float v = transform.localEulerAngles.z;
        if (v > 180)
            v -= 360;
        return v;
    }
    public void reset()
    {
        transform.position = startingPosition;
        transform.rotation = startingRotation;
        rb.velocity = startingVelocity;
        rb.angularVelocity = startingAV;

        frontCollider.steerAngle = 0;
        rearCollider.motorTorque = 0;
    }
    public class Wheel
    {
        private WheelCollider collider;
        private Transform image;
        private Vector3 localHitPoint;
        private TrailRenderer trail;
        public Wheel(WheelCollider collider, Transform image)
        {
            this.collider = collider;
            this.image = image;

            float y = collider.radius + collider.suspensionDistance * collider.suspensionSpring.targetPosition;
            localHitPoint = new Vector3(0, -y, 0);

            trail = collider.GetComponentInChildren<TrailRenderer>();
        }
        public void update()
        {
            collider.GetWorldPose(out Vector3 pos, out Quaternion rot);
            image.position = pos;
            image.rotation = rot;
            if (trail != null)
            {
                if (collider.GetGroundHit(out WheelHit hit))
                {
                    trail.emitting = true;
                    trail.transform.position = hit.point;
                }
                else
                {
                    trail.emitting = false;
                }
            }
        }
        public Vector3 getHitPoint()
        {
            if (collider.GetGroundHit(out WheelHit hit))
                return hit.point;
            else
                return collider.transform.position + collider.transform.TransformVector(localHitPoint);
        }
    }
    public class Geometry
    {
        private float wheelbase;
        private float cmHeight;
        private float cmOffset;
        private Transform bike;
        /// <summary>
        /// 
        /// </summary>
        /// <param name="wheelbase">Wheelbase</param>
        /// <param name="cmHeight">Center of mass height</param>
        /// <param name="cmOffset">Center of mass horizontal offset relative rear wheel</param>
        public Geometry(float wheelbase, float cmHeight, float cmOffset, Transform bike)
        {
            this.wheelbase = wheelbase;
            this.cmHeight = cmHeight;
            this.cmOffset = cmOffset;

            this.bike = bike;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="lean">Angle of lean</param>
        /// <param name="v">Linear velocity.</param>
        /// <returns></returns>
        public float getSteer(float lean, float v)
        {
            if (Mathf.Abs(lean) < 0.000001f)
                return 0;
            lean *= Mathf.Deg2Rad;
            // first we calculate the acceleration needed to compensate for the lean of the bike
            float a = Physics.gravity.y * Mathf.Tan(lean);
            float r = v * v / a;   // required radius of rotation of the center of mass

            float d = Mathf.Abs(cmHeight * Mathf.Sin(lean)); // Horizontal displacement of the center of mass.
            float dR2 = r * r - cmOffset * cmOffset;
            if (dR2 <= 0)
                return 30 * Mathf.Sign(-lean);
            float R = d + Mathf.Sqrt(r * r - cmOffset * cmOffset); // rear wheel radius of rotation
            float steer = Mathf.Atan2(wheelbase, R) * Mathf.Rad2Deg * Mathf.Sign(-lean);

            //draw(steer, R);
            return steer;
        }
        public float getLean(float steer, float v)
        {
            if (Mathf.Abs(steer) < 0.000001f)
                return 0;

            // first iteration
            // As a result of the tilt of the bike, the center of mass shifts slightly
            // towards the direction of the turn. This leads to a decrease in the radius of rotation.
            // In the first iteration, we do not take into account the radius reduction.
            steer *= Mathf.Deg2Rad;
            float R = Mathf.Abs(wheelbase / Mathf.Tan(steer));
            float r = Mathf.Sqrt(R * R + cmOffset * cmOffset);
            float a = v * v / r;

            // second iteration
            // Now we can approximately calculate the horizontal displacement of the center of mass.
            float g = Physics.gravity.y;
            float d = cmHeight * a / Mathf.Sqrt(g * g + a * a);
            r = Mathf.Sqrt((R - d) * (R - d) + cmOffset * cmOffset);
            a = v * v / r;
            float lean = Mathf.Atan2(a, -Physics.gravity.y) * Mathf.Sign(-steer) * Mathf.Rad2Deg;

            return lean;
        }
        private void draw(float steer, float R)
        {
            Vector3 frontPos = new Vector3(0, 0, cmOffset);
            Vector3 rearPos = new Vector3(0, 0, -cmOffset);
            Vector3 center = new Vector3(R, 0, -cmOffset) * Mathf.Sign(steer);

            Vector3 from = bike.TransformPoint(center);
            Vector3 to1 = bike.TransformPoint(frontPos);
            Vector3 to2 = bike.TransformPoint(rearPos);
            from.y = 0;

            Debug.DrawLine(from, to1);
            Debug.DrawLine(from, to2);
        }
    }
}
