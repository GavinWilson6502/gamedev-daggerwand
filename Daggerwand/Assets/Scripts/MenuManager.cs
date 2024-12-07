using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using TMPro;

public class MenuManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] PersistentManager persistentManager;
    [Header("Camera")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Vector2 aspectRatio = new Vector2(16, 9);
    [Header("UI")]
    [SerializeField] GameObject optionsMenu;
    [SerializeField] GameObject startScreen;
    [SerializeField] GameObject levelSelect;
    [SerializeField] GameObject gameOver;
    [SerializeField] Transform nativeResolution;
    [SerializeField] Transform customResolution;
    [SerializeField] Slider masterSlider;
    [SerializeField] Slider musicSlider;
    [SerializeField] Slider sfxSlider;
    [SerializeField] TextMeshProUGUI masterPercent;
    [SerializeField] TextMeshProUGUI musicPercent;
    [SerializeField] TextMeshProUGUI sfxPercent;
    [SerializeField] Button doneButton;
    [SerializeField] AudioMixer mixer;
    [SerializeField] Button startButton;
    [SerializeField] Button optionsButton;
    [SerializeField] Button quitButton;
    [SerializeField] Button button1;
    [SerializeField] Button button2;
    [SerializeField] Button button3;
    [SerializeField] Button button4;
    [SerializeField] Button button5;
    [SerializeField] Button button6;
    [SerializeField] Button button7;
    [SerializeField] Button backButton;
    [SerializeField] Button continueButton;
    [SerializeField] Button levelSelectButton;
    [SerializeField] AudioSource audioSource;
    [SerializeField] AudioClip buttonSFX;

    // Start is called before the first frame update
    void Start()
    {
        persistentManager.init(this);
        if (persistentManager.fetchPrevLevel() == -1) {
            Resolution res = Screen.resolutions[Screen.resolutions.Length - 1];
            Screen.SetResolution(res.width, res.height, true);
            initOptionsMenu();
        }
        else if (persistentManager.fetchPrevLevel() == 0) initLevelSelect();
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

    void initOptionsMenu() {
        Resolution res = Screen.resolutions[Screen.resolutions.Length - 1];
        float horizontal = res.width;
        float vertical = res.height;
        TextMeshProUGUI nativeLabel = nativeResolution.GetChild(1).GetComponent<TextMeshProUGUI>();
        Toggle nativeToggle = nativeResolution.GetChild(0).GetComponent<Toggle>();
        TextMeshProUGUI customLabel = customResolution.GetChild(1).GetComponent<TextMeshProUGUI>();
        TextMeshProUGUI customBy = customResolution.GetChild(3).GetComponent<TextMeshProUGUI>();
        Toggle customToggle = customResolution.GetChild(0).GetComponent<Toggle>();
        TMP_InputField customWidth = customResolution.GetComponentsInChildren<TMP_InputField>()[0];
        TMP_InputField customHeight = customResolution.GetComponentsInChildren<TMP_InputField>()[1];
        nativeLabel.SetText("Native (" + horizontal + "x" + vertical + ")");
        int[] stored = persistentManager.fetchResolution();
        customWidth.SetTextWithoutNotify(stored == null ? "" : "" + stored[0]);
        customHeight.SetTextWithoutNotify(stored == null ? "" : "" + stored[1]);
        if (nativeToggle.isOn = (stored == null)) {
            nativeLabel.color = Color.white;
            customToggle.isOn = false;
            customLabel.color = Color.gray;
            customWidth.interactable = false;
            customBy.color = Color.gray;
            customHeight.interactable = false;
        }
        else {
            nativeLabel.color = Color.gray;
            customToggle.isOn = true;
            customLabel.color = Color.white;
            customWidth.interactable = true;
            customBy.color = Color.white;
            customHeight.interactable = true;
        }
        float[] volumes = persistentManager.fetchVolumes();
        mixer.SetFloat("Master_volume", masterSlider.value == 0 ? -80 : Mathf.Log10(masterSlider.value = volumes[0]) * 20);
        mixer.SetFloat("Music_volume", musicSlider.value == 0 ? -80 : Mathf.Log10(musicSlider.value = volumes[1]) * 20);
        mixer.SetFloat("SFX_volume", sfxSlider.value == 0 ? -80 : Mathf.Log10(sfxSlider.value = volumes[2]) * 20);
        optionsMenu.SetActive(true);
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
        //if (count >= 6) button7.gameObject.SetActive(true);
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
        audioSource.PlayOneShot(buttonSFX, 4);
        if (button == doneButton) {
            int width = 1920;
            int height = 1080;
            int.TryParse(customResolution.GetChild(2).GetComponent<TMP_InputField>().text, out width);
            int.TryParse(customResolution.GetChild(4).GetComponent<TMP_InputField>().text, out height);
            persistentManager.storeResolution(nativeResolution.GetComponentInChildren<Toggle>().isOn, width, height);
            if (!(nativeResolution.GetComponentInChildren<Toggle>().isOn && Screen.fullScreen))
                Screen.SetResolution(width, height, nativeResolution.GetComponentInChildren<Toggle>().isOn);
            persistentManager.storeVolumes(masterSlider.value, musicSlider.value, sfxSlider.value);
            optionsMenu.SetActive(false);
            startScreen.SetActive(true);
        }
        else if (button == startButton) {
            startScreen.SetActive(false);
            initLevelSelect();
        }
        else if (button == optionsButton) {
            startScreen.SetActive(false);
            initOptionsMenu();
        }
        else if (button == quitButton) Application.Quit();
        else if (button == button1) SceneManager.LoadScene(1);
        else if (button == button2) SceneManager.LoadScene(2);
        else if (button == button3) SceneManager.LoadScene(3);
        else if (button == button4) SceneManager.LoadScene(4);
        else if (button == button5) SceneManager.LoadScene(5);
        else if (button == button6) SceneManager.LoadScene(6);
        else if (button == button7) Debug.Log("Button7 pressed");
        else if (button == backButton) {
            levelSelect.SetActive(false);
            startScreen.SetActive(true);
        }
        else if (button == continueButton) SceneManager.LoadScene(persistentManager.fetchPrevLevel());
        else if (button == levelSelectButton) {
            gameOver.SetActive(false);
            initLevelSelect();
        }
    }

    public void onValueChanged(Slider slider) {
        if (slider == masterSlider) {
            mixer.SetFloat("Master_volume", slider.value == 0 ? -80 : Mathf.Log10(slider.value) * 20);
            masterPercent.SetText("" + (int)(slider.value * 100) + "%");
        }
        else if (slider == musicSlider) {
            mixer.SetFloat("Music_volume", slider.value == 0 ? -80 : Mathf.Log10(slider.value) * 20);
            musicPercent.SetText("" + (int)(slider.value * 100) + "%");
        }
        else if (slider == sfxSlider) {
            mixer.SetFloat("SFX_volume", slider.value == 0 ? -80 : Mathf.Log10(slider.value) * 20);
            sfxPercent.SetText("" + (int)(slider.value * 100) + "%");
        }
    }

    public void onValueChanged(Toggle toggle) {
        if (toggle == nativeResolution.GetComponentInChildren<Toggle>()) {
            nativeResolution.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            customResolution.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.gray;
            customResolution.GetChild(2).GetComponent<TMP_InputField>().interactable = false;
            customResolution.GetChild(3).GetComponent<TextMeshProUGUI>().color = Color.gray;
            customResolution.GetChild(4).GetComponent<TMP_InputField>().interactable = false;
        }
        else if (toggle == customResolution.GetComponentInChildren<Toggle>()) {
            customResolution.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.white;
            customResolution.GetChild(2).GetComponent<TMP_InputField>().interactable = true;
            customResolution.GetChild(3).GetComponent<TextMeshProUGUI>().color = Color.white;
            customResolution.GetChild(4).GetComponent<TMP_InputField>().interactable = true;
            nativeResolution.GetChild(1).GetComponent<TextMeshProUGUI>().color = Color.gray;
        }
    }
}
