using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volley : MonoBehaviour
{
    [SerializeField] GameplayManager gameplayManager;
    [SerializeField] float period;
    [SerializeField] float phase;
    float attackTime;
    Rigidbody2D arrow;
    [SerializeField] float arrowSpeed;

    ProjectileLauncher launcher;

    // Start is called before the first frame update
    void Start()
    {
        attackTime = ((period + phase) % period + period) % period;
        launcher = GetComponentInChildren<ProjectileLauncher>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void FixedUpdate() {
        if (gameplayManager.isPaused()) return;

        if (attackTime <= 0) {
            if (arrow != null) Destroy(arrow.gameObject);
            arrow = launcher.launch(1, 1).GetComponent<Rigidbody2D>();
            gameplayManager.addProjectile(arrow.GetComponent<EnemyWeapon>());
            arrow.transform.SetParent(null);
            arrow.velocity = new Vector2(0, -arrowSpeed);
            attackTime = period;
        }
        else if (arrow != null && arrow.velocity.y != 0 && arrow.transform.position.y + arrow.velocity.y * Time.deltaTime <= transform.position.y) {
            arrow.transform.position = transform.position;
            arrow.bodyType = RigidbodyType2D.Kinematic;
            arrow.velocity = Vector2.zero;
            Collider2D[] arrowColliders = arrow.GetComponentsInChildren<Collider2D>();
            for (int i = 0; i < arrowColliders.Length; ++i) arrowColliders[i].enabled = false;
        }
        if (Mathf.Abs(transform.position.y - gameplayManager.getMainCameraY()) > gameplayManager.getMainCameraHeight()) return;
        attackTime -= Time.deltaTime;
    }
}
