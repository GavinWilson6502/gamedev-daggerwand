using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentManager : MonoBehaviour
{
    static PersistentManager singleton;

    int lives = 3;
    int potions = 0;
    int[] magic = new int[] {100, 100, 100, 100, 100, 100, 100};
    List<bool> pickedUp = null;
    int checkpoint = 0;
    int levelsBeaten = 1;
    int prevLevel = -1;
    float masterVolume = 0, musicVolume = 1, sfxVolume = 1;
    bool nativeResolution = true;
    int[] customResolution = new int[] {1920, 1080};

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void store(int storeLives, int storePotions, int[] storeMagic, List<bool> storePickedUp, int storeCheckpoint) {
        lives = storeLives;
        potions = storePotions;
        magic = storeMagic;
        pickedUp = storePickedUp;
        checkpoint = storeCheckpoint;
    }
    public void die() {
        if (--lives <= 0) {
            store(3, 0, new int[] {100, 100, 100, 100, 100, 100, 100}, null, 0);
            SceneManager.LoadScene(0);
        }
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }
    public void win(int storeLives, int storePotions, int levelId) {
        store(storeLives, storePotions, new int[] {100, 100, 100, 100, 100, 100, 100}, null, 0);
        levelsBeaten |= (1 << levelId);
        if (SceneManager.GetActiveScene().buildIndex < 7) {
            prevLevel = 0;
            SceneManager.LoadScene(0);
        }
        else SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex + 1);
    }
    public void init(GameplayManager manager) {
        if (singleton == null) {
            singleton = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        manager.setPersistentManager(singleton);
        Destroy(gameObject);
    }
    public void init(MenuManager manager) {
        if (singleton == null) {
            singleton = this;
            DontDestroyOnLoad(gameObject);
            return;
        }
        manager.setPersistentManager(singleton);
        Destroy(gameObject);
    }
    public int fetchLives() {
        return lives;
    }
    public int fetchPotions() {
        return potions;
    }
    public int[] fetchMagic() {
        return magic;
    }
    public List<bool> fetchPickedUp() {
        return pickedUp;
    }
    public int fetchCheckpoint() {
        return checkpoint;
    }
    public int fetchLevelsBeaten() {
        return levelsBeaten;
    }
    public int fetchPrevLevel() {
        return prevLevel;
    }
    public float[] fetchVolumes() {
        return new float[] {masterVolume, musicVolume, sfxVolume};
    }
    public int[] fetchResolution() {
        if (nativeResolution) return null;
        int[] temp = new int[customResolution.Length];
        for (int i = 0; i < customResolution.Length; ++i) temp[i] = customResolution[i];
        return temp;
    }
    public void storePrevLevel(int levelId) {
        prevLevel = levelId;
    }
    public void storeVolumes(float master, float music, float sfx) {
        masterVolume = master;
        musicVolume = music;
        sfxVolume = sfx;
    }
    public void storeResolution(bool native, int width, int height) {
        if (nativeResolution = native) return;
        customResolution = new int[] {width, height};
    }
}
