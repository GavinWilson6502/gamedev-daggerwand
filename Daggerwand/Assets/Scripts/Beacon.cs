using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Beacon : MonoBehaviour
{
    [SerializeField] GameplayManager gameplayManager;
    [SerializeField] Sprite beaconIntact;
    [SerializeField] Sprite beaconAlight;
    [SerializeField] Sprite beaconExtinguished;
    SpriteRenderer beaconRenderer;
    Rigidbody2D arrow;
    [SerializeField] float arrowSpeed;
    [SerializeField] Sprite arrowIntact;
    [SerializeField] Sprite arrowAlight;
    [SerializeField] Sprite arrowExtinguished;
    SpriteRenderer arrowRenderer;
    [SerializeField] float maxStateTime;
    [SerializeField] float shootTime;
    [SerializeField] float burnTime;
    float stateTime;
    Vector2 target = Vector2.zero;

    bool shot = false;
    bool hit = false;
    Rigidbody2D rb;
    ProjectileLauncher launcher;

    // Start is called before the first frame update
    void Start()
    {
        beaconRenderer = GetComponent<SpriteRenderer>();
        stateTime = maxStateTime;
        rb = GetComponent<Rigidbody2D>();
        launcher = GetComponentInChildren<ProjectileLauncher>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() {
        launcher.transform.position = new Vector2(launcher.transform.position.x, Mathf.Max(transform.position.y + 15.5f, gameplayManager.getMainCameraY() + gameplayManager.getMainCameraHeight() + 1));
        if (gameplayManager.isPaused()) {
            rb.simulated = false;
            return;
        }
        rb.simulated = true;
        if (rb.bodyType != RigidbodyType2D.Dynamic) return;
        if (stateTime > shootTime) {
            if (stateTime < maxStateTime || (targetReached() && Physics2D.BoxCast(transform.position, new Vector2(0.4375f, 0.0625f), 0, Vector2.down, 0.5f, 64))) {
                GetComponent<Collider2D>().enabled = true;
                rb.velocity = Vector2.zero;
                beaconRenderer.sprite = beaconAlight;
                stateTime -= Time.deltaTime;
            }
            else if (targetReached()) GetComponent<Collider2D>().enabled = true;
            return;
        }
        if (!shot) {
            arrow = launcher.launch(1, 1).GetComponent<Rigidbody2D>();
            gameplayManager.addProjectile(arrow.GetComponent<EnemyWeapon>());
            arrow.transform.SetParent(null);
            arrow.velocity = new Vector2(0, -arrowSpeed);
            arrowRenderer = arrow.GetComponent<SpriteRenderer>();
            arrowRenderer.sprite = arrowIntact;
            shot = true;
            return;
        }
        if (arrow == null) hit = true;
        if (!hit) {
            if (arrow.transform.position.y + arrow.velocity.y * Time.fixedDeltaTime <= transform.position.y + 0.25f) {
                arrow.transform.position = new Vector2(transform.position.x, transform.position.y + 0.25f);
                arrow.bodyType = RigidbodyType2D.Kinematic;
                arrow.velocity = Vector2.zero;
                Collider2D[] arrowColliders =  arrow.GetComponentsInChildren<Collider2D>();
                for (int i = 0; i < arrowColliders.Length; ++i) arrowColliders[i].enabled = false;
                arrowRenderer.sprite = arrowAlight;
                hit = true;
            }
            return;
        }
        if (stateTime > 0) {
            if (stateTime <= burnTime) {
                beaconRenderer.sprite = beaconExtinguished;
                if (arrowRenderer != null) arrowRenderer.sprite = arrowExtinguished;
            }
            stateTime -= Time.deltaTime;
            return;
        }
        if (arrow != null) Destroy(arrow.gameObject);
        Destroy(launcher.gameObject);
        Destroy(gameObject);
    }

    public void setTarget(float x, float y) {
        target.x = x;
        target.y = y;
    }

    private bool targetReached() {
        if (transform.position.y < target.y) {
            transform.position = new Vector2(transform.position.x, target.y);
            return true;
        }
        if (rb.velocity.x < 0) return transform.position.x <= target.x;
        if (rb.velocity.x > 0) return transform.position.x >= target.x;
        return true;
    }
}
