using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyArmedPatrolController : EnemyController
{
    [SerializeField] float patrolDistance;
    [SerializeField] float speed;
    [SerializeField] Vector2 direction;
    float maxPatrolTime;
    float patrolTime;

    Collider2D[] colliders;

    // Start is called before the first frame update
    void Start()
    {
        maxPatrolTime = patrolDistance / speed;
        patrolTime = maxPatrolTime;
        colliders = GetComponentsInChildren<Collider2D>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/
    protected override void updateIfUnpaused() {
        colliders[2].enabled = stunTime <= 0;
    }

    protected override void fixedUpdateIfUnpaused() {
        if (stunTime > 0 || shockTime > 0) return;
        if (patrolTime <= 0) {
            direction *= -1;
            patrolTime = maxPatrolTime;
        }
        transform.localPosition += (Vector3)(direction * speed * Time.deltaTime);
        patrolTime -= Time.deltaTime;
    }

    protected override bool attackedColliderIntangible(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return !(attackedCollider.name.Equals("Hurtbox") || attackedCollider.name.Equals("Shield") || weaponName.Equals("Block"));
    }
    protected override bool attackedColliderBlocks(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return attackedCollider.name.Equals("Shield") && !weaponName.Equals("Twirl");
    }
}
