using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameBehaviour : MonoBehaviour
{
  public enum GameState
  {
    Play,
    Died,
    Finished
  }

  private Camera camera;
  private GameObject house;
  private GameObject furniturePrefab;
  private Vector2 initialGravity;

  public AudioClip[] diedSounds;
  public AnimationCurve diedCameraPositionCurve;
  public AnimationCurve diedCameraRotationCurve;
  public AnimationCurve diedCameraZoomCurve;
  public AnimationCurve diedFadeCurve;
  public float diedTime;

  public AudioClip[] finishSounds;
  public AnimationCurve finishedCameraPositionCurve;
  public AnimationCurve finishedCameraRotationCurve;
  public AnimationCurve finishedCameraZoomCurve;
  public AnimationCurve finishedFadeCurve;
  public float finishedTime;

  private GameState gameState;

	void Start ()
	{
	  camera = GameObject.FindObjectOfType<Camera>();
    house = GameObject.FindGameObjectWithTag("House");
	  var initialFurniture = house.transform.Find("Furniture").gameObject;
	  furniturePrefab = Object.Instantiate(initialFurniture, house.transform, true);
    furniturePrefab.SetActive(false);
	  initialGravity = Physics2D.gravity;
	}

  void Update()
  {
    switch (gameState)
    {
      case GameState.Play:
        UpdatePlay();
        break;
      case GameState.Died:
        UpdateDied();
        break;
      case GameState.Finished:
        UpdateFinished();
        break;
    }
    
  }

  void UpdatePlay()
  {
    // Freezing/House turning
    var isFrozen = Input.GetKey(KeyCode.LeftShift);
    Time.timeScale = isFrozen ? 0.0f : 1.0f;
    if (isFrozen)
    {
      if (Input.GetKeyDown(KeyCode.LeftArrow))
      {
        Physics2D.gravity = Physics2D.gravity.PerpendicularClockwise();
      }
      if (Input.GetKeyUp(KeyCode.RightArrow))
      {
        Physics2D.gravity = Physics2D.gravity.PerpendicularCounterClockwise();
      }
    }
  }

  private float startTime;
  private Vector2 startCameraPosition;
  private float startCameraRotation;
  private float startOrtho;

  void UpdateDied()
  {
    var time = Time.unscaledTime - startTime;
    var scale = time / diedTime;
    camera.transform.position = LinearInterpolate(diedCameraPositionCurve.Evaluate(scale), startCameraPosition, Vector2.zero).ToVector3(camera.transform.position.z);
    camera.orthographicSize = LinearInterpolate(diedCameraZoomCurve.Evaluate(scale), startOrtho, 3.0f);
    var endCameraRotation = startCameraRotation + Mathf.DeltaAngle(startCameraRotation, 0);
    camera.transform.eulerAngles = new Vector3(0, 0, LinearInterpolate(diedCameraRotationCurve.Evaluate(scale), startCameraRotation, endCameraRotation));
    Physics2D.gravity = initialGravity.magnitude * new Vector2(Mathf.Sin(camera.transform.eulerAngles.z * Mathf.Deg2Rad), -Mathf.Cos(camera.transform.eulerAngles.z * Mathf.Deg2Rad));

    camera.gameObject.transform.Find("DiedCanvas/Text").GetComponent<CanvasRenderer>().SetAlpha(diedFadeCurve.Evaluate(scale));

    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
    {
      camera.gameObject.transform.Find("DiedCanvas").gameObject.SetActive(false);
      Respawn();
    }
  }

  public void OnDied()
  {
    GetComponent<AudioSource>().PlayOneShot(diedSounds.Pick());

    startCameraPosition = camera.transform.position;
    startOrtho = camera.orthographicSize;
    startTime = Time.unscaledTime;
    startCameraRotation = camera.transform.eulerAngles.z;
    camera.gameObject.transform.Find("DiedCanvas").gameObject.SetActive(true);
    gameState = GameState.Died;
  }

  void UpdateFinished()
  {
    var time = Time.unscaledTime - startTime;
    var scale = time / finishedTime;
    camera.transform.position = LinearInterpolate(finishedCameraPositionCurve.Evaluate(scale), startCameraPosition, Vector2.zero).ToVector3(camera.transform.position.z);
    camera.orthographicSize = LinearInterpolate(finishedCameraZoomCurve.Evaluate(scale), startOrtho, 3.0f);
    var endCameraRotation = startCameraRotation + Mathf.DeltaAngle(startCameraRotation, 0);
    camera.transform.eulerAngles = new Vector3(0, 0, LinearInterpolate(finishedCameraRotationCurve.Evaluate(scale), startCameraRotation, endCameraRotation));

    camera.gameObject.transform.Find("FinishedCanvas/Text").GetComponent<CanvasRenderer>().SetAlpha(finishedFadeCurve.Evaluate(scale));

    if (Input.GetKeyDown(KeyCode.LeftArrow) || Input.GetKeyDown(KeyCode.RightArrow) || Input.GetKeyDown(KeyCode.Space) || Input.GetKeyDown(KeyCode.Escape))
    {
      camera.gameObject.transform.Find("FinishedCanvas").gameObject.SetActive(false);
      LoadNextLevel();
    }
  }

  public void OnFinish()
  {
    GetComponent<AudioSource>().PlayOneShot(finishSounds.Pick());
    startCameraPosition = camera.transform.position;
    startOrtho = camera.orthographicSize;
    startTime = Time.unscaledTime;
    startCameraRotation = camera.transform.eulerAngles.z;
    camera.gameObject.transform.Find("FinishedCanvas").gameObject.SetActive(true);
    GameObject.FindGameObjectWithTag("Player").SetActive(false);
    gameState = GameState.Finished;
  }

  // Update is called once per frame
  private void Respawn () {
    var currentFurniture = house.transform.Find("Furniture").gameObject;
    Destroy(currentFurniture);
	  currentFurniture = Object.Instantiate(furniturePrefab, house.transform);
	  currentFurniture.name = "Furniture";
    currentFurniture.SetActive(true);

	  Physics2D.gravity = initialGravity;

    gameState = GameState.Play;
	}

  private void LoadNextLevel()
  {
    var currentIndex = SceneManager.GetActiveScene().buildIndex;
    var nextIndex = currentIndex + 1;
    SceneManager.LoadScene(nextIndex);
  }

  float LinearInterpolate(float scale, float min, float max)
  {
    return min + (max - min)*Mathf.Clamp(scale, 0.0f, 1.0f);
  }

  Vector2 LinearInterpolate(float scale, Vector2 min, Vector2 max)
  {
    return min + (max - min)*Mathf.Clamp(scale, 0.0f, 1.0f);
  }
}
