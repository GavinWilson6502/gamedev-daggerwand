using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SideHopper : EnemyController
{
    [Header("Sprites")]
    [SerializeField] Sprite idle;
    [SerializeField] Sprite squash;
    [SerializeField] Sprite stretch;
    Sprite sprite;
    [SerializeField] float size = 1;
    [Header("Behavior")]
    [SerializeField] float maxHorizontalV;
    [SerializeField] float terminalVelocity;
    Transform player;
    int facing = -1;
    [SerializeField] float maxAttackTime;
    [SerializeField] float landedTime;
    [SerializeField] float telegraphTime;
    float attackTime = 0;
    
    bool grounded = true;
    float horizontalV;
    SpriteRenderer renderer;
    Collider2D[] colliders;

    // Start is called before the first frame update
    void Start()
    {
        if (transform.parent != null) {
            gameplayManager = GetComponentInParent<GameplayManager>();
            transform.SetParent(null);
        }
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        sprite = idle;
        player = gameplayManager.getPlayer().transform;
        renderer = GetComponent<SpriteRenderer>();
        renderer.flipX = facing < 0;
        colliders = GetComponentsInChildren<Collider2D>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
    protected override void updateIfUnpaused() {
        if (Mathf.Abs(rb.velocity.y) > terminalVelocity) rb.velocity = new UnityEngine.Vector2(rb.velocity.x, Mathf.Sign(rb.velocity.y) * terminalVelocity);

        grounded = Physics2D.CircleCast(transform.position, 0.4375f * size - 0.03125f, UnityEngine.Vector2.down, 0.0625f, 64);
        if ((stunTime > 0 || shockTime > 0) && grounded) return;

        bool seesPlayer = false;
        UnityEngine.Vector2 diff = UnityEngine.Vector2.zero;
        if (player != null) {
            diff = player.position - transform.position;
            if (seesPlayer = (Mathf.Abs(diff.y) <= 4 && !Physics2D.Raycast(transform.position, diff.normalized, diff.magnitude, 64)))
                facing = (renderer.flipX = diff.x <= 0) ? -1 : 1;
        }

        if (!grounded) {
            attackTime = maxAttackTime;
            renderer.sprite = stretch;
            colliders[1].enabled = false;
            colliders[2].enabled = false;
            colliders[3].enabled = true;
            return;
        }

        if (attackTime > landedTime || attackTime <= telegraphTime) {
            sprite = squash;
            if (attackTime <= 0) {
                attackTime = maxAttackTime;
                rb.velocity = new UnityEngine.Vector2(horizontalV = facing * (seesPlayer ? getHorizontalV(diff) : maxHorizontalV) * UnityEngine.Random.Range(0.75f, 1f), terminalVelocity);
                return;
            }
        }
        else sprite = idle;
        attackTime -= Time.deltaTime;
        renderer.sprite = sprite;
        colliders[1].enabled = renderer.sprite == idle;
        colliders[2].enabled = renderer.sprite == squash;
        colliders[3].enabled = false;
    }

    protected override void fixedUpdateIfUnpaused() {
        if (Physics2D.Raycast(transform.position, UnityEngine.Vector2.down, 0.4375f * size + 0.03125f, 64)) {
            if (rb.velocity.y <= 0.03125f / Time.deltaTime) rb.velocity = new UnityEngine.Vector2(0, rb.velocity.y);
        }
        else if (Physics2D.Raycast(transform.position, UnityEngine.Vector2.right * facing, 0.4375f * size + 0.03125f, 64)) {
            renderer.flipX = (facing *= -1) < 0;
            rb.velocity = new UnityEngine.Vector2(-horizontalV, rb.velocity.y);
        }
        horizontalV = rb.velocity.x;
    }

    float getHorizontalV(UnityEngine.Vector2 diff) {
        float arcHeight = -terminalVelocity * terminalVelocity / (2 * gameplayManager.getGravity().y);
        float[] roots = quadraticFormula(0.5f * gameplayManager.getGravity().y, terminalVelocity, Mathf.Max(-diff.y, -arcHeight));
        float t;
        if (Single.IsNaN(roots[0]) || Single.IsInfinity(roots[0])) {
            if (Single.IsNaN(roots[1]) || Single.IsInfinity(roots[1])) {
                return maxHorizontalV;
            }
            t = roots[1];
        }
        else if (Single.IsNaN(roots[1]) || Single.IsInfinity(roots[1])) t = roots[0];
        else t = Mathf.Max(roots);
        return Mathf.Min(Mathf.Abs(diff.x) / t, maxHorizontalV);
    }

    public float getSize() {
        return size;
    }
}
