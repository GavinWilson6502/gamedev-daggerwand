using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Unity.VisualScripting;
using Unity.VisualScripting.Antlr3.Runtime;
using UnityEngine;
using UnityEngine.Tilemaps;

public class PlayerController : MonoBehaviour
{
    [SerializeField] GameplayManager gameplayManager;

    [Header("Sprites")]
    [SerializeField] Sprite standing;
    [SerializeField] Sprite crouching;
    [SerializeField] Sprite standingSlash;
    [SerializeField] Sprite crouchingSlash;
    [SerializeField] Sprite standingThrust;
    [SerializeField] Sprite crouchingThrust;
    [SerializeField] Sprite standingBackjab;
    [SerializeField] Sprite crouchingBackjab;
    [SerializeField] Sprite twirl;
    [SerializeField] Sprite standingBlock;
    [SerializeField] Sprite crouchingBlock;
    [SerializeField] Sprite standingSwing;
    [SerializeField] Sprite crouchingSwing;
    [SerializeField] Sprite slam;
    Sprite currentStanding;
    Sprite currentCrouching;

    [Header("Physics")]
    [SerializeField] float walkForce;
    [SerializeField] float walkSpeed;
    [SerializeField] float terminalVelocity;
    [SerializeField] float flingSpeed;
    [SerializeField] float maxJumpBuffer;
    float jumpBuffer = 0;
    [SerializeField] float maxCoyoteTime;
    float coyoteTime = 0;
    [SerializeField] float knockbackSpeed;

    [Header("Animation")]
    [SerializeField] float maxSlashTime;
    [SerializeField] float maxThrustTime;
    [SerializeField] float backjabTime;
    [SerializeField] float maxSwingTime;
    [SerializeField] float maxSlamTime;
    int facing = 1;

    [Header("Combat")]
    [SerializeField] int maxHealth = 100;
    int health;
    [SerializeField] int maxMagic = 100;
    [SerializeField] int[] magicCost;
    int[] magic = new int[7];
    int potions = 0;
    int lives = 3;
    [SerializeField] float maxInvincibilityTime;
    float invincibilityTime = 0;
    [SerializeField] float maxKnockbackTime;
    float knockbackTime = 0;
    float shieldKnockbackTime = 0;

    PlayerWeapon projectile = null;

    SpriteRenderer renderer;
    Rigidbody2D rb;
    Collider2D[] colliders;

    bool isCrouching = false;
    bool isJumping = false;
    bool isFlinging = false;
    int weapon = 0;
    float attackTime = 0;
    bool boHeld = false;
    Dictionary<GameObject, int[]> projectileAttackedStatus = new Dictionary<GameObject, int[]>();
    Dictionary<GameObject, int[]> meleeAttackedStatus = new Dictionary<GameObject, int[]>();

    // Start is called before the first frame update
    void Start()
    {
        gameplayManager.init();
        gameplayManager.updateHUDLives(lives = gameplayManager.fetchLives());
        gameplayManager.updateHUDPotion(potions = gameplayManager.fetchPotions(), true);
        magic = gameplayManager.fetchMagic();
        for (int i = 0; i < magic.Length; ++i) gameplayManager.updateHUDMagic(i, (float)magic[i] / maxMagic);
        currentStanding = standing;
        currentCrouching = crouching;
        health = maxHealth;
        renderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        colliders = GetComponentsInChildren<Collider2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (gameplayManager.moveCamera(transform)) die();

        if (gameplayManager.isPaused()) return;

        foreach (KeyValuePair<GameObject, int[]> kvp in projectileAttackedStatus) {
            if (kvp.Value[0] <= 0) continue;
            onAttackedEffective(kvp.Value[1]);
            meleeAttackedStatus.Clear();
            break;
        }
        foreach (KeyValuePair<GameObject, int[]> kvp in meleeAttackedStatus) {
            if (kvp.Value[0] <= 0) continue;
            onAttackedEffective(kvp.Value[1]);
            break;
        }
        projectileAttackedStatus.Clear();
        meleeAttackedStatus.Clear();

        float maxVerticalSpeed = isFlinging ? flingSpeed : terminalVelocity;
        if (Math.Abs(rb.velocity.y) > maxVerticalSpeed) rb.velocity = new Vector2(rb.velocity.x, Math.Sign(rb.velocity.y) * maxVerticalSpeed);
        if (Math.Abs(rb.velocity.x) > walkSpeed) rb.velocity = new Vector2(Math.Sign(rb.velocity.x) * walkSpeed, rb.velocity.y);

        if (invincibilityTime > 0) invincibilityTime -= Time.deltaTime;

        if (projectile != null && (Mathf.Abs(projectile.transform.position.x - gameplayManager.getMainCameraX()) > gameplayManager.getMainCameraWidth() + projectile.getMargin()
            || projectile.getDespawnOffTop() && projectile.transform.position.y - gameplayManager.getMainCameraY() > gameplayManager.getMainCameraHeight() + projectile.getMargin()
            || projectile.getDespawnOffBottom() && gameplayManager.getMainCameraY() - projectile.transform.position.y > gameplayManager.getMainCameraHeight() + projectile.getMargin())) {
            Destroy(projectile.gameObject);
            projectile = null;
        }
    }

