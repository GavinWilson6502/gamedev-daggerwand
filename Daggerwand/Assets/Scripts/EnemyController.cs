using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using Unity.VisualScripting;
using UnityEngine;
using System;

public class EnemyController : MonoBehaviour
{
    [SerializeField] protected GameplayManager gameplayManager;

    [SerializeField] protected int maxHealth = 100;
    protected int health;
    [Serializable]
    protected class DamageValueDict : Dictionary<string, int>, ISerializationCallbackReceiver {
        public int slashDamage, thrustDamage, backjabDamage, twirlDamage, blockDamage, swingDamage, slamDamage,
                   beamDamage, pierceDamage, throwDamage, waveDamage, emberDamage;
        public void OnBeforeSerialize() {
            slashDamage = ContainsKey("Slash") ? this["Slash"] : 1;
            thrustDamage = ContainsKey("Thrust") ? this["Thrust"] : 1;
            backjabDamage = ContainsKey("Backjab") ? this["Backjab"] : 1;
            twirlDamage = ContainsKey("Twirl") ? this["Twirl"] : 1;
            blockDamage = ContainsKey("Block") ? this["Block"] : 1;
            swingDamage = ContainsKey("Swing") ? this["Swing"] : 1;
            slamDamage = ContainsKey("Slam") ? this["Slam"] : 1;
            beamDamage = ContainsKey("Beam") ? this["Beam"] : 1;
            pierceDamage = ContainsKey("Pierce") ? this["Pierce"] : 1;
            throwDamage = ContainsKey("Throw") ? this["Throw"] : 1;
            waveDamage = ContainsKey("Wave") ? this["Wave"] : 1;
            emberDamage = ContainsKey("Ember") ? this["Ember"] : 1;
        }
        public void OnAfterDeserialize() {
            this["Slash"] = slashDamage;
            this["Thrust"] = thrustDamage;
            this["Backjab"] = backjabDamage;
            this["Twirl"] = twirlDamage;
            this["Block"] = blockDamage;
            this["Swing"] = swingDamage;
            this["Slam"] = slamDamage;
            this["Beam"] = beamDamage;
            this["Pierce"] = pierceDamage;
            this["Throw"] = throwDamage;
            this["Wave"] = waveDamage;
            this["Ember"] = emberDamage;
        }
    }
    [SerializeField] protected DamageValueDict damageValues = new DamageValueDict() {
        {"Slash", 1}, {"Thrust", 1}, {"Backjab", 1}, {"Twirl", 1}, {"Block", 1}, {"Swing", 1}, {"Slam", 1},
        {"Beam", 1}, {"Pierce", 1}, {"Throw", 1}, {"Wave", 1}, {"Ember", 1}
    };
    [SerializeField] protected float maxInvincibilityTime;
    protected float invincibilityTime = 0;
    [SerializeField] protected float maxStunTime = 1;
    protected float stunTime = 0;
    [SerializeField] protected float maxShockTime;
    protected float shockTime = 0;
    [SerializeField] GameObject[] drops;
    [SerializeField] int[] probabilities;

    protected string[] attackedBy = {"None", "None"};

