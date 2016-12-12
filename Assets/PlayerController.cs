using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
  public float Speed = 0f;

  public GameObject splatterPrefab;
  public GameObject wallpaperSplatterPrefab;
  public bool isFrozen = false;

  // Update is called once per frame
  void Update()
  {
    var surfaceBehaviour = GetComponent<SurfaceBehaviour>();

    var gravityDirection = Physics2D.gravity.normalized;

    var flatSurfaceNormal = surfaceBehaviour.collisions
      .SelectMany(collision => collision.contacts)
      .Select(contact => contact.normal)
      .Aggregate(-gravityDirection, (best, normal) => Vector2.Dot(-gravityDirection, normal) > Vector2.Dot(-gravityDirection, best) ? normal : best);
    var isOnSurface = surfaceBehaviour.collisions.Count > 0 && Vector2.Dot(-gravityDirection, flatSurfaceNormal) > 0.3f;

    // Rotation
    if (surfaceBehaviour.collisions.Count > 0)
    {
      transform.rotation = Quaternion.Euler(0f, 0f, Vector2.up.GetAngleToward(flatSurfaceNormal));
    }

    // Movement
    var up = flatSurfaceNormal;
    var right = flatSurfaceNormal.PerpendicularClockwise();

    var rigidbody = GetComponent<Rigidbody2D>();
    var movex = Input.GetAxis("Horizontal");

    if (!Mathf.Approximately(movex, 0.0f))
      GetComponent<SpriteRenderer>().flipX = movex < 0;

    var speed = isOnSurface ? Speed : Speed * 0.1f;

    var newVelocity = right * movex * speed + up * Vector2.Dot(up, rigidbody.velocity);
    rigidbody.velocity = newVelocity;
  }

  void OnTriggerEnter2D(Collider2D other)
  {
    if (other.tag == "Finish")
    {
      Vector2 surface;
      if (GetComponent<SurfaceBehaviour>().GetSurfaceNormal(out surface))
      {
        var sameRotation = Vector2.Dot(surface, Vector2.up) > 0.9;
        if (sameRotation)
        {
          GameObject.FindObjectOfType<GameBehaviour>().OnFinish();
        }
      }
    }
  }

  float GetContactForce(ContactPoint2D contact)
  {
    var self = contact.collider.attachedRigidbody;
    var other = contact.otherCollider.attachedRigidbody;
    var myContactVelocity = self.GetPointVelocity(contact.point);
    var otherContactVelocity = other.GetPointVelocity(contact.point);
    return Vector2.Dot(contact.normal, otherContactVelocity - myContactVelocity);
  }

  void HandleCollision(Collision2D collision)
  {
    if (!gameObject.activeSelf)
      return;
    var isCrushed = GetComponent<SurfaceBehaviour>()
      .collisions.Any(surfaceCollision =>
      {
        var contactCombinations =
          surfaceCollision.contacts.SelectMany(
            surfaceContact => collision.contacts.Select(currentContact => new { surfaceContact, currentContact }))
            .ToArray();
        var contactCombinationCount = contactCombinations.Length;
        //Debug.Log("  Collision:");
        var opposingContactCombinations = contactCombinations
          .Where(contactCombination => Vector2.Dot(contactCombination.currentContact.normal, contactCombination.surfaceContact.normal) < 0)
          .ToArray();
        if (opposingContactCombinations.Length == 0)
          return false;
        var totalForce = opposingContactCombinations
          .Max(pair =>
          {
            var currentContactSpeed = GetContactForce(pair.currentContact);
            var surfaceContactSpeed = GetContactForce(pair.surfaceContact);
            //Debug.Log((int)Time.time + "    " + currentContactSpeed + "  " + surfaceContactSpeed);
            return currentContactSpeed - surfaceContactSpeed;
          });
        //Debug.Log(totalForce);
        //Debug.Log("    Total: " + totalForce);
        return totalForce > 0.2f;
      });

    if (isCrushed)
    {
      //Debug.Log("Died");
      var collisions = GetComponent<SurfaceBehaviour>().collisions.Concat(new[] { collision }).ToArray();
      var solidCollision =
        collisions.FirstOrDefault(c => c.rigidbody.bodyType == RigidbodyType2D.Static);
      var house = GameObject.FindGameObjectWithTag("House");
      if (solidCollision != null)
      {
        var contact = solidCollision.contacts.First();
        var alongSurface = contact.normal.PerpendicularClockwise();
        var point = contact.point +
                    alongSurface*Vector2.Dot(alongSurface, transform.position.ToVector2() - contact.point);

        var splatter = GameObject.Instantiate(splatterPrefab, house.transform, true);
        splatter.transform.position = point;
        splatter.transform.Rotate(0, 0, Vector2.up.GetAngleToward(contact.normal));
      }
      else
      {
        var splatter = GameObject.Instantiate(wallpaperSplatterPrefab, house.transform, true);
        splatter.transform.position = transform.position;
        splatter.GetComponent<SpriteRenderer>().flipX = Random.Range(0, 1) == 1;
        splatter.GetComponent<SpriteRenderer>().flipY = Random.Range(0, 1) == 1;
      }

      GameObject.FindObjectOfType<GameBehaviour>().OnDied();
      gameObject.SetActive(false);
    }
  }

  void OnCollisionEnter2D(Collision2D collision)
  {
    HandleCollision(collision);
  }

  void OnCollisionStay2D(Collision2D collision)
  {
    HandleCollision(collision);
  }
}
