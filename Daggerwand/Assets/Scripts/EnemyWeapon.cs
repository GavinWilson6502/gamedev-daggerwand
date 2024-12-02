using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UIElements;

public class EnemyWeapon : MonoBehaviour
{
    [SerializeField] bool projectile;
    bool held = false;
    [SerializeField] bool piercing;
    [SerializeField] bool persistAfterBlocked;
    [SerializeField] bool despawnOffTop;
    [SerializeField] bool despawnOffBottom;
    [SerializeField] float margin;
    [SerializeField] int damage;
    [SerializeField] int priority;
    EnemyController enemyController;
    bool disabled = false;

    protected bool isPaused = false;
    int attackStatus = 0;

    // Start is called before the first frame update
    void Start()
    {
        setEnemyController();
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return;
        if (projectile) {
            if ((!persistAfterBlocked && attackStatus < 0) || (!piercing && attackStatus > 0)) Destroy(gameObject);
        }
        else if (attackStatus < 0) onBlocked();
        attackStatus = 0;
    }

    void FixedUpdate() {
        Rigidbody2D[] rigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rigidbody in rigidbodies) rigidbody.simulated = !isPaused;
    }

    void OnTriggerEnter2D(Collider2D other) {
        OnTriggerStay2D(other);
    }

    public virtual void OnTriggerStay2D(Collider2D other) {
        if (disabled) return;
        if (attackStatus < 0 || other.transform.parent == null) return;
        PlayerController pc = other.GetComponentInParent<PlayerController>();
        if (pc == null) return;
        int temp = pc.onAttacked(projectile ? gameObject : enemyController.gameObject, projectile, damage, priority, other);
        if (temp == 0) return;
        attackStatus = temp;
    }

    void onBlocked() {
        if (persistAfterBlocked) return;
        GetComponent<Collider2D>().enabled = false;
        if (projectile) Destroy(gameObject);
    }

    public bool getProjectile() {
        return projectile;
    }

    public bool getHeld() {
        return held;
    }
    public void setHeld(bool h) {
        held = h;
    }

    public bool getPersistAfterBlocked() {
        return persistAfterBlocked;
    }

    public EnemyController getEnemyController() {
        return enemyController;
    }

    public void setIsPaused(bool paused) {
        isPaused = paused;
    }

    public bool getDespawnOffTop() {
        return despawnOffTop;
    }

    public bool getDespawnOffBottom() {
        return despawnOffBottom;
    }

    public float getMargin() {
        return margin;
    }

    public void setEnemyController() {
        enemyController = GetComponentInParent<EnemyController>();
    }

    public void setDisabled(bool d) {
        disabled = d;
    }
}
