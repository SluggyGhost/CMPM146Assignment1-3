using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System;
using UnityEditor.ShaderGraph.Internal;
using System.Net.Security;
using System.Runtime.InteropServices.WindowsRuntime;

public class SteeringBehavior : MonoBehaviour
{
    public Vector3 target;
    public KinematicBehavior kinematic;
    public List<Vector3> path;
    public TextMeshProUGUI label;

    private int currentPathIndex = 0;
    private float arriveThreshold = 10f;
    private float maxSpeed = 20f;
    private float minSpeed = 4f;
    private float facingTargetThreshold = 10f;

    void Start()
    {
        kinematic = GetComponent<KinematicBehavior>();
        target = transform.position;
        path = null;
        EventBus.OnSetMap += SetMap;
    }

    void Update()
    {
        bool pathMode = path != null && path.Count > 0;
        if (path != null)
        {
            Debug.Log($"path: {path}");
        } else Debug.Log("path null");
        if (pathMode)
        {
            Console.WriteLine("pathMode detected, calling FollowPath()");
            FollowPath();
        }
        else
        {
            Console.WriteLine("calling SeekTarget()");
            SeekTarget();
        }
    }
    
    int GetSign(float n)
    {
        if (n < 0)
        {
            return -1;
        }
        else
        {
            return 1;
        }
    }

    bool SharpTurn(float angle)
    {
        angle = Mathf.Abs(angle);
        if (angle > 45f) {
            return true;
        }
        return false;
    }
    void SeekTarget()
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
        float targetAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        // Debug.Log($"target angle: {targetAngle}");
        float speed;
        if (SharpTurn(targetAngle) && distance < 25f)
        {
            speed = minSpeed;
        }
        else
        {
            speed = Mathf.Lerp(minSpeed, maxSpeed, distance / 10f);
        }
        kinematic.SetDesiredSpeed(speed);

        float speedRotational = Mathf.Lerp(minSpeed, maxSpeed, targetAngle / 90f);;
        if (!FacingTarget(targetAngle))
        {
            if (SharpTurn(targetAngle))
            {
                speedRotational *= 10f;
            }
            else
            {
                speedRotational *= 3f;
        }
        kinematic.SetDesiredRotationalVelocity(speedRotational * GetSign(targetAngle));
        }
    }

    bool PathDone()
    {
        if (currentPathIndex == path.Count - 1)
        {
            return true;
        }
        return false;
    }

    bool FacingTarget(float angle)
    {
        if (Mathf.Abs(angle) <= facingTargetThreshold) {
            return true;
        }
        return false;
    }   

    void FollowPath()
    {
        if (PathDone())
        {
            // Reached final waypoint, now seek target
            SeekTarget();
            return;
        }

        Vector3 waypoint = path[currentPathIndex];
        Vector3 direction = waypoint - transform.position;
        direction.y = 0;
        float distance = direction.magnitude;
        Console.WriteLine($"cpi: {currentPathIndex}");
        if (distance < arriveThreshold)
        {
            currentPathIndex++;
            return;
        }

        direction.Normalize();
        float targetAngle = Vector3.SignedAngle(transform.forward, direction, Vector3.up);
        //Debug.Log($"target angle: {targetAngle}");
        float speed;
        if (SharpTurn(targetAngle) && distance < 25f)
        {
            speed = minSpeed;
        }
        else
        {
            speed = Mathf.Lerp(minSpeed, maxSpeed, distance / 10f);
        }
        kinematic.SetDesiredSpeed(speed);

        float speedRotational = Mathf.Lerp(minSpeed, maxSpeed, targetAngle / 90f);;
        if (!FacingTarget(targetAngle))
        {
            if (SharpTurn(targetAngle))
            {
                speedRotational *= 10f;
            }
            else
            {
                speedRotational *= 3f;
        }
        kinematic.SetDesiredRotationalVelocity(speedRotational * GetSign(targetAngle));
        }
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
