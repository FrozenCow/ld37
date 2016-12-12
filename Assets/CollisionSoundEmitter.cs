using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CollisionSoundEmitter : MonoBehaviour
{
  public AudioClip[] sounds;
  public float minVelocity;
  public float maxVelocity;

  void OnCollisionEnter2D(Collision2D collision)
  {
    if (sounds.Length == 0) return;
    var magnitude = collision.relativeVelocity.magnitude;
    if (magnitude <= minVelocity) return;
    var soundIndex = Random.Range(0, sounds.Length - 1);
    var sound = sounds[soundIndex];
    var volumeScale = (magnitude - minVelocity)/(maxVelocity - minVelocity);
    GetComponent<AudioSource>().PlayOneShot(sound, Mathf.Clamp(volumeScale, 0, 1.0f));
  }
}
