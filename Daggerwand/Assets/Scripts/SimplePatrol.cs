using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimplePatrol : EnemyController
{
    [SerializeField] float speed;
    Vector2 wallExtents = Vector2.zero;
    Vector2 ledgeExtents = Vector2.zero;
    Vector2 resumeVel;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = new Vector2(speed, 0);
        Collider2D[] colliders = GetComponents<Collider2D>();
        wallExtents = colliders[0].bounds.extents;
        ledgeExtents = colliders[1].bounds.extents;
        resumeVel = new Vector2(speed, 0);
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void fixedUpdateIfUnpaused() {
        if (stunTime > 0 || shockTime > 0) {
            if (rb.velocity.x != 0) {
                resumeVel = rb.velocity;
                rb.velocity = Vector2.zero;
            }
            return;
        }
        rb.velocity = resumeVel;
        if (Physics2D.Raycast(
                transform.position + new Vector3(Mathf.Sign(rb.velocity.x) * wallExtents.x, 0.03125f - wallExtents.y),
                rb.velocity.normalized,
                rb.velocity.magnitude * Time.deltaTime,
                64
            ) || Physics2D.Raycast(
                transform.position + (Vector3)rb.velocity * Time.deltaTime + new Vector3(Mathf.Sign(rb.velocity.x) * ledgeExtents.x, -0.03125f - ledgeExtents.y),
                -rb.velocity.normalized,
                rb.velocity.magnitude * Time.deltaTime,
                64
            )) {
            rb.velocity *= -1;
            resumeVel *= -1;
        }
    }
}
