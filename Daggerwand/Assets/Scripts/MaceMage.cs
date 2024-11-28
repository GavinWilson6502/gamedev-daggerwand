using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaceMage : BossController
{
    bool leaping = false;
    float leapTarget;
    [SerializeField] float terminalVelocity;
    float leapSpeed;
    [SerializeField] Sprite windupSprite;
    [SerializeField] Sprite slamSprite;
    [SerializeField] Sprite waitSprite;
    [SerializeField] Sprite leapingSprite;
    [SerializeField] float maxAttackTime;
    [SerializeField] float slamTime;
    [SerializeField] float waitTime;
    float attackTime = 0;
    int slamsLeft;
    Stack<GameObject> shockwaves = new Stack<GameObject>();
    [SerializeField] float tremorTime;

    Transform player;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        leapSpeed = gameplayManager.getGravity().y / (-2 * terminalVelocity);
        player = gameplayManager.getPlayer().transform;
    }

    // Update is called once per frame
    /*public override void Update()
    {
        
    }*/

    protected override void onActivate() {
        startLeap(gameplayManager.getMainCameraX());
    }

    protected override void bossBehavior() {
        colliders[3].enabled = false;
        colliders[4].enabled = false;
        foreach (GameObject wave in shockwaves) {
            if (wave != null) {
                EnemyShockwave w = wave.GetComponent<EnemyShockwave>();
                if (w != null) w.refresh();
            }
        }
        if (leaping) {
            if (rb.velocity.y > 0.5f * terminalVelocity || !Physics2D.BoxCast(transform.position, new Vector2(1.4375f, 0.0625f), 0, Vector2.down, 1, 64)) return;
            gameplayManager.startTremor(tremorTime);
            foreach (GameObject wave in shockwaves) {
                if (wave != null) Destroy(wave);
            }
            shockwaves.Clear();
            slamsLeft = Random.Range(2, 9);
            attackTime = slamTime;
            leaping = false;
        }
        if (attackTime <= 0) {
            if (slamsLeft <= 0) {
                startLeap(player == null ? leapTarget : Mathf.Clamp(player.position.x, gameplayManager.getMainCameraX() - 9.25f, gameplayManager.getMainCameraX() + 9.25f));
                return;
            }
            colliders[1].enabled = true;
            colliders[2].enabled = false;
            facing = -facing;
            renderer.flipX = !renderer.flipX;
            renderer.sprite = windupSprite;
            attackTime = maxAttackTime;
        }
        else if (attackTime <= waitTime) {
            if (renderer.sprite != waitSprite) renderer.sprite = waitSprite;
        }
        else if (attackTime <= slamTime && renderer.sprite != slamSprite) {
            colliders[1].enabled = false;
            colliders[2].enabled = true;
            Collider2D maceSlam = colliders[facing > 0 ? 3 : 4];
            maceSlam.enabled = true;
            shockwaves.Push(maceSlam.GetComponent<ProjectileLauncher>().launch(facing, 1));
            gameplayManager.addProjectile(shockwaves.Peek().GetComponent<EnemyWeapon>());
            renderer.sprite = slamSprite;
            --slamsLeft;
        }
        attackTime -= Time.deltaTime;
    }

    void startLeap(float target) {
        leapTarget = target;
        float diff = leapTarget - transform.position.x;
        if (diff < 0) {
            facing = -1;
            renderer.flipX = true;
        }
        else if (diff > 0) {
            facing = 1;
            renderer.flipX = false;
        }
        colliders[1].enabled = true;
        colliders[2].enabled = false;
        renderer.sprite = leapingSprite;
        rb.velocity = new Vector2(diff * leapSpeed, terminalVelocity);
        leaping = true;
        attackTime = maxAttackTime;
    }

    protected override bool attackedColliderIntangible(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return !(attackedCollider.name.Equals("Hurtbox") || weaponName.Equals("Block") || (attackedCollider.name.Equals("Mace Slam") && weaponName.Equals("Ember")));
    }
    protected override bool attackedColliderBlocks(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return attackedCollider.name.Equals("Mace Slam") && weaponName.Equals("Ember");
    }

    public override Color getColor() { return new Color(0, 0.25f, 1); }
    public override string getInitial() { return "M"; }
}
