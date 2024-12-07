using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class PikeMage : BossController
{
    Transform player;
    [SerializeField] Sprite readySprite;
    [SerializeField] Sprite thrustSprite;
    [SerializeField] Sprite downStabSprite;
    Sprite apexSprite;
    [SerializeField] float terminalVelocity;
    [SerializeField] float targetRange;
    float leapTime;
    float leapSpeed = 0;
    bool leaping = false;
    [SerializeField] float thrustDuration;
    [SerializeField] float thrustInterval;
    float thrustTime = 0;
    [SerializeField] float projectileSpeed;
    bool justHitShield = false, hasHitShield = false;
    bool landed = false;

    ProjectileLauncher launcher;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        player = gameplayManager.getPlayer().transform;
        apexSprite = downStabSprite;
        leapTime = -2 * terminalVelocity / gameplayManager.getGravity().y;
        launcher = GetComponentInChildren<ProjectileLauncher>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
    protected override void updateIfUnpaused() {
        if (landed) hasHitShield = false;
    }

    protected override bool attackedColliderIntangible(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        if (attackedCollider.name.Equals("Horizontal Pike")) {
            if (!weaponName.Equals("Block") || hasHitShield) return true;
            justHitShield = (hasHitShield = true);
            thrustTime = 0;
            landed = false;
            return false;
        }
        return !(attackedCollider.name.Equals("Hurtbox") || (attackedCollider.name.Equals("Mage Pike(Clone)") && weaponName.Equals("Block")));
    }

    protected override void onAttackedBlock() {
        invincibilityTime = justHitShield ? (shockTime = maxShockTime) : maxInvincibilityTime;
    }
    protected override void onAttackedEffective(int index) {
        if (justHitShield) {
            health -= 2 * damageValues["Block"];
            justHitShield = false;
            return;
        }
        base.onAttackedEffective(index);
    }

    protected override void onActivate() {
        thrustTime = thrustDuration + thrustInterval + thrustDuration;
        thrust();
    }

    protected override void bossBehavior() {
        float diff = transform.position.x - gameplayManager.getMainCameraX();
        if (Mathf.Abs(diff) > 11.53125f)
            transform.position = new Vector3(gameplayManager.getMainCameraX() + Mathf.Sign(diff) * 11.5f, transform.position.y, transform.position.z);
        if (leaping) {
            if (Physics2D.BoxCast(transform.position, new Vector2(0.875f, 0.0625f), 0, Vector2.down, 1, 64) && rb.velocity.y < 0.5f * terminalVelocity) {
                if (renderer.sprite == downStabSprite && player != null) {
                    leapSpeed = (player.position.x + (Random.Range(0, 2) * 2 - 1) * targetRange - transform.position.x) / leapTime;
                    apexSprite = readySprite;
                    rb.velocity = new Vector2(leapSpeed, terminalVelocity);
                    return;
                }
                rb.velocity = Vector2.zero;
                leaping = false;
                landed = true;
                if (player == null || shockTime > 0) return;
                thrustTime = thrustDuration + thrustInterval + thrustDuration;
                thrust();
                return;
            }
            rb.velocity = new Vector2(leapSpeed, rb.velocity.y);
            if (renderer.sprite == apexSprite || rb.velocity.y > 0) return;
            colliders[2].enabled = apexSprite == readySprite;
            colliders[3].enabled = apexSprite == downStabSprite;
            renderer.sprite = apexSprite;
            if (player != null) turn(apexSprite == downStabSprite ? (player.position.x - transform.position.x) : -leapSpeed);
            return;
        }
        if (shockTime > 0 || justHitShield) return;
        if (thrustTime > 0) {
            if (thrustTime <= thrustDuration) {
                if (renderer.sprite != thrustSprite) thrust();
            }
            else if (thrustTime <= thrustDuration + thrustInterval) {
                if (renderer.sprite != readySprite) unthrust();
                if (player == null) thrustTime = 0;
            }
            thrustTime -= Time.deltaTime;
            return;
        }
        if (renderer.sprite == thrustSprite) unthrust();
        if (player == null) return;
        leapSpeed = (player.position.x - transform.position.x) / leapTime;
        apexSprite = downStabSprite;
        rb.velocity = new Vector2(leapSpeed, terminalVelocity);
        leaping = true;
    }
    void turn(float diff) {
        if (Mathf.Sign(diff) != -facing) return;
        facing = -facing;
        renderer.flipX = !renderer.flipX;
        colliders[2].transform.localPosition = new Vector3(-colliders[2].transform.localPosition.x, colliders[2].transform.localPosition.y, colliders[2].transform.localPosition.z);
        colliders[3].transform.localPosition = new Vector3(-colliders[3].transform.localPosition.x, colliders[3].transform.localPosition.y, colliders[3].transform.localPosition.z);
    }
    void thrust() {
        colliders[2].transform.localPosition = new Vector3(facing * 1.75f, colliders[2].transform.localPosition.y, colliders[2].transform.localPosition.z);
        renderer.sprite = thrustSprite;
        EnemyWeapon projectile = launcher.launch(facing, 1).GetComponent<EnemyWeapon>();
        gameplayManager.addProjectile(projectile);
        projectile.setEnemyController();
        projectile.transform.SetParent(null);
        projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * projectileSpeed, 0);
    }
    void unthrust() {
        colliders[2].transform.localPosition = new Vector3(facing * 0.75f, colliders[2].transform.localPosition.y, colliders[2].transform.localPosition.z);
        renderer.sprite = readySprite;
    }

    public override Color getColor() { return Color.green; }
    public override string getInitial() { return "P"; }

    public bool getHitShield() {
        return hasHitShield && !justHitShield;
    }
}
