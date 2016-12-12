using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

public static class Extensions
{
  public static Vector2 PerpendicularCounterClockwise(this Vector2 vector2)
  {
    return new Vector2(-vector2.y, vector2.x);
  }

  public static Vector2 PerpendicularClockwise(this Vector2 vector2)
  {
    return new Vector2(vector2.y, -vector2.x);
  }

  public static float GetAngleToward(this Vector2 from, Vector2 to)
  {
    var sign = Mathf.Sign(from.x * to.y - from.y * to.x);
    return Vector2.Angle(from, to) * sign;
  }

  public static Vector2 ToVector2(this Vector3 v)
  {
    return new Vector2(v.x, v.y);
  }

  public static Vector3 ToVector3(this Vector2 v, float z)
  {
    return new Vector3(v.x, v.y, z);
  }

  public static T Pick<T>(this T[] arr)
  {
    var index = Random.Range(0, arr.Length - 1);
    return arr[index];
  }
}
