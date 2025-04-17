using UnityEngine;
using System.Collections.Generic;
using TMPro;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    public TextMeshProUGUI label;

    private int currentPathIndex = 0;
    private float arriveThreshold = 3f;
    private float maxSpeed = 5f;
    private float minSpeed = 2f;
    private float turnSensitivity = 5f;

    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

    void Update()
    {
        if (path != null && path.Count > 0)
        {
            FollowPath();
        }
        else
        {
            SeekSingleTarget();
        }

        // Show debug label (distance to next point/target)
        float distance = (path != null && path.Count > 0)
            ? Vector3.Distance(transform.position, path[currentPathIndex])
            : Vector3.Distance(transform.position, target);
        if (label != null)
            label.text = $"Dist: {distance:F2}";
    }

float GetSignedAngle(Vector3 from, Vector3 to)
{
    float angle = Vector3.SignedAngle(from, to, Vector3.up); // Y axis for 2D turning
    return angle;
}
    void SeekSingleTarget()
{
    Vector3 direction = target - transform.position;
    direction.y = 0;
    float distance = direction.magnitude;

    if (distance < arriveThreshold)
    {
        kinematic.SetDesiredSpeed(0);
        kinematic.SetDesiredRotationalVelocity(0);
        return;
    }

    direction.Normalize();
    float desiredSpeed = Mathf.Lerp(minSpeed, maxSpeed, distance / 10f);
    kinematic.SetDesiredSpeed(desiredSpeed);

    float turnAmount = GetSignedAngle(transform.forward, direction);
    kinematic.SetDesiredRotationalVelocity(turnAmount * turnSensitivity);
}


    void FollowPath()
{
    if (currentPathIndex >= path.Count)
    {
        path = null;
        return;
    }

    Vector3 waypoint = path[currentPathIndex];
    Vector3 direction = waypoint - transform.position;
    direction.y = 0;
    float distance = direction.magnitude;

    if (distance < arriveThreshold)
    {
        currentPathIndex++;
        return;
    }

    direction.Normalize();
    float angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(direction));
    float speed = Mathf.Lerp(maxSpeed, minSpeed, angle / 90f);
    kinematic.SetDesiredSpeed(speed);

    float turnAmount = GetSignedAngle(transform.forward, direction);
    kinematic.SetDesiredRotationalVelocity(turnAmount * turnSensitivity);
}


    public void SetTarget(Vector3 target)
    {
        this.target = target;
        this.path = null;
        currentPathIndex = 0;
        EventBus.ShowTarget(target);
    }

    public void SetPath(List<Vector3> path)
    {
        this.path = path;
        currentPathIndex = 0;
    }

    public void SetMap(List<Wall> outline)
    {
        this.path = null;
        this.target = transform.position;
        currentPathIndex = 0;
    }
}