    void FixedUpdate()
    {
        colliders[6].enabled = false;
        
        rb.simulated = !gameplayManager.isPaused();
        if (projectile != null) {
            projectile.GetComponent<Rigidbody2D>().simulated = !gameplayManager.isPaused();
            projectile.setIsPaused(gameplayManager.isPaused());
        }
        if (gameplayManager.isPaused()) return;

        if (knockbackTime > 0) {
            knockbackTime -= Time.deltaTime;
            return;
        }
        if (attackTime > 0) {
            attackTime -= Time.deltaTime;
            if (currentStanding == standingThrust && attackTime <= backjabTime) {
                currentStanding = standingBackjab;
                currentCrouching = crouchingBackjab;
                colliders[6].enabled = true;
            }
        }
        if (knockbackTime <= 0 && attackTime <= 0 && (weapon < 2 || weapon > 3)) {
            currentStanding = standing;
            currentCrouching = crouching;
        }
    }

    public void setSprite() {
        renderer.sprite = isCrouching ? currentCrouching : currentStanding;
    }

    public void walk(int direction) {
        turn(direction);
        if (rb.velocity.x * facing >= 0) shieldKnockbackTime = 0;
        if (shieldKnockbackTime > 0) shieldKnockbackTime -= Time.deltaTime;
        else rb.AddForce(new Vector2(direction * walkForce, 0));
    }

    public void stop() {
        if (shieldKnockbackTime > 0) return;
        rb.velocity = new Vector2(0, rb.velocity.y);
    }

    public void turn(int direction) {
        if (direction < 0) {
            renderer.flipX = true;
            facing = -1;
        }
        else if (direction > 0) {
            renderer.flipX = false;
            facing = 1;
        }
        colliders[4].transform.localPosition = new Vector2(facing * 1, colliders[4].transform.localPosition.y);
        colliders[5].transform.localPosition = new Vector2(facing * 3, colliders[5].transform.localPosition.y);
        colliders[6].transform.localPosition = new Vector2(facing * -0.5f, colliders[6].transform.localPosition.y);
        colliders[7].transform.localPosition = new Vector2(facing * 0.75f, colliders[7].transform.localPosition.y);
        colliders[8].transform.localPosition = new Vector2(facing * 0.625f, colliders[8].transform.localPosition.y);
        colliders[13].transform.localPosition = new Vector2(facing * 0.5f, colliders[13].transform.localPosition.y);
    }

    public void aim(int horizontal, int vertical) {
        colliders[13].transform.rotation = Quaternion.identity;
        if (vertical == 0) return;
        if (horizontal == 0) colliders[13].transform.Rotate(0, 0, vertical * facing * 90);
        else colliders[13].transform.Rotate(0, 0, horizontal == vertical ? 45 : -45);
    }

    public bool crouch(bool inputHeld) {
        if (currentCrouching == slam && attackTime > 0) return true;
        if (inputHeld && !isCrouching) {
            isCrouching = true;
            colliders[0].enabled = false;
            colliders[1].enabled = true;
            colliders[2].enabled = false;
            colliders[3].enabled = true;
            colliders[4].transform.localPosition = new Vector2(colliders[4].transform.localPosition.x, -0.5f);
            colliders[5].transform.localPosition = new Vector2(colliders[5].transform.localPosition.x, -0.5f);
            colliders[6].transform.localPosition = new Vector2(colliders[6].transform.localPosition.x, -0.5f);
            colliders[8].transform.localPosition = new Vector2(colliders[8].transform.localPosition.x, -0.5f);
            colliders[10].transform.localPosition = new Vector2(colliders[10].transform.localPosition.x, -0.25f);
            colliders[13].transform.localPosition = new Vector2(colliders[13].transform.localPosition.x, -0.5f);
        }
        else if (!inputHeld && isCrouching && !Physics2D.BoxCast(transform.position, new Vector2(0.9375f, 0.0625f), 0, Vector2.up, 0.9375f, 64)) {
            isCrouching = false;
            colliders[0].enabled = true;
            colliders[1].enabled = false;
            colliders[2].enabled = true;
            colliders[3].enabled = false;
            colliders[4].transform.localPosition = new Vector2(colliders[4].transform.localPosition.x, 0.5f);
            colliders[5].transform.localPosition = new Vector2(colliders[5].transform.localPosition.x, 0.5f);
            colliders[6].transform.localPosition = new Vector2(colliders[6].transform.localPosition.x, 0.5f);
            colliders[8].transform.localPosition = new Vector2(colliders[8].transform.localPosition.x, 0.5f);
            colliders[10].transform.localPosition = new Vector2(colliders[10].transform.localPosition.x, 0.25f);
            colliders[13].transform.localPosition = new Vector2(colliders[13].transform.localPosition.x, 0.5f);
        }
        return isCrouching;
    }

    public void jump(int input) {
        if (knockbackTime > 0) input = -1;
        if (isGrounded()) coyoteTime = maxCoyoteTime;
        if (gameplayManager.isPaused()) {
            if (input < 0) jumpBuffer = 0;
            else if (input > 0 && !isJumping) jumpBuffer = maxJumpBuffer;
            return;
        }

        if (input < 0) {    //!OnKey
            if (isJumping) rb.velocity = new Vector2(rb.velocity.x, 0);
            jumpBuffer = 0;
        }
        else if (coyoteTime > 0) {
            if ((input > 0 || jumpBuffer > 0) && (currentCrouching != slam || attackTime <= 0)) {
                if (boHeld && isCrouching) {
                    rb.velocity = new Vector2(rb.velocity.x, flingSpeed);
                    isFlinging = true;
                    magic[weapon] -= magicCost[weapon];
                    if (magic[weapon] < 0) magic[weapon] = 0;
                    gameplayManager.updateHUDMagic(weapon, (float)magic[weapon] / maxMagic);
                }
                else {
                    rb.velocity = new Vector2(rb.velocity.x, terminalVelocity);
                    isJumping = true;
                }
                jumpBuffer = 0;
                coyoteTime = 0;
            }
        }
        else if (input > 0) jumpBuffer = maxJumpBuffer;
        else if (jumpBuffer > 0) jumpBuffer -= Time.deltaTime;

        if (rb.velocity.y <= 0) {
            isJumping = false;
            isFlinging = false;
        }
        if (coyoteTime > 0) coyoteTime -= Time.deltaTime;
    }

    public bool isGrounded() {
        return Physics2D.BoxCast(transform.position, new Vector2(0.9375f, 0.0625f), 0, Vector2.down, 1, 64);
    }

    public void setWeapon(int w) {
        releaseAttack();
        weapon = w;
    }

    public void pressAttack() {
        if (attackTime > 0 || weapon == 2 || weapon == 3 || magic[weapon] <= 0) return;
        switch (weapon) {
            case 0:
                attackTime = maxSlashTime;
                currentStanding = standingSlash;
                currentCrouching = crouchingSlash;
                colliders[4].enabled = true;
                if (projectile != null) break;
                projectile = colliders[4].GetComponent<ProjectileLauncher>().launch(facing, 1).GetComponent<PlayerWeapon>();
                break;
            case 1:
                attackTime = maxThrustTime;
                currentStanding = standingThrust;
                currentCrouching = crouchingThrust;
                colliders[5].enabled = true;
                if (projectile != null) break;
                projectile = colliders[5].GetComponent<ProjectileLauncher>().launch(facing, 1).GetComponent<PlayerWeapon>();
                break;
            case 4:
                attackTime = maxSwingTime;
                currentStanding = standingSwing;
                currentCrouching = crouchingSwing;
                colliders[10].enabled = true;
                if (projectile != null) break;
                projectile = colliders[10].GetComponent<ProjectileLauncher>().launch(facing, 1).GetComponent<PlayerWeapon>();
                break;
            case 5:
                crouch(true);
                attackTime = maxSlamTime;
                currentCrouching = slam;
                int index = facing > 0 ? 11 : 12;
                colliders[index].enabled = true;
                if (!(projectile == null && isGrounded())) break;
                projectile = colliders[index].GetComponent<ProjectileLauncher>().launch(facing, 1).GetComponent<PlayerWeapon>();
                break;
            case 6:
                if (projectile != null) return;
                projectile = colliders[13].GetComponent<ProjectileLauncher>().launch(facing, 1).GetComponent<PlayerWeapon>();
                magic[weapon] -= magicCost[weapon];
                if (magic[weapon] < 0) magic[weapon] = 0;
                gameplayManager.updateHUDMagic(weapon, (float)magic[weapon] / maxMagic);
                return;
        }
        magic[weapon] -= magicCost[weapon];
        if (magic[weapon] < 0) magic[weapon] = 0;
        gameplayManager.updateHUDMagic(weapon, (float)magic[weapon] / maxMagic);
    }
    public void holdAttack() {
        switch (weapon) {
            case 2:
                if (magic[weapon] <= 0) {
                    releaseAttack();
                    break;
                }
                boHeld = true;
                currentStanding = twirl;
                currentCrouching = crouching;
                colliders[7].enabled = !isCrouching;
                break;
            case 3:
                if (magic[weapon] <= 0 && shieldKnockbackTime <= 0) {
                    releaseAttack();
                    break;
                }
                currentStanding = standingBlock;
                currentCrouching = crouchingBlock;
                colliders[8].enabled = true;
                break;
        }
    }
    public void releaseAttack() {
        if (weapon < 2 || weapon > 3) return;
        boHeld = false;
        currentStanding = standing;
        currentCrouching = crouching;
        colliders[7].enabled = false;
        colliders[8].enabled = false;
    }

