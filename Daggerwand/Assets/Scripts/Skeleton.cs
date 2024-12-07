using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Skeleton : EnemyController
{
    Transform player;
    Vector2 home;
    [SerializeField] float walkSpeed;
    [SerializeField] float terminalVelocity;
    [SerializeField] float attackRange;
    [SerializeField] int maxShotsBeforeMelee;
    int shotsBeforeMelee;
    bool melee = false;
    bool crouching = false;
    float jumpHeight = Mathf.Infinity;
    float airSpeed = 0;
    int facing = -1;
    [SerializeField] Sprite standingSprite;
    [SerializeField] Sprite crouchingSprite;
    [SerializeField] float projectileSpeed;

    SpriteRenderer renderer;
    Collider2D[] colliders;
    ProjectileLauncher launcher;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        player = gameplayManager.getPlayer().transform;
        home = transform.position;
        shotsBeforeMelee = maxShotsBeforeMelee;
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        launcher = GetComponentInChildren<ProjectileLauncher>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void fixedUpdateIfUnpaused() {
        if (rb.velocity.y < -terminalVelocity) rb.velocity = new Vector2(rb.velocity.x, -terminalVelocity);
        if (Mathf.Abs(transform.position.x - gameplayManager.getMainCameraX()) > gameplayManager.getMainCameraWidth() + 0.5f
         || Mathf.Abs(transform.position.y - gameplayManager.getMainCameraY()) > gameplayManager.getMainCameraHeight() + 1) {
            transform.position = home;
            rb.velocity = Vector2.zero;
            crouch(false);
            jumpHeight = Mathf.Infinity;
            airSpeed = 0;
            if (player != null) turn((int)Mathf.Sign(player.position.x - transform.position.x));
            return;
        }
        if (stunTime > 0 || shockTime > 0) return;
        if (isJumping()) {
            rb.velocity = new Vector2(airSpeed, rb.velocity.y);
            if (transform.position.y > jumpHeight) {
                transform.position = new Vector2(transform.position.x, jumpHeight);
                rb.velocity = new Vector2(airSpeed, 0);
                shoot();
            }
            if (rb.velocity.y <= 0) {
                crouch(false);
                jumpHeight = Mathf.Infinity;
            }
            return;
        }
        if (rb.velocity.y <= 0) {
            crouch(false);
            jumpHeight = Mathf.Infinity;
        }
        if (player == null) return;
        turn((int)Mathf.Sign(player.position.x - transform.position.x));
        if (shotsBeforeMelee <= 0) {
            float tempRange = player.position.x - transform.position.x;
            float t;
            if (melee = !melee) t = (-terminalVelocity - Mathf.Sqrt(terminalVelocity * terminalVelocity + 2 * gameplayManager.getGravity().y * (player.position.y - transform.position.y))) / gameplayManager.getGravity().y;
            else {
                tempRange -= Mathf.Sign(tempRange) * attackRange;
                t = (-terminalVelocity - Mathf.Abs(terminalVelocity)) / gameplayManager.getGravity().y;
                shotsBeforeMelee = maxShotsBeforeMelee;
            }
            airSpeed = (float.IsNaN(t) || float.IsInfinity(t)) ? (Mathf.Sign(tempRange) * walkSpeed) : (tempRange / t);
            rb.velocity = new Vector2(airSpeed, terminalVelocity);
            return;
        }
        if (Random.Range(0, 2) > 0) crouch(true);
        else if (Mathf.Abs(player.position.x - transform.position.x) > attackRange) airSpeed = Mathf.Sign(player.position.x - transform.position.x) * walkSpeed;
        jumpHeight = transform.position.y + Random.Range(0f, 4f);
        rb.velocity = new Vector2(airSpeed, terminalVelocity);
    }

    bool isJumping() {
        return rb.velocity.y >= 0.5f * terminalVelocity
            || !Physics2D.BoxCast(transform.position, new Vector2(0.875f, 0.0625f), 0, Vector2.down, 1, 64);
    }

    void shoot() {
        Rigidbody2D projectile = launcher.launch(facing, 1).GetComponent<Rigidbody2D>();
        gameplayManager.addProjectile(projectile.GetComponent<EnemyWeapon>());
        projectile.transform.SetParent(null);
        projectile.bodyType = RigidbodyType2D.Kinematic;
        projectile.velocity = new Vector2(facing * projectileSpeed, 0);
        --shotsBeforeMelee;
    }

    void crouch(bool shouldCrouch) {
        crouching = shouldCrouch;
        renderer.sprite = crouching ? crouchingSprite : standingSprite;
        colliders[1].enabled = !crouching;
        colliders[2].enabled = crouching;
        launcher.transform.localPosition = new Vector2(launcher.transform.localPosition.x, crouching ? -0.5f : 0.5f);
        if (crouching) airSpeed = 0;
    }

    void turn(int dir) {
        if (dir > 0) {
            facing = 1;
            renderer.flipX = false;
        }
        if (dir < 0) {
            facing = -1;
            renderer.flipX = true;
        }
        launcher.transform.localPosition = new Vector2(facing * 0.5f, launcher.transform.localPosition.y);
    }
}
