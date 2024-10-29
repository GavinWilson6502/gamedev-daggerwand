using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InvincibleEnemyController : EnemyController
{
    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override bool attackedColliderBlocks(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return true;
    }
}
