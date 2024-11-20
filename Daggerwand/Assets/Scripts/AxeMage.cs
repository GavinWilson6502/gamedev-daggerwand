using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeMage : BossController
{
    bool jumping = false;
    float moveTarget;
    [SerializeField] float terminalVelocity;
    float jumpSpeed;
    [SerializeField] float walkSpeed;
    [SerializeField] Sprite walkingHolding;
    [SerializeField] Sprite walkingSwing;
    Sprite walkingSprite;
    [SerializeField] Sprite jumpingHolding;
    [SerializeField] Sprite jumpingSwing;
    Sprite jumpingSprite;
    [SerializeField] float maxAttackTime;
    [SerializeField] float swingTime;
    float attackTime = 0;
    [SerializeField] Vector2 throwVelocity;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        // t = -2 vy0 / a
        // vx = 1 / t = a / (-2 vy0)
        jumpSpeed = gameplayManager.getGravity().y / (-2 * terminalVelocity);
        walkingSprite = walkingHolding;
        jumpingSprite = jumpingHolding;
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void onActivate() {
        setMoveTarget();
    }

    protected override void bossBehavior() {
        colliders[2].enabled = false;
        if (jumping) {
            if (rb.velocity.y <= 0.5f * terminalVelocity && Physics2D.BoxCast(transform.position, new Vector2(0.9375f, 0.0625f), 0, Vector2.down, 1, 64)) jumping = false;
            else rb.velocity = new Vector2(facing * jumpSpeed, rb.velocity.y);
        }
        if (!jumping) {
            if ((facing * (transform.position.x - gameplayManager.getMainCameraX()) % 3 + 3) % 3 >= 2.5f) {
                rb.velocity = new Vector2(facing * jumpSpeed, terminalVelocity);
                jumping = true;
            }
            else rb.velocity = new Vector2(facing * walkSpeed, rb.velocity.y);
        }
        if (facing * (moveTarget - transform.position.x) <= 0) setMoveTarget();
        
        if (attackTime <= 0) {
            walkingSprite = walkingHolding;
            jumpingSprite = jumpingHolding;
            attackTime = maxAttackTime;
        }
        else if (attackTime <= swingTime && walkingSprite != walkingSwing) {
            walkingSprite = walkingSwing;
            jumpingSprite = jumpingSwing;
            colliders[2].enabled = true;
            EnemyWeapon projectile = colliders[2].GetComponent<ProjectileLauncher>().launch(facing, 1).GetComponent<EnemyWeapon>();
            gameplayManager.addProjectile(projectile);
            projectile.transform.SetParent(null);
            projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * throwVelocity.x, throwVelocity.y);
        }
        attackTime -= Time.deltaTime;
        renderer.sprite = jumping ? jumpingSprite : walkingSprite;
    }

    void setMoveTarget() {
        int limit = Mathf.CeilToInt(gameplayManager.getMainCameraWidth()) - 2;
        moveTarget = gameplayManager.getMainCameraX() + Random.Range(-limit, limit) + 0.5f;
        if (moveTarget - transform.position.x < 0) {
            facing = -1;
            renderer.flipX = true;
        }
        else if (moveTarget - transform.position.x > 0) {
            facing = 1;
            renderer.flipX = false;
        }
    }

    public override Color getColor() { return new Color(1, 0, 0); }
    public override string getInitial() { return "A"; }
}
