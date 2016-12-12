using System.Collections.Generic;
using UnityEngine;

public class SurfaceBehaviour : MonoBehaviour
{
  public readonly List<Collision2D> collisions = new List<Collision2D>();
  void OnCollisionEnter2D(Collision2D collision)
  {
    collisions.RemoveAll(c => c == null || c.collider == null || c.gameObject == null || c.gameObject == collision.gameObject);
    collisions.Add(collision);
  }

  void OnCollisionExit2D(Collision2D collision)
  {
    collisions.RemoveAll(c => c == null || c.collider == null || c.gameObject == null || c.gameObject == collision.gameObject);
  }

  void OnShapeshifted()
  {
    collisions.Clear();
  }

  public bool GetSurfaceNormal(out Vector2 surfaceNormal)
  {
    var c = Vector2.zero;
    var count = 0;
    foreach (var collision in collisions)
    {
      foreach (var contact in collision.contacts)
      {
        c += contact.normal;
        count++;
      }
    }
    if (count > 0)
    {
      surfaceNormal = c / (float)count;
      return true;
    }
    else
    {
      surfaceNormal = Vector2.zero;
      return false;
    }
  }
}
