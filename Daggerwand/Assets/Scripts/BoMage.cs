using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BoMage : BossController
{
    Transform player;
    [SerializeField] Sprite standingSprite;
    [SerializeField] Sprite kickSprite;
    [SerializeField] Sprite twirlSprite;
    [SerializeField] Sprite jabSprite;
    [SerializeField] Sprite sideStrikeSprite;
    [SerializeField] Sprite highStrikeSprite;
    [SerializeField] Sprite lowStrikeSprite;
    [SerializeField] float midRange;
    [SerializeField] float outerCloseQuarters;
    [SerializeField] float innerCloseQuarters;
    [SerializeField] float walkSpeed;
    [SerializeField] float terminalVelocity;
    [SerializeField] float kickSpeed;
    bool kicking = false, outerFlurry = false, innerFlurry = false;
    [SerializeField] float maxAttackTime;
    float attackTime;
    [SerializeField] float maxAttackStepTime;
    float attackStepTime;
    int flurryCollider = 5;
    bool parried = false;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; ++i) {
            EnemyWeapon weapon = colliders[i].GetComponent<EnemyWeapon>();
            if (weapon == null) continue;
            weapon.setEnemyController();
            if (i > 4) weapon.setDisabled(true);
        }
        player = gameplayManager.getPlayer().transform;
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    /*void OnCollisionEnter2D(Collision2D other) {
        if (rb.bodyType == RigidbodyType2D.Dynamic) return;
        renderer.sprite = standingSprite;
        colliders[1].enabled = true;
        colliders[2].enabled = false;
        colliders[3].enabled = false;
        rb.bodyType = RigidbodyType2D.Dynamic;
        rb.velocity = Vector2.zero;
    }*/

    protected override bool attackedColliderIntangible(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return !attackedCollider.name.Equals("Hurtbox") && (projectile || weaponName.Equals("Block") || attackedCollider.name.Equals("Bo Twirl"));
    }
    protected override bool attackedColliderBlocks(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        if (!(attackedCollider.name.Equals("Hurtbox") || projectile || attackedCollider.name.Equals("Bo Twirl"))) return parried = true;
        return false;
    }

    protected override void bossBehavior() {
        if (Mathf.Abs(transform.position.y - gameplayManager.getMainCameraY()) > 5.25 || Mathf.Abs(transform.position.x - gameplayManager.getMainCameraX()) > 11.75f) {
            if (rb.bodyType != RigidbodyType2D.Dynamic) {
                renderer.sprite = standingSprite;
                colliders[1].enabled = true;
                colliders[2].enabled = false;
                colliders[3].enabled = false;
                rb.bodyType = RigidbodyType2D.Dynamic;
            }
        }
        bool grounded = Physics2D.BoxCast(transform.position, new Vector2(0.9375f, 0.0625f), 0, Vector2.down, 1, 64);
        if (kicking) {
            if (!grounded) return;
            if (player != null) turn(player.position.x - transform.position.x);
            kicking = false;
            return;
        }
        if (outerFlurry) {
            if (attackTime > 0) {
                if (!parried && attackTime <= Time.fixedDeltaTime) colliders[flurryCollider].GetComponent<EnemyWeapon>().setDisabled(false);
                attackTime -= Time.fixedDeltaTime;
                return;
            }
            renderer.sprite = standingSprite;
            colliders[flurryCollider].GetComponent<EnemyWeapon>().setDisabled(true);
            colliders[flurryCollider].enabled = false;
            parried = false;
            if (attackStepTime > 0) {
                rb.velocity = new Vector2(facing / maxAttackStepTime, rb.velocity.y);
                attackStepTime -= Time.fixedDeltaTime;
                return;
            }
            rb.velocity = Vector2.zero;
            if (++flurryCollider > 7) {
                if (player != null) turn(player.position.x - transform.position.x);
                outerFlurry = false;
                return;
            }
            if (flurryCollider > 6) {
                if (facing < 0) ++flurryCollider;
                renderer.sprite = highStrikeSprite;
            }
            else {
                renderer.sprite = sideStrikeSprite;
                attackStepTime = maxAttackStepTime;
            }
            colliders[flurryCollider].enabled = true;
            attackTime = maxAttackTime;
            return;
        }
        if (innerFlurry) {
            if (attackTime > 0) {
                if (!parried && attackTime <= Time.fixedDeltaTime) colliders[flurryCollider].GetComponent<EnemyWeapon>().setDisabled(false);
                attackTime -= Time.fixedDeltaTime;
                return;
            }
            colliders[flurryCollider].GetComponent<EnemyWeapon>().setDisabled(true);
            colliders[flurryCollider].enabled = false;
            parried = false;
            if ((flurryCollider -= 2) < 7) {
                renderer.sprite = standingSprite;
                if (player != null) turn(player.position.x - transform.position.x);
                innerFlurry = false;
                return;
            }
            renderer.sprite = highStrikeSprite;
            colliders[flurryCollider].enabled = true;
            attackTime = maxAttackTime;
            return;
        }
        if (player == null) return;
        if (!grounded || rb.velocity.y > 0.5f * terminalVelocity) {
            if (rb.velocity.y > 0) return;
            renderer.sprite = kickSprite;
            colliders[1].enabled = false;
            colliders[facing > 0 ? 2 : 3].enabled = true;
            rb.bodyType = RigidbodyType2D.Kinematic;
            rb.velocity = (player.position - transform.position).normalized * kickSpeed;
            kicking = true;
            return;
        }
        if (Mathf.Abs(transform.position.x - player.position.x) > midRange) {
            renderer.sprite = standingSprite;
            for (int i = 4; i < colliders.Length; ++i) colliders[i].enabled = false;
            rb.velocity = new Vector2(0, terminalVelocity);
        }
        else if (Mathf.Abs(transform.position.x - player.position.x) > outerCloseQuarters) {
            renderer.sprite = twirlSprite;
            colliders[4].enabled = true;
            for (int i = 5; i < colliders.Length; ++i) colliders[i].enabled = false;
            rb.velocity = new Vector2(facing * walkSpeed, rb.velocity.y);
        }
        else if (Mathf.Abs(transform.position.x - player.position.x) > innerCloseQuarters) {
            renderer.sprite = jabSprite;
            colliders[4].enabled = false;
            colliders[flurryCollider = 5].enabled = true;
            rb.velocity = Vector2.zero;
            attackTime = maxAttackTime;
            attackStepTime = maxAttackStepTime;
            outerFlurry = true;
            return;
        }
        else {
            renderer.sprite = lowStrikeSprite;
            colliders[4].enabled = false;
            colliders[flurryCollider = facing > 0 ? 9 : 10].enabled = true;
            rb.velocity = Vector2.zero;
            attackTime = maxAttackTime;
            innerFlurry = true;
            return;
        }
    }
    void turn(float diff) {
        if (diff < 0) {
            facing = -1;
            renderer.flipX = true;
        }
        else if (diff > 0) {
            facing = 1;
            renderer.flipX = false;
        }
        colliders[4].transform.localPosition = new Vector3(facing * 0.75f, colliders[4].transform.localPosition.y, colliders[4].transform.localPosition.z);
        colliders[5].transform.localPosition = new Vector3(facing * 1.5f, colliders[5].transform.localPosition.y, colliders[5].transform.localPosition.z);
        colliders[6].transform.localPosition = new Vector3(facing * 0.5f, colliders[6].transform.localPosition.y, colliders[6].transform.localPosition.z);
    }

    public override Color getColor() { return new Color(0.5f, 0.25f, 0); }
    public override string getInitial() { return "B"; }
}
