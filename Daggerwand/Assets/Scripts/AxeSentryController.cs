using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AxeSentryController : EnemyController
{
    Transform player;
    int facing = -1;
    [SerializeField] float maxAttackTime;
    [SerializeField] float throwTime;
    [SerializeField] float reloadTime;
    float attackTime = 0;
    string animation = "Holding";
    [SerializeField] float initialVerticalV;
    [SerializeField] float maxHorizontalV;

    ProjectileLauncher launcher;
    Rigidbody2D projectile = null;
    SpriteRenderer renderer;
    
    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        player = gameplayManager.getPlayer().transform;
        launcher = GetComponentInChildren<ProjectileLauncher>();
        renderer = GetComponent<SpriteRenderer>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void fixedUpdateIfUnpaused() {
        if (stunTime > 0 || shockTime > 0) return;
        if (player != null) turn((int)Mathf.Sign(player.position.x - transform.position.x));
        if (attackTime <= 0) {
            animation = "Holding";
            projectile = launcher.launch(facing, 1).GetComponent<Rigidbody2D>();
            projectile.GetComponent<EnemyWeapon>().setHeld(true);
            gameplayManager.addProjectile(projectile.GetComponent<EnemyWeapon>());
            attackTime = maxAttackTime;
        }
        else if (attackTime <= reloadTime) {
            if (!animation.Equals("Reloading")) animation = "Reloading";
        }
        else if (attackTime <= throwTime && !animation.Equals("Throwing")) {
            animation = "Throwing";
            if (projectile != null) {
                projectile.GetComponent<EnemyWeapon>().setHeld(false);
                projectile.transform.SetParent(null);
                projectile.velocity = new Vector2(getHorizontalV(), initialVerticalV);
                projectile.bodyType = RigidbodyType2D.Dynamic;
                projectile = null;
            }
        }
        attackTime -= Time.deltaTime;
    }

    void turn(int direction) {
        if (direction == facing) return;
        if (direction < 0) renderer.flipX = false;
        else if (direction > 0) renderer.flipX = true;
        facing = direction;
        launcher.transform.localPosition = new Vector2(facing * -1.125f, launcher.transform.localPosition.y);
    }

    float getHorizontalV() {
        if (player == null) return facing * maxHorizontalV;
        float arcHeight = -initialVerticalV * initialVerticalV / (2 * gameplayManager.getGravity().y);
        float[] roots = quadraticFormula(0.5f * gameplayManager.getGravity().y, initialVerticalV, Mathf.Max(launcher.transform.position.y - player.position.y, -arcHeight));
        float t;
        if (Single.IsNaN(roots[0]) || Single.IsInfinity(roots[0])) {
            if (Single.IsNaN(roots[1]) || Single.IsInfinity(roots[1])) {
                return facing * maxHorizontalV;
            }
            t = roots[1];
        }
        else if (Single.IsNaN(roots[1]) || Single.IsInfinity(roots[1])) t = roots[0];
        else t = Mathf.Max(roots);
        return facing * Mathf.Min(Mathf.Abs(player.position.x - launcher.transform.position.x) / t, maxHorizontalV);
    }
}