    protected Rigidbody2D rb;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    public virtual void Update()
    {
        if (gameplayManager.isPaused()) return;

        if (health <= 0) {
            int sum = 0;
            for (int i = 0; i < probabilities.Length; ++i) sum += probabilities[i];
            int rand = UnityEngine.Random.Range(0, sum);
            sum = 0;
            int index = drops.Length;
            for (int i = 0; i < probabilities.Length; ++i) {
                sum += probabilities[i];
                if (rand >= sum) continue;
                index = i;
                break;
            }
            if (index < drops.Length) gameplayManager.addPickup(Instantiate(drops[index], transform.position, Quaternion.identity).GetComponent<Pickup>());
            foreach (Transform child in GetComponentsInChildren<Transform>()) {
                if (child.gameObject == gameObject) continue;
                Destroy(child.gameObject);
            }
            Destroy(gameObject);
        }

        if (invincibilityTime > 0) invincibilityTime -= Time.deltaTime;
        if (stunTime > 0) stunTime -= Time.deltaTime;
        if (shockTime > 0) shockTime -= Time.deltaTime;
        if (attackedBy[0].Equals("Blocked")) onBlock();
        else if (attackedBy[0].Equals("Twirl")) onAttackedTwirl();
        else if (!attackedBy[0].Equals("None")) {
            if (attackedBy[0].Equals("Block")) onAttackedBlock();
            onAttackedEffective(0);
        }
        attackedBy[0] = "None";
        if (attackedBy[1].Equals("Blocked")) onBlock();
        else if (!attackedBy[1].Equals("None")) {
            onAttackedEffective(1);
        }
        attackedBy[1] = "None";
        updateIfUnpaused();
    }
    protected virtual void updateIfUnpaused() {}

    void FixedUpdate() {
        Rigidbody2D[] rigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rigidbody in rigidbodies) rigidbody.simulated = !gameplayManager.isPaused();
        EnemyWeapon[] hitboxes = GetComponentsInChildren<EnemyWeapon>();
        foreach (EnemyWeapon hitbox in hitboxes) hitbox.setIsPaused(gameplayManager.isPaused());
        if (gameplayManager.isPaused()) return;
        fixedUpdateIfUnpaused();
    }
    protected virtual void fixedUpdateIfUnpaused() {}

    public int onAttacked(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        if (attackedColliderIntangible(weaponName, projectile, direction, attackedCollider)) return 0;
        int index = projectile ? 1 : 0;
        if (attackedColliderBlocks(weaponName, projectile, direction, attackedCollider)) {
            if (invincibilityTime <= 0) attackedBy[index] = "Blocked";
            return attackedCollider.GetComponent<EnemyWeapon>().getPersistAfterBlocked() ? -1 : -2;
        }
        if (attackedBy[index].Equals("Blocked")) return 0;
        if (weaponName.Equals("Twirl")) {
            if (invincibilityTime <= 0 && stunTime <= 0) attackedBy[index] = "Twirl";
            return stunTime <= 0 ? 1 : 0;
        }
        EnemyWeapon attackedHitbox = attackedCollider.GetComponent<EnemyWeapon>();
        if (invincibilityTime <= 0 && (!attackedHitbox.getProjectile() || attackedHitbox.getHeld())) attackedBy[index] = string.Copy(weaponName);
        if (attackedCollider.name.Equals("Hurtbox")) return health - damageValues[weaponName] > 0 ? 1 : 3;
        return attackedHitbox.getPersistAfterBlocked() ? 1 : 2;
    }
    protected virtual bool attackedColliderIntangible(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return !(attackedCollider.name.Equals("Hurtbox") || weaponName.Equals("Block"));
    }
    protected virtual bool attackedColliderBlocks(string weaponName, bool projectile, Vector2 direction, Collider2D attackedCollider) {
        return false;
    }

    protected virtual void onBlock() {}
    protected virtual void onAttackedTwirl() {
        if (health <= damageValues["Twirl"]) {
            onAttackedEffective(0);
            return;
        }
        stunTime = maxStunTime;
    }
    protected virtual void onAttackedBlock() {
        invincibilityTime = maxInvincibilityTime;
        shockTime = maxShockTime;
    }
    protected virtual void onAttackedEffective(int index) {
        if (index == 1) invincibilityTime = maxInvincibilityTime;
        health -= damageValues[attackedBy[index]];
    }

    protected static float[] quadraticFormula(float a, float b, float c) {
        if (a == 0) return new float[] {-c / b, -c / b};
        float temp = Mathf.Sqrt(b * b - 4 * a * c);
        return new float[] {(-b - temp) / (2 * a), (-b + temp) / (2 * a)};
    }
}
