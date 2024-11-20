using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] PersistentManager persistentManager;
    [Header("Camera")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Vector2 aspectRatio = new Vector2(16, 9);
    [Header("UI")]
    [SerializeField] GameObject levelSelect;
    [SerializeField] GameObject gameOver;
    [SerializeField] Button button1;
    [SerializeField] Button button2;
    [SerializeField] Button button3;
    [SerializeField] Button button4;
    [SerializeField] Button button5;
    [SerializeField] Button button6;
    [SerializeField] Button button7;
    [SerializeField] Button continueButton;
    [SerializeField] Button levelSelectButton;

    // Start is called before the first frame update
    void Start()
    {
        persistentManager.init(this);
        if (persistentManager.fetchPrevLevel() == 0) initLevelSelect();
        else gameOver.SetActive(true);
    }

    // Update is called once per frame
    void Update()
    {
        fixAspectRatio();
    }

    public void setPersistentManager(PersistentManager manager) {
        persistentManager = manager;
    }

    void fixAspectRatio() {
        float invScreenAspect = (float)Screen.height / Screen.width;
        float invTargetAspect = aspectRatio.y / aspectRatio.x;
        if (invScreenAspect < invTargetAspect) {  //if screen is shorter/wider than target aspect ratio, then do pillarboxing
            float targetWidth = invScreenAspect * aspectRatio.x / aspectRatio.y;
            mainCamera.rect = new Rect((1 - targetWidth) * 0.5f, 0, targetWidth, 1);
        }
        else if (invScreenAspect > invTargetAspect) { //if screen is taller/narrower than target aspect ratio, then do letterboxing
            float targetHeight = Screen.width * invTargetAspect / Screen.height;
            mainCamera.rect = new Rect(0, (1 - targetHeight) * 0.5f, 1, targetHeight);
        }
    }

    void initLevelSelect() {
        int levelsBeaten = persistentManager.fetchLevelsBeaten();
        int count = 0;
        if (initButton(levelsBeaten, 1, button1)) ++count;
        if (initButton(levelsBeaten, 2, button2)) ++count;
        if (initButton(levelsBeaten, 3, button3)) ++count;
        if (initButton(levelsBeaten, 4, button4)) ++count;
        if (initButton(levelsBeaten, 5, button5)) ++count;
        if (initButton(levelsBeaten, 6, button6)) ++count;
        levelSelect.SetActive(true);
        if (count >= 6) button7.gameObject.SetActive(true);
    }
    bool initButton(int levelsBeaten, int i, Button b) {
        if (((levelsBeaten >> i) & 1) == 1) {
            ColorBlock c = b.colors;
            c.normalColor = c.disabledColor;
            b.colors = c;
            return true;
        }
        return false;
    }

    public void onClick(Button button) {
        if (button == button1) SceneManager.LoadScene(1);
        else if (button == button2) SceneManager.LoadScene(2);
        else if (button == button3) SceneManager.LoadScene(3);
        else if (button == button4) SceneManager.LoadScene(4);
        else if (button == button5) SceneManager.LoadScene(5);
        else if (button == button6) SceneManager.LoadScene(6);
        else if (button == button7) Debug.Log("Button7 pressed");
        else if (button == continueButton) SceneManager.LoadScene(persistentManager.fetchPrevLevel());
        else if (button == levelSelectButton) {
            gameOver.SetActive(false);
            levelSelect.SetActive(true);
        }
    }
}
