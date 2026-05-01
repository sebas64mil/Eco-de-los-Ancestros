using System.Collections.Generic;
using UnityEngine;

public static class TrajectoryCalculator2D
{
    public static List<Vector2> Calculate(
        Vector2 startPosition,
        Vector2 initialVelocity,
        TrajectorySettingsSO settings,
        LayerMask collisionMask,
        bool allowBounce
    )
    {
        List<Vector2> points = new List<Vector2>();

        Vector2 position = startPosition;
        Vector2 velocity = initialVelocity;

        int bounces = 0;

        points.Add(position);

        while (points.Count < settings.maxPoints && bounces <= settings.maxBounces)
        {
            Vector2 prevPosition = position;

            velocity += Physics2D.gravity * settings.timeStep;
            position += velocity * settings.timeStep;

            RaycastHit2D hit = Physics2D.Linecast(prevPosition, position, collisionMask);

            if (hit.collider != null && !hit.collider.isTrigger)
            {
                points.Add(hit.point);

                if (!allowBounce)
                {
                    break;
                }

                velocity = Vector2.Reflect(velocity, hit.normal) * settings.bounceDamping;

                position = hit.point + hit.normal * settings.surfaceOffset;

                points.Add(position);

                bounces++;

                if (velocity.magnitude < settings.minVelocity)
                    break;

                continue;
            }
            points.Add(position);
        }

        return points;
    }
}