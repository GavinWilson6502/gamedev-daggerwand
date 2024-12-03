using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShieldMage : BossController
{
    Transform player;
    [SerializeField] Sprite highBlockSprite;
    [SerializeField] Sprite lowBlockSprite;
    [SerializeField] Sprite highGuardBrokenSprite;
    [SerializeField] Sprite lowGuardBrokenSprite;
    Sprite blockSprite;
    Sprite guardBrokenSprite;
    [SerializeField] float maxLowBlockTime;
    float lowBlockTime = 0;
    [SerializeField] float maxGuardBrokenTime;
    float guardBrokenTime = 0;
    [SerializeField] float walkSpeed;
    [SerializeField] float chargeSpeed;
    [SerializeField] float terminalVelocity;
    float leapSpeed;
    bool leaping = false;
    float maxShortChargeTime;
    float shortChargeTime = 0;
    bool shortCharge = false;
    bool longCharge = false;
    [SerializeField] float longChargeRange;
    [SerializeField] float shortChargeRange;
    int nextAttack = 2;
    [SerializeField] float maxTelegraphTime;
    float telegraphTime;
    [SerializeField] float maxStopTime;
    float stopTime = 0;

    Collider2D lowBlockCollider, highBlockCollider;

    List<string> hitShield = new List<string>();
    List<string> missedShield = new List<string>();

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
        renderer = GetComponent<SpriteRenderer>();
        colliders = GetComponentsInChildren<Collider2D>();
        player = gameplayManager.getPlayer().transform;
        leapSpeed = -11 * gameplayManager.getGravity().y / terminalVelocity;
        maxShortChargeTime = 4 * shortChargeRange / chargeSpeed;
        lowBlockCollider = colliders[1];
        highBlockCollider = colliders[2];
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override bool attackedColliderIntangible(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return false;
    }
    protected override bool attackedColliderBlocks(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        if (Mathf.Sign(direction.x) != facing) {
            if (weaponName.Equals("Twirl")) guardBrokenTime = maxGuardBrokenTime;
            bool hasHitShield = false;
            foreach (string s in hitShield) {
                if (hasHitShield = weaponName.Equals(s)) break;
            }
            if (!hasHitShield) {
                if ((attackedCollider == colliders[1] && lowBlockTime <= 0)
                 || ((attackedCollider == colliders[2] || attackedCollider == colliders[3]) && lowBlockTime > 0)) missedShield.Add(string.Copy(weaponName));
                else if (attackedCollider == colliders[1] || attackedCollider == colliders[2] || attackedCollider == colliders[3]) {
                    hitShield.Add(string.Copy(weaponName));
                    string toRemove = "Unmatchable";
                    foreach (string s in missedShield) {
                        if (weaponName.Equals(s)) {
                            toRemove = s;
                            break;
                        }
                    }
                    if (!toRemove.Equals("Unmatchable")) missedShield.Remove(toRemove);
                }
            }
        }
        return Mathf.Sign(direction.x) != facing && guardBrokenTime <= 0;
    }

    protected override void onAttackedTwirl() {
        rb.velocity = Vector2.zero;
        base.onAttackedTwirl();
    }

    protected override void bossBehavior() {
        if (Mathf.Abs(transform.position.x - gameplayManager.getMainCameraX()) >= 11.5f)
            transform.position = new Vector3(gameplayManager.getMainCameraX() + Mathf.Sign(transform.position.x - gameplayManager.getMainCameraX()) * 11.25f, transform.position.y, transform.position.z);
        if (Mathf.Abs(transform.position.y - gameplayManager.getMainCameraY()) >= 5.25f)
            transform.position = new Vector3(transform.position.x, gameplayManager.getMainCameraY() + Mathf.Sign(transform.position.y - gameplayManager.getMainCameraY()) * 5, transform.position.z);
        if (guardBrokenTime > 0) guardBrokenTime -= Time.deltaTime;
        if (player != null && stunTime <= 0) {
            if (missedShield.Count > 0) lowBlockTime = (lowBlockTime <= 0) ? maxLowBlockTime : 0;
            hitShield.Clear();
            missedShield.Clear();
            if (lowBlockTime > 0) lowBlockTime -= Time.deltaTime;
            if (lowBlockTime > 0) {
                colliders[2].enabled = false;
                colliders[3].enabled = true;
                guardBrokenSprite = lowGuardBrokenSprite;
                blockSprite = lowBlockSprite;
                shortCharge = false;
                longCharge = false;
                if (leaping) duringLeap();
                else {
                    rb.velocity = new Vector2(0, rb.velocity.y);
                    if (Mathf.Sign(transform.position.x - player.position.x) == facing) {
                        facing = -facing;
                        renderer.flipX = !renderer.flipX;
                    }
                }
            }
            else {
                colliders[2].enabled = true;
                colliders[3].enabled = false;
                guardBrokenSprite = highGuardBrokenSprite;
                blockSprite = highBlockSprite;
                if (leaping) duringLeap();
                else if (shortCharge) {
                    if (shortChargeTime > 0) {
                        rb.velocity = Vector2.Lerp(Vector2.zero, new Vector2(facing * chargeSpeed, rb.velocity.y), shortChargeTime / maxShortChargeTime);
                        shortChargeTime -= Time.deltaTime;
                    }
                    else if (stopTime > 0) {
                        rb.velocity = new Vector2(0, rb.velocity.y);
                        stopTime -= Time.deltaTime;
                    }
                    else {
                        if (facing * (player.position.x - transform.position.x) < 0) {
                            facing = -facing;
                            renderer.flipX = !renderer.flipX;
                        }
                        shortCharge = false;
                    }
                }
                else if (longCharge) {
                    if (telegraphTime > 0) telegraphTime -= Time.deltaTime;
                    else {
                        if (facing * (transform.position.x - gameplayManager.getMainCameraX()) < 11) 
                            rb.velocity = new Vector2(facing * chargeSpeed, rb.velocity.y);
                        else {
                            rb.velocity = Vector2.zero;
                            if (facing * (player.position.x - transform.position.x) < 0) {
                                facing = -facing;
                                renderer.flipX = !renderer.flipX;
                            }
                            longCharge = false;
                        }
                    }
                }
                else if (nextAttack <= 0) {
                    if (Mathf.Abs(transform.position.x - player.position.x) > shortChargeRange)
                        rb.velocity = new Vector2(facing * walkSpeed, rb.velocity.y);
                    else {
                        startShortCharge();
                        nextAttack = Random.Range(0, 2);
                    }
                }
                else if (nextAttack == 1) {
                    if (Mathf.Sign(transform.position.x - player.position.x) == facing) {
                        facing = -facing;
                        renderer.flipX = !renderer.flipX;
                    }
                    float diff = Mathf.Abs(transform.position.x - player.position.x);
                    if (Mathf.Abs(diff - longChargeRange) <= walkSpeed * Time.deltaTime) {
                        startLongCharge();
                        nextAttack = Random.Range(0, 2);
                    }
                    else if (diff > longChargeRange)
                        rb.velocity = new Vector2(facing * walkSpeed, rb.velocity.y);
                    else if (Mathf.Abs(player.position.x - facing * longChargeRange - gameplayManager.getMainCameraX()) > 11
                          && Mathf.Abs(transform.position.x - gameplayManager.getMainCameraX()) >= 11) {
                        rb.velocity = new Vector2(facing * leapSpeed, terminalVelocity);
                        leaping = true;
                    }
                    else
                        rb.velocity = new Vector2(-facing * walkSpeed, rb.velocity.y);
                }
                else if (nextAttack > 1) {
                    startLongCharge();
                    nextAttack = 0;
                }
            }
        }
        if (guardBrokenTime > 0) renderer.sprite = guardBrokenSprite;
        else renderer.sprite = blockSprite;
    }
    void duringLeap() {
        if (!Physics2D.BoxCast(transform.position, new Vector2(1.4375f, 0.0625f), 0, Vector2.down, 1, 64) || rb.velocity.y > 0.5f * terminalVelocity)
            rb.velocity = new Vector2(facing * leapSpeed, rb.velocity.y);
        else {
            rb.velocity = Vector2.zero;
            if (facing * (player.position.x - transform.position.x) < 0) {
                facing = -facing;
                renderer.flipX = !renderer.flipX;
            }
            leaping = false;
        }
    }
    void startShortCharge() {
        rb.velocity = new Vector2(facing * chargeSpeed, rb.velocity.y);
        shortChargeTime = maxShortChargeTime;
        shortCharge = true;
    }
    void startLongCharge() {
        rb.velocity = Vector2.zero;
        telegraphTime = maxTelegraphTime;
        longCharge = true;
    }

    public override Color getColor() { return Color.yellow; }
    public override string getInitial() { return "S"; }
}
