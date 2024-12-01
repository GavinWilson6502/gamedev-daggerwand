using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class BossController : EnemyController
{
    bool active = false;
    protected int facing = -1;
    protected SpriteRenderer renderer;
    protected Collider2D[] colliders;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void fixedUpdateIfUnpaused()
    {
        if (!active) return;
        bossBehavior();
        gameplayManager.updateHUDBoss(Mathf.Max(0, (float)health / maxHealth));
    }

    protected abstract void bossBehavior();

    public void activate() {
        active = true;
        onActivate();
    }

    protected bool getActive() {
        return active;
    }

    protected virtual void onActivate() {}

    public abstract Color getColor();
    public abstract string getInitial();
}
