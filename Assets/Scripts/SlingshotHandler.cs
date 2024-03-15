using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class SlingshotHandler : MonoBehaviour
{
    [Header("Line Renderers")]
    [SerializeField] private LineRenderer leftLineRenderer;
    [SerializeField] private LineRenderer rightLineRenderer;

    [Header("Transforms")]
    [SerializeField] private Transform leftStartPosition;
    [SerializeField] private Transform rightStartPosition;
    [SerializeField] private Transform centerPosition;
    [SerializeField] private Transform idlePosition;
    [SerializeField] private Transform elasticTransform;

    [Header("Properties")]
    [SerializeField] private float maxDistance = 3.5f;
    [SerializeField] private float shotForce = 5f;
    [SerializeField] private float timeBetweenBirdRespawns = 2f;
    [SerializeField] private float elasticDivider = 1.2f;
    [SerializeField] private float maxAnimationTime = 1f;
    [SerializeField] private AnimationCurve elasticCurve;

    [Header("Scripts")]
    [SerializeField] private SlingshotArea slingshotArea;
    [SerializeField] private CameraManager cameraManager;

    [Header("Angry Bird")]
    [SerializeField] private AngryBird angryBirdPrefab;
    [SerializeField] private float angryBirdPositionOffset = 1f;

    [Header("Sounds")]
    [SerializeField] private AudioClip[] elasticPulledClips;
    [SerializeField] private AudioClip[] elasticReleasedClips;

    private AngryBird spawnedAngryBird;
    private AudioSource audioSource;

    private Vector2 slingshotLinesPosition;
    private Vector2 direction;
    private Vector2 directionNormalized;

    private bool clickedWithinArea;
    private bool birdOnSlingshot;

    private void Awake() {
        audioSource = GetComponent<AudioSource>();

        leftLineRenderer.enabled = false;
        rightLineRenderer.enabled = false;

        SpawnAngryBird();
    }

    private void Update()
    {
        if (InputManager.wasLeftMouseButtonPressed && slingshotArea.IsWithinSlingshotArea()) {
          clickedWithinArea = true;

          if (birdOnSlingshot) {
            SoundManager.instance.PlayRandomClip(elasticPulledClips, audioSource);
            cameraManager.SwitchToFollowCam(spawnedAngryBird.transform);
          }
        }

        if (InputManager.isLeftMouseButtonPressed && clickedWithinArea && birdOnSlingshot) {
            DrawSlingshot();
            PositionAndRotateAngryBird();
        }

        if (InputManager.wasLeftMouseButtonReleased && birdOnSlingshot && clickedWithinArea) {
            if (GameManager.instance.HasEnoughShots()) {
                clickedWithinArea = false;
                birdOnSlingshot = false;

                spawnedAngryBird.LaunchBird(direction, shotForce);
                GameManager.instance.UseShot();

                SoundManager.instance.PlayRandomClip(elasticReleasedClips, audioSource);

                AnimateSlingshot();
                
                if (GameManager.instance.HasEnoughShots()) {
                    StartCoroutine(SpawnAngryBirdAfterTime());
                }
            }
        }
    }

    #region Slingshot Methods

    private void DrawSlingshot() 
    {
        Vector3 touchPosition = Camera.main.ScreenToWorldPoint(InputManager.mousePosition);
        slingshotLinesPosition = centerPosition.position + Vector3.ClampMagnitude(touchPosition - centerPosition.position, maxDistance);
        SetLines(slingshotLinesPosition);

        direction = (Vector2)centerPosition.position - slingshotLinesPosition;
        directionNormalized = direction.normalized;
    }

    private void SetLines(Vector2 position) 
    {
        leftLineRenderer.SetPosition(0, position);
        leftLineRenderer.SetPosition(1, leftStartPosition.position);

        rightLineRenderer.SetPosition(0, position);
        rightLineRenderer.SetPosition(1, rightStartPosition.position);

        if (!leftLineRenderer.enabled && !rightLineRenderer.enabled) {
            leftLineRenderer.enabled = true;    
            rightLineRenderer.enabled = true;    
        }
    }

    #endregion

    #region Angry Bird Methods

    private void SpawnAngryBird() {
        elasticTransform.DOComplete();
        SetLines(idlePosition.position);

        Vector2 direction = (centerPosition.position - idlePosition.position).normalized;
        Vector2 spawnPosition = (Vector2)idlePosition.position + direction * angryBirdPositionOffset;

        spawnedAngryBird = Instantiate(angryBirdPrefab, spawnPosition, Quaternion.identity);
        spawnedAngryBird.transform.right = direction;

        birdOnSlingshot = true;

        cameraManager.SwitchToIdleCam();
    }

    private void PositionAndRotateAngryBird() {
        spawnedAngryBird.transform.position = slingshotLinesPosition + directionNormalized * angryBirdPositionOffset;
        spawnedAngryBird.transform.right = directionNormalized;
    }

    private IEnumerator SpawnAngryBirdAfterTime() {
        yield return new WaitForSeconds(timeBetweenBirdRespawns);

        SpawnAngryBird();
    }
    #endregion

    #region Animate Slingshot

    private void AnimateSlingshot() {
        elasticTransform.position = leftLineRenderer.GetPosition(0);
        float dist = Vector2.Distance(elasticTransform.position, centerPosition.position);
        float time = dist / elasticDivider;

        elasticTransform.DOMove(centerPosition.position, time).SetEase(elasticCurve);
        StartCoroutine(AnimateSlingshotLines(elasticTransform, time));
    }

    private IEnumerator AnimateSlingshotLines(Transform trans, float time) {
        float elapsedTime = 0f;

        while (elapsedTime < time && elapsedTime < maxAnimationTime) {
            elapsedTime += Time.deltaTime;

            SetLines(trans.position);

            yield return null;
        }
    }

    #endregion
}
