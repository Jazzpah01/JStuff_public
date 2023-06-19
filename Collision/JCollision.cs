using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class JCollision : MonoBehaviour
{
    public static bool Collides(IAABB box, Vector2 point)
    {
        return (point.x > box.MinPosition.x && point.y > box.MinPosition.y && point.x < box.MaxPosition.x && point.y < box.MaxPosition.y);
    }

    public static bool Collides(IAABB box0, IAABB box1)
    {
        //maxx1 > minx2 && minx1 < maxx2 && maxy1 > miny1 && miny1 < maxy2
        return box0.MaxPosition.x > box1.MinPosition.x && box0.MinPosition.x < box1.MaxPosition.x &&
            box0.MaxPosition.y > box1.MinPosition.y && box0.MinPosition.y < box1.MaxPosition.y;
    }

    public static bool Collides(ICircleCollider circle, Vector2 point)
    {
        return Vector2.Distance(circle.Position, point) <= circle.Radius;
    }

    public static bool Collides(Vector2 circlePoint, float circleRadius, Vector2 point)
    {
        return Vector2.Distance(circlePoint, point) <= circleRadius;
    }
}
