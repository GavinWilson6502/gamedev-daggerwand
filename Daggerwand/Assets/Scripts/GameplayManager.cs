using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

public class GameplayManager : MonoBehaviour
{
    [Header("Scene Management")]
    [SerializeField] PersistentManager persistentManager;
    [SerializeField] float maxDeathTime;
    float deathTime = 0;
    bool dead = false;
    [SerializeField] PlayerController player;
    [Header("Camera")]
    [SerializeField] Camera mainCamera;
    [SerializeField] Vector2 aspectRatio = new Vector2(16, 9);
    [SerializeField] LevelVertex startingVertex;
    LevelVertex loVertex;
    LevelVertex hiVertex;
    [SerializeField] float maxScreenTransitionTime;
    float screenTransitionTime = 0;
    Vector3 screenTransitionStart = Vector3.zero;
    Vector3 screenTransitionEnd = Vector3.zero;
    [Header("UI")]
    [SerializeField] Canvas hud;
    [Header("Grid")]
    [SerializeField] Tilemap ground;
    [Header("Physics")]
    [SerializeField] Vector2 gravity;
    
    List<Pickup> pickups = new List<Pickup>();
    List<EnemyWeapon> projectiles = new List<EnemyWeapon>();
    bool gamePause = false;
    bool selectPause = false;

    // Start is called before the first frame update
    void Start()
    {
        loVertex = startingVertex;
        hiVertex = startingVertex.getPartner();
        fixLoHiVertices();
        Physics2D.gravity = gravity;
    }

    // Update is called once per frame
    void Update()
    {
        fixAspectRatio();
        if (!dead) return;
        deathTime -= Time.deltaTime;
        if (deathTime <= 0) persistentManager.die();
    }

    void FixedUpdate() {
        pickups.RemoveAll(item => item == null);
        projectiles.RemoveAll(item => item == null);
        foreach (Pickup p in pickups) p.setIsPaused(isPaused());
        foreach (EnemyWeapon p in projectiles) {
            p.setIsPaused(isPaused());
            if ((Mathf.Abs(p.transform.position.x - mainCamera.transform.position.x) > getMainCameraWidth() + p.getMargin())
                || (p.getDespawnOffTop() && p.transform.position.y - mainCamera.transform.position.y > getMainCameraHeight() + p.getMargin())
                || (p.getDespawnOffBottom() && mainCamera.transform.position.y - p.transform.position.y > getMainCameraHeight() + p.getMargin())) Destroy(p.gameObject);
        }
    }

    public PlayerController getPlayer() {
        return player;
    }

    public bool isPaused() {
        return gamePause || selectPause || screenTransitionTime > 0;
    }

    public void toggleGamePause() {
        if (selectPause) return;
        gamePause = !gamePause;
    }

    public bool getGamePause() {
        return gamePause;
    }

    public bool setSelectPause(bool inputHeld) {
        bool prev = selectPause;
        selectPause = inputHeld;
        return selectPause && !prev;
    }

    public bool getSelectPause() {
        return selectPause;
    }

    public void store(int lives, int potions, int[] magic) {
        persistentManager.store(lives, potions, magic);
    }
    public void die() {
        dead = true;
        deathTime = maxDeathTime;
        InputHandler ih = GetComponent<InputHandler>();
        if (ih != null) ih.enabled = false;
        gamePause = false;
        selectPause = false;
        setSelectMenuActive(false);
        screenTransitionTime = 0;
    }
    public void init() {
        persistentManager.init(this);
    }
    public int fetchLives() {
        return persistentManager.fetchLives();
    }
    public int fetchPotions() {
        return persistentManager.fetchPotions();
    }
    public int[] fetchMagic() {
        return persistentManager.fetchMagic();
    }

    public void setPersistentManager(PersistentManager manager) {
        persistentManager = manager;
    }

    public float getMainCameraX() {
        return mainCamera.transform.position.x;
    }

    public float getMainCameraY() {
        return mainCamera.transform.position.y;
    }

    public float getMainCameraHeight() {
        return mainCamera.orthographicSize;
    }

