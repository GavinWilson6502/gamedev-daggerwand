using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class PlayerWeapon : MonoBehaviour
{
    [SerializeField] protected string weaponName;
    [SerializeField] protected bool projectile;
    [SerializeField] protected Vector2 direction;
    [SerializeField] bool continuous;
    [SerializeField] protected Vector2 initialV;
    [SerializeField] bool despawnOffTop;
    [SerializeField] bool despawnOffBottom;
    [SerializeField] float margin;
    Rigidbody2D rb;
    protected Collider2D collider;

    protected bool isPaused = false;
    bool wasEnabled = false;
    protected Dictionary<EnemyController, List<int>> attackStatus = new Dictionary<EnemyController, List<int>>();
    List<EnemyController> hits = new List<EnemyController>();
    List<EnemyController> tempHits = new List<EnemyController>();
    List<EnemyWeapon> blockedProjectiles = new List<EnemyWeapon>();

    // Start is called before the first frame update
    void Start()
    {
        collider = GetComponent<Collider2D>();
        if (!projectile) return;
        SpriteRenderer renderer = GetComponent<SpriteRenderer>();
        ProjectileLauncher launcher = GetComponentInParent<ProjectileLauncher>();
        renderer.flipX = launcher.getFlipX();
        renderer.flipY = launcher.getFlipY();
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = transform.TransformDirection(new Vector2(launcher.getFlipX() ? -initialV.x : initialV.x, launcher.getFlipY() ? -initialV.y : initialV.y));
        transform.SetParent(null);
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return;

        if (projectile) {
            if (continuous) {
                foreach (EnemyController enemy in tempHits) hits.Add(enemy);
                tempHits.Clear();
                attackStatus.Clear();
                return;
            }
            foreach (KeyValuePair<EnemyController, List<int>> kvp in attackStatus) {
                foreach (int i in kvp.Value) {
                    if (i != 0) {
                        Destroy(gameObject);
                        return;
                    }
                }
            }
            return;
        }
        if (weaponName.Equals("Twirl")) {
            foreach (KeyValuePair<EnemyController, List<int>> kvp in attackStatus) {
                int wasEffective = 0;
                foreach (int i in kvp.Value) {
                    if (i < 0) wasEffective = -1;
                    else if (i == 1 && wasEffective >= 0) wasEffective = 1;
                }
                if (wasEffective > 0) GetComponentInParent<PlayerController>().onStun();
            }
        }
        else if (weaponName.Equals("Block")) {
            blockedProjectiles.RemoveAll(item => item == null);
            if (attackStatus.Count != 0) {
                int state = 0;
                bool abs1 = false;
                foreach (KeyValuePair<EnemyController, List<int>> kvp in attackStatus) {
                    state = 0;
                    abs1 = false;
                    foreach (int i in kvp.Value) {
                        if (Math.Abs(i) == 1) abs1 = true;
                        if (i < 0) state = -1;
                        else if (i == 3 && state >= 0) state = 1;
                    }
                    if (state < 1 && abs1) break;
                }
                GetComponentInParent<PlayerController>().onBlock(state < 1 && abs1);
            }
        }
        attackStatus.Clear();
    }

    void FixedUpdate() {
        if (isPaused) return;

        if (projectile) return;
        if (wasEnabled && !continuous) collider.enabled = false;
        wasEnabled = collider.enabled;
    }

    void OnTriggerEnter2D(Collider2D other) {
        OnTriggerStay2D(other);
    }

    public virtual void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.layer != 10) return;
        EnemyWeapon enemyWeapon = other.GetComponent<EnemyWeapon>();
        EnemyController enemy = other.transform.parent == null ? enemyWeapon.getEnemyController() : other.GetComponentInParent<EnemyController>();
        if (weaponName.Equals("Block") && other.transform.parent != null && other.transform.parent.name.Equals("Shield Mage")) {
            if (!attackStatus.ContainsKey(enemy)) attackStatus.Add(enemy, new List<int>());
            attackStatus[enemy].Add(2);
            return;
        }
        if (weaponName.Equals("Block") && other.name.Equals("Mage Shockwave(Clone)")) return;
        if (enemy == null || hits.Contains(enemy) || (weaponName.Equals("Block") && blockedProjectiles.Contains(enemyWeapon))) return;
        int temp = enemy.onAttacked(weaponName, projectile, projectile ? rb.velocity.normalized : (direction == Vector2.zero ? (other.transform.position - transform.position).normalized : direction), other);
        if (temp == 0) return;
        if (!attackStatus.ContainsKey(enemy)) attackStatus.Add(enemy, new List<int>());
        attackStatus[enemy].Add(temp);
        if (projectile && continuous && !tempHits.Contains(enemy)) tempHits.Add(enemy);
        else if (weaponName.Equals("Block") && enemyWeapon.getProjectile() && !blockedProjectiles.Contains(enemyWeapon)) blockedProjectiles.Add(enemyWeapon);
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

    public void setIsPaused(bool paused) {
        isPaused = paused;
    }
}
