using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using DG.Tweening;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;

    public int maxNumberOfShots = 3;
    private int usedNumberOfShots;
    [SerializeField] private float secondsToWaitBeforeDeathCheck = 3f;
    [SerializeField] private GameObject restartScreenObject;
    [SerializeField] private SlingshotHandler slingshotHandler;
    [SerializeField] private Image nextLevelImage;

    private IconHandler iconHandler;

    private List<Baddie> baddies = new List<Baddie>();

    private void Awake() {
        if (instance == null) {
            instance = this;
        }

        iconHandler = FindObjectOfType<IconHandler>();

        Baddie[] baddiesArr = FindObjectsOfType<Baddie>();

        for (int i = 0; i < baddiesArr.Length; i++)
        {
            baddies.Add(baddiesArr[i]);
        }
    }

    public void StartGame() {
        SceneManager.LoadScene(1);
    }

    public void QuitGame() {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
        Application.Quit();
    }

    public void UseShot() {
        usedNumberOfShots++;
        iconHandler.UseShot(usedNumberOfShots);

        CheckForLastShot();
    }

    public bool HasEnoughShots() {
        return usedNumberOfShots < maxNumberOfShots;
    }

    public void CheckForLastShot() {
        if (usedNumberOfShots == maxNumberOfShots) {
            StartCoroutine(CheckAfterWaitTime());
        }
    }

    private IEnumerator CheckAfterWaitTime() {
        yield return new WaitForSeconds(secondsToWaitBeforeDeathCheck);

        if (baddies.Count == 0) {
            WinGame();
        } else {
            RestartGame();
        }

    }

    public void RemoveBaddie(Baddie baddie) {
        baddies.Remove(baddie);
        CheckForAllDeadBaddies();
    }

    private void CheckForAllDeadBaddies() {
        if (baddies.Count == 0) {
            WinGame();
        }
    }

    #region Win/Lose

    private void WinGame() {
        restartScreenObject.SetActive(true);
        slingshotHandler.enabled = false;
    }

    public void RestartGame() {
        DOTween.Clear(true);
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void NextLevel() {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int maxLevels = SceneManager.sceneCountInBuildSettings;

        if (currentSceneIndex + 1 == maxLevels) {
            SceneManager.LoadScene(1);
        } else {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
        }

    }

    #endregion
}
