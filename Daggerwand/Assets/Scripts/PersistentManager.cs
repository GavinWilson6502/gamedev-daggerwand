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

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void store(int storeLives, int storePotions, int[] storeMagic) {
        lives = storeLives;
        potions = storePotions;
        magic = storeMagic;
    }
    public void die() {
        --lives;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
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
    public int fetchLives() {
        return lives;
    }
    public int fetchPotions() {
        return potions;
    }
    public int[] fetchMagic() {
        return magic;
    }
}
