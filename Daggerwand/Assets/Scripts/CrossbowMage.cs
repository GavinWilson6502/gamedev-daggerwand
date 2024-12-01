using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrossbowMage : BossController
{
    [SerializeField] float walkSpeed;
    [SerializeField] float attackInterval;
    float attackTime = 0;
    [SerializeField] int maxVolleysBeforeCrossing;
    int volleysBeforeCrossing;
    float introWaitStartTime = 0;
    [SerializeField] float introWaitTime;
    Transform[] bolts = new Transform[3];
    int boltIndex = 0;
    [SerializeField] float boltSpeed;
    bool crossing = false;

    Transform player;
    ProjectileLauncher launcher;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        volleysBeforeCrossing = maxVolleysBeforeCrossing;
        player = gameplayManager.getPlayer().transform;
        launcher = GetComponentInChildren<ProjectileLauncher>();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override bool attackedColliderIntangible(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return !getActive() || base.attackedColliderIntangible(weaponName, projectile, direction, attackedCollider);
    }

    protected override void onAttackedEffective(int index) {
        base.onAttackedEffective(index);
        crossing = true;
    }

    protected override void bossBehavior() {
        if (crossing && introWaitStartTime > 0 && Time.time - introWaitStartTime >= introWaitTime) {
            float progress = facing * (transform.position.x - gameplayManager.getMainCameraX());
            bool cast = Physics2D.BoxCast(transform.position, new Vector2(0.9375f, 0.0625f), 0, Vector2.down, 1, 64);
            if (progress >= 11.5f && cast) {
                rb.velocity = Vector2.zero;
                facing = -facing;
                renderer.flipX = !renderer.flipX;
                launcher.transform.localPosition = new Vector3(-launcher.transform.localPosition.x, launcher.transform.localPosition.y, launcher.transform.localPosition.z);
                boltIndex = 0;
                attackTime = 0;
                volleysBeforeCrossing = maxVolleysBeforeCrossing;
                crossing = false;
                return;
            }
            float temp;
            if (!cast || rb.velocity.y > 0.5f * (temp = Mathf.Sqrt(-12 * gameplayManager.getGravity().y) + 1))
                rb.velocity = new Vector2(facing * Mathf.Sqrt(gameplayManager.getGravity().y / -48), rb.velocity.y);
            else if (progress >= 10 && progress < 10.5f) rb.velocity = new Vector2(facing * Mathf.Sqrt(gameplayManager.getGravity().y / -48), temp);
            else rb.velocity = new Vector2(facing * walkSpeed, 0);
            return;
        }
        if (Mathf.Abs(transform.position.x - gameplayManager.getMainCameraX()) > 11.5f) {
            transform.position += new Vector3(Mathf.Sign(gameplayManager.getMainCameraX() - transform.position.x) * walkSpeed * Time.deltaTime, 0);
            if (Mathf.Abs(transform.position.x - gameplayManager.getMainCameraX()) < 11.5f) {
                transform.position = new Vector3(gameplayManager.getMainCameraX() + Mathf.Sign(transform.position.x - gameplayManager.getMainCameraX()) * 11.5f, transform.position.y, transform.position.z);
                if (introWaitStartTime == 0) introWaitStartTime = Time.time;
            }
            return;
        }
        if (player == null) return;
        aim(boltIndex != 1);
        if (attackTime > 0) {
            attackTime -= Time.deltaTime;
            return;
        }
        if ((boltIndex == 0 && (bolts[0] != null || bolts[1] != null || bolts[2] != null)) || Time.time - introWaitStartTime < introWaitTime) return;
        shoot(boltIndex);
        if (++boltIndex > 2) {
            boltIndex = 0;
            if (--volleysBeforeCrossing <= 0) crossing = true;
        }
        else attackTime = attackInterval;
    }

    void aim(bool lead) {
        launcher.transform.rotation = Quaternion.identity;
        float a = player.position.x - launcher.transform.position.x;
        float o = gameplayManager.getMainCameraY() - 5 - launcher.transform.position.y;
        float h = Mathf.Sqrt(a * a + o * o);
        launcher.transform.Rotate(0, 0, Mathf.Rad2Deg * (Mathf.Atan2(o, a) - (lead ? Mathf.Asin(o / h * player.GetComponent<Rigidbody2D>().velocity.x / boltSpeed) : 0)));
    }

    void shoot(int index) {
        bolts[index] = launcher.launch(facing, 1).transform;
        gameplayManager.addProjectile(bolts[index].GetComponent<EnemyWeapon>());
        bolts[index].SetParent(null);
        bolts[index].GetComponent<Rigidbody2D>().velocity = bolts[index].TransformVector(new Vector2(boltSpeed, 0));
    }

    public override Color getColor() { return new Color(1, 0.5f, 0); }
    public override string getInitial() { return "C"; }
}
