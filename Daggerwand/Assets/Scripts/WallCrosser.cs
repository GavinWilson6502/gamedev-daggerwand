using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WallCrosser : EnemyController
{
    int facing = 1;
    [SerializeField] float maxAttackTime;
    float attackTime;
    [SerializeField] float flySpeed;
    [SerializeField] float projectileSpeed;
    Vector2 resumeVel = Vector2.zero;

    SpriteRenderer renderer;
    ProjectileLauncher launcher;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        attackTime = maxAttackTime;
        renderer = GetComponent<SpriteRenderer>();
        if (flySpeed < 0) {
            facing = -1;
            renderer.flipX = true;
            flySpeed = Mathf.Abs(flySpeed);
        }
        launcher = GetComponentInChildren<ProjectileLauncher>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void fixedUpdateIfUnpaused(){
        if (stunTime > 0 || shockTime > 0) {
            if (rb.velocity.x != 0) {
                resumeVel = rb.velocity;
                rb.velocity = Vector2.zero;
            }
            return;
        }
        rb.velocity = resumeVel;
        if (attackTime <= 0 && rb.velocity.x == 0) {
            facing *= -1;
            rb.velocity = new Vector2(facing * flySpeed, 0);
            resumeVel = rb.velocity;
            renderer.flipX = !renderer.flipX;
            EnemyWeapon projectile = launcher.launch(facing, 1).GetComponent<EnemyWeapon>();
            projectile.GetComponent<SpriteRenderer>().flipX = facing < 0;
            projectile.GetComponent<Rigidbody2D>().velocity = new Vector2(facing * projectileSpeed, 0);
            projectile.transform.SetParent(null);
            gameplayManager.addProjectile(projectile);
        }
        attackTime -= Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (attackTime > 0 || other.gameObject.layer != 6) return;
        float xExtent = GetComponent<Collider2D>().bounds.extents.x;
        transform.position = new Vector2((rb.velocity.x < 0 ? (Mathf.Floor(transform.position.x) + xExtent) : (Mathf.Ceil(transform.position.x) - xExtent)), transform.position.y);
        rb.velocity = Vector2.zero;
        resumeVel = Vector2.zero;
        attackTime = maxAttackTime;
    }
}