    public float getMainCameraWidth() {
        return mainCamera.orthographicSize * mainCamera.aspect;
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

    public bool moveCamera(Transform target) {
        if (screenTransitionTime <= 0) return scrollScreen(target);
        screenTransition(target);
        return false;
    }

    bool scrollScreen(Transform target) {
        mainCamera.transform.position = new Vector3(
            Mathf.Min(Mathf.Max(loVertex.transform.position.x, target.position.x), hiVertex.transform.position.x),
            Mathf.Min(Mathf.Max(loVertex.transform.position.y, target.position.y), hiVertex.transform.position.y),
            mainCamera.transform.position.z
        );

        float diffX = target.position.x - mainCamera.transform.position.x;
        float limitX = getMainCameraWidth() - 0.5f;
        if (Mathf.Abs(diffX) > limitX) target.position = new Vector2(mainCamera.transform.position.x + Mathf.Sign(diffX) * limitX, target.position.y);
        float diffY = target.position.y - mainCamera.transform.position.y;
        float limitY = getMainCameraHeight() - 0.5f;
        LevelVertex endpoint = null;
        if (mainCamera.transform.position.x <= loVertex.transform.position.x && mainCamera.transform.position.y <= loVertex.transform.position.y) endpoint = loVertex;
        else if (mainCamera.transform.position.x >= hiVertex.transform.position.x && mainCamera.transform.position.y >= hiVertex.transform.position.y) endpoint = hiVertex;

        if (Mathf.Abs(diffY) >= limitY) {
            if (diffY < 0) {
                if (endpoint != null && endpoint.getDown() != null) {
                    target.position = new Vector2(target.position.x, mainCamera.transform.position.y - limitY);
                    startScreenTransition(endpoint, endpoint.getDown(), target.position, new Vector3(target.position.x, endpoint.getDown().transform.position.y + getMainCameraHeight() - 1, 0));
                    return false;
                }
                if (diffY < -1.5f - limitY) {
                    //kill
                    return true;
                }
            }
            else if (diffY > 0 && endpoint != null && endpoint.getUp() != null) {
                target.position = new Vector2(target.position.x, mainCamera.transform.position.y + limitY);
                startScreenTransition(endpoint, endpoint.getUp(), target.position, new Vector3(target.position.x, endpoint.getUp().transform.position.y - getMainCameraHeight() + 1, 0));
                return false;
            }
        }
        if (endpoint == null) return false;
        if (Mathf.Abs(diffX) >= limitX) {
            if (diffX < 0 && endpoint.getLeft() != null) {
                startScreenTransition(endpoint, endpoint.getLeft(), target.position, new Vector3(Mathf.Floor(endpoint.getLeft().transform.position.x + getMainCameraWidth()) - 0.5f, target.position.y, 0));
                return false;
            }
            if (diffX > 0 && endpoint.getRight() != null) {
                startScreenTransition(endpoint, endpoint.getRight(), target.position, new Vector3(Mathf.Ceil(endpoint.getRight().transform.position.x - getMainCameraWidth()) + 0.5f, target.position.y, 0));
                return false;
            }
        }
        return false;
    }
    void startScreenTransition(LevelVertex cameraStart, LevelVertex cameraEnd, Vector3 targetStart, Vector3 targetEnd) {
        screenTransitionTime = maxScreenTransitionTime;
        loVertex = cameraStart;
        hiVertex = cameraEnd;
        screenTransitionStart = targetStart;
        screenTransitionEnd = targetEnd;
    }

    void screenTransition(Transform target) {
        screenTransitionTime -= Time.deltaTime;
        if (screenTransitionTime > 0) {
            float t = screenTransitionTime / maxScreenTransitionTime;
            target.position = Vector3.Lerp(screenTransitionEnd, screenTransitionStart, t);
            Vector3 lerped = Vector3.Lerp(hiVertex.transform.position, loVertex.transform.position, t);
            mainCamera.transform.position = new Vector3(lerped.x, lerped.y, mainCamera.transform.position.z);
            return;
        }
        target.position = screenTransitionEnd;
        mainCamera.transform.position = new Vector3(hiVertex.transform.position.x, hiVertex.transform.position.y, mainCamera.transform.position.z);
        loVertex = hiVertex.getPartner();
        fixLoHiVertices();
    }

    void fixLoHiVertices() {
        if (hiVertex.transform.position.x >= loVertex.transform.position.x && hiVertex.transform.position.y >= loVertex.transform.position.y) return;
        loVertex = hiVertex;
        hiVertex = loVertex.getPartner();
    }

    public Tilemap getGround() {
        return ground;
    }

    public void addPickup(Pickup p) {
        pickups.Add(p);
    }

    public void addProjectile(EnemyWeapon p) {
        projectiles.Add(p);
    }

    public void updateHUDHealth(float normalizedHealth) {
        RectTransform bar = hud.transform.GetChild(0).GetChild(0).GetComponent<RectTransform>();
        bar.localScale = new Vector2(bar.localScale.x, normalizedHealth);
    }

    public void updateHUDMagic(int weapon, float normalizedMagic) {
        RectTransform bar = hud.transform.GetChild(1).GetChild(0).GetComponent<RectTransform>();
        bar.localScale = new Vector2(bar.localScale.x, normalizedMagic);
        bar = hud.transform.GetChild(2).GetChild(0).GetChild(weapon).GetChild(0).GetComponent<RectTransform>();
        bar.localScale = new Vector2(bar.localScale.x, normalizedMagic);
    }

    public void updateHUDPotion(int potions, bool maxHealth) {
        hud.transform.GetChild(2).GetChild(0).GetChild(7).GetComponentsInChildren<TextMeshProUGUI>()[1].text = "" + potions;
        hud.transform.GetChild(2).GetChild(1).GetChild(7).GetComponent<Image>().color = (!maxHealth && potions > 0) ? new Color(0.75f, 0.75f, 0.75f) : new Color(0.5625f, 0.5625f, 0.5625f);
    }

    public void updateHUDSelection(int weapon) {
        Transform options = hud.transform.GetChild(2).GetChild(0);
        for (int i = 0; i < options.childCount - 1; ++i) {
            options.GetChild(i).GetComponent<Image>().color = i == weapon ? new Color(1f, 1f, 1f) : new Color(0.75f, 0.75f, 0.75f);
        }
        Transform magicBar = hud.transform.GetChild(1).GetChild(0);
        magicBar.parent.gameObject.SetActive(weapon != 0);
        magicBar.parent.GetComponentInChildren<TextMeshProUGUI>().text = options.GetChild(weapon).GetComponentInChildren<TextMeshProUGUI>().text;
        magicBar.GetComponent<RectTransform>().localScale = options.GetChild(weapon).GetChild(0).GetComponent<RectTransform>().localScale;
        magicBar.GetComponent<Image>().color = options.GetChild(weapon).GetChild(0).GetComponent<Image>().color;
    }

    public void updateHUDLives(int lives) {
        hud.transform.GetChild(2).GetChild(2).GetComponentsInChildren<TextMeshProUGUI>()[1].text = "" + lives;
    }

    public void setSelectMenuActive(bool active) {
        hud.transform.GetChild(2).gameObject.SetActive(active);
    }

    public Vector2 getGravity() {
        return gravity;
    }
}
