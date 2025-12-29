using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class SwingController : MonoBehaviour
{
    [Header("Manual Setup")]
    public Rigidbody rb;           // the bob's rigidbody
    public HingeJoint hinge;       // hinge joint on the bob
    public Slider slider;          // slider (0..maxPush)
    public TextMeshProUGUI sliderText;
    public SwingController otherSwing;
    public StateManager3 stateManager;

    [Header("Manual Values")]
    public float L; // rope length
    public bool IsSynced = false;

    [Header("Tuning")]
    public float tweakFactor = 1f;      // for slider value
    public float deviationDeg = 8f;      // how close to vertical to count as "bottom" (= 0)
    public float minAngularVelocity = 0f;
    public float pushDuration = 0.06f;     // apply force over this many fixed updates
    public float cooldown = 0.15f;      // min time between pushes
    public float maxAngularVelocity = 50f;

    [Header("Debug")]
    public float angVel;
    public Vector3 hingeAxisWorld;
    public bool isVelocitySynced;

    // internal
    private float pushTimer = 0f;
    private float cooldownTimer = 0f;
    private float prevAngle = 0f;
    private int framesLeftToPush = 0;
    private float pushSign = 1f;
    private float m;

    void Start()
    {
        if (rb == null) rb = GetComponent<Rigidbody>();
        if (hinge == null) hinge = GetComponent<HingeJoint>();

        if (slider != null)
            slider.onValueChanged.AddListener((v) =>
            {
                sliderText.text = v.ToString("0") + " N";
                StartCoroutine(CheckForces());
                otherSwing.StartCoroutine(otherSwing.CheckForces());
            });

        prevAngle = hinge.angle;

        rb.angularVelocity = hinge.transform.TransformDirection(hinge.axis) * 0.6f; // small initial speed

        m = rb.mass;
    }

    void FixedUpdate()
    {
        float angle = hinge.angle;       // degrees
        angVel = hinge.velocity;   // degrees/sec
        // when its swinging forward: +80 , when swinging backwards: -80 , solution: absolute value
        float absAngle = Mathf.Abs(angle); // absolute value of degree

        // starts counting down
        cooldownTimer -= Time.fixedDeltaTime;
        if (cooldownTimer < 0f) cooldownTimer = 0f;

        // true if the deviation is in the trigger zone (so 8) for a push
        bool insideDeviation = absAngle <= deviationDeg;
        // check if angle passed the bottom: if sign changed and if previous and current angle are in the deviaton window
        bool crossedThrough = Mathf.Sign(prevAngle) != Mathf.Sign(angle) || (Mathf.Abs(prevAngle) < deviationDeg && Mathf.Abs(angle) < deviationDeg);
        // nmove even at rest
        bool IsMoving = Mathf.Abs(angVel) >= minAngularVelocity || slider.value > 0f;

        // trigger push when inside deviation window, moving and cooldown expired
        if (insideDeviation && IsMoving && cooldownTimer == 0f && crossedThrough && Mathf.Abs(angVel) < maxAngularVelocity)
        {
            // decide push direction: same direction as current motion (helps the swing)
            pushSign = Mathf.Sign(angVel);
            // schedule a short-duration push (apply over a few fixed updates for smoothness)
            framesLeftToPush = Mathf.Max(1, Mathf.RoundToInt(pushDuration / Time.fixedDeltaTime));
            cooldownTimer = cooldown; // reset to default value of cooldown
        }

        // apply scheduled pushes
        if (framesLeftToPush > 0)
        {
            // compute torque magnitude from slider
            float sliderValue = slider != null ? slider.value : 0f;

            // moment of inertia: I = m * L²
            float momentOfInertia = m * (L * L);

            // torque calculation: t = I * a , a = slider value
            float torque = sliderValue * momentOfInertia * tweakFactor * pushSign;

            hingeAxisWorld = hinge.transform.TransformDirection(hinge.axis);

            // apply torque. ForceMode.Force for smooth continuous push
            rb.AddTorque(hingeAxisWorld * torque, ForceMode.Force);

            framesLeftToPush--;
        }

        prevAngle = angle;

        // sync with the other swing if assigned
        if (otherSwing != null && slider.value != 0)
        {
            // run sync only if this swing is heavier and slider is double the lighter one
            bool isHeavyDouble = (m > otherSwing.m) && (slider != null && otherSwing.slider != null)
                                 && Mathf.Approximately(slider.value, otherSwing.slider.value * 2f);
            if (isHeavyDouble)
            {
                // Velocity error
                float velError = angVel - otherSwing.angVel;

                // 🆕 POSITION ERROR - how far apart are their angles?
                float angleError = hinge.angle - otherSwing.hinge.angle;

                // Combined correction (velocity + position)
                float correctiveGain = 0.2f;
                float positionGain = 0.2f; // 🆕 tune this!

                float correctiveTorque = (velError * correctiveGain + angleError * positionGain)
                                         * otherSwing.m * (L * L);

                Vector3 otherHingeAxis = otherSwing.hinge.transform.TransformDirection(otherSwing.hinge.axis);
                otherSwing.rb.AddTorque(otherHingeAxis * correctiveTorque, ForceMode.Force);

                // Consider synced when BOTH velocity and angle are close
                bool velocityClose = Mathf.Abs(velError) < 1f;
                bool angleClose = Mathf.Abs(angleError) < 5f; // 🆕 within 5 degrees
                isVelocitySynced = velocityClose && angleClose;
            }
            else
            {
                isVelocitySynced = false;
            }
        }
        else
        {
            isVelocitySynced = false;
        }
    }

    public void ResetInternalState()
    {
        cooldownTimer = 0;
        prevAngle = 0;
    }

    IEnumerator CheckForces()
    {
        if (otherSwing.m < m)
        {
            if (Mathf.Approximately(slider.value, otherSwing.slider.value * 2))
            {
                yield return new WaitForSeconds(8f);
                stateManager.ShowFeedback();
            }
            else
            {
                yield return new WaitForSeconds(8f);
            }
        }
    }
}