    public void destroyProjectile() {
        if (projectile == null) return;
        Destroy(projectile.gameObject);
        projectile = null;
    }

    public float getAttackTime() {
        return attackTime;
    }

    public int onAttacked(GameObject enemy, bool projectile, int damage, int priority, Collider2D attackedCollider) {
        if (invincibilityTime > 0) return 0;
        Dictionary<GameObject, int[]> attackedStatus = projectile ? projectileAttackedStatus : meleeAttackedStatus;
        if (attackedCollider == colliders[8]) return (attackedStatus[enemy] = new int[] {-1, damage, priority})[0];
        if (attackedStatus.ContainsKey(enemy) && (attackedStatus[enemy][0] < 0 || attackedStatus[enemy][2] >= priority)) return 0;
        if (attackedCollider == colliders[2] || attackedCollider == colliders[3]) return (attackedStatus[enemy] = new int[] {1, damage, priority})[0];
        return 0;
    }

    public void onStun() {
        magic[weapon] -= magicCost[weapon];
        if (magic[weapon] < 0) magic[weapon] = 0;
        gameplayManager.updateHUDMagic(weapon, (float)magic[weapon] / maxMagic);
    }

    public void onBlock(bool doShieldKnockback) {
        if (knockbackTime > 0 || shieldKnockbackTime > 0) return;
        magic[weapon] -= magicCost[weapon];
        if (magic[weapon] < 0) magic[weapon] = 0;
        gameplayManager.updateHUDMagic(weapon, (float)magic[weapon] / maxMagic);
        if (!doShieldKnockback) return;
        shieldKnockbackTime = maxKnockbackTime * knockbackSpeed / walkSpeed;
        rb.velocity = new Vector2(-walkSpeed * facing, rb.velocity.y);
    }

    void onAttackedEffective(int damage) {
        invincibilityTime = maxInvincibilityTime;
        knockbackTime = maxKnockbackTime;
        shieldKnockbackTime = 0;
        attackTime = 0;
        releaseAttack();
        currentStanding = standing;
        currentCrouching = crouching;
        setSprite();
        rb.velocity = new Vector2(-knockbackSpeed * facing, rb.velocity.y);
        health -= damage;
        if (health <= 0) {
            health = 0;
            die();
        }
        gameplayManager.updateHUDHealth((float)health / maxHealth);
        gameplayManager.updateHUDPotion(potions, health >= maxHealth);
    }

    public bool getKnockback() {
        return knockbackTime > 0;
    }

    public Tilemap getGround() {
        return gameplayManager.getGround();
    }

    public void onPickup(string type, int value) {
        if (health > 0 && type.Equals("Health")) {
            health += value;
            if (health > maxHealth) health = maxHealth;
            gameplayManager.updateHUDHealth((float)health / maxHealth);
            gameplayManager.updateHUDPotion(potions, health >= maxHealth);
        }
        if (type.Equals("Magic")) {
            magic[weapon] += value;
            if (magic[weapon] > maxMagic) magic[weapon] = maxMagic;
            gameplayManager.updateHUDMagic(weapon, (float)magic[weapon] / maxMagic);
        }
        if (type.Equals("Potion") && potions < 9) gameplayManager.updateHUDPotion(++potions, health >= maxHealth);
        if (type.Equals("Extra Life")) {
            lives += value;
            if (lives > 9) lives = 9;
            gameplayManager.updateHUDLives(lives);
        }
    }

    public void quaff() {
        if (health <= 0 || health >= maxHealth || potions <= 0) return;
        health = maxHealth;
        gameplayManager.updateHUDHealth(1);
        gameplayManager.updateHUDPotion(--potions, true);
    }

    void die() {
        gameplayManager.store(lives, potions, magic);
        gameplayManager.die();
        Destroy(gameObject);
    }

    public int getLives() {
        return lives;
    }
    public int getPotions() {
        return potions;
    }

    //TODO: some enemies + level design, and fine tune stuff like projectile speed while you're at it
    //If there's time later, then have shield continue to block already-blocked attacks
}
