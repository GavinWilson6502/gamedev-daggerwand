using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System;

public class PlayerShockwave : PlayerWeapon
{
    [SerializeField] Tilemap ground;

    // Start is called before the first frame update
    void Start()
    {
        ground = GetComponentInParent<PlayerController>().getGround();
        if (ground.HasTile(ground.WorldToCell(new Vector3(transform.position.x, transform.position.y + 0.5f, 0)))
            || !ground.HasTile(ground.WorldToCell(new Vector3(transform.position.x, transform.position.y - 0.5f, 0)))) {
            Destroy(gameObject);
            return;
        }
        transform.position = new Vector2(transform.position.x, Mathf.Round(transform.position.y));
        if (GetComponentInParent<ProjectileLauncher>().getFlipX()) {
            direction = -direction;
            transform.Rotate(0, 0, 180);
        }
        transform.SetParent(null);
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    void FixedUpdate() {
        if (isPaused) return;
        
        Vector2 destination = (Vector2)transform.position + direction * initialV.magnitude * Time.deltaTime;
        if (direction.x > 0 && Mathf.Ceil(transform.position.x) < destination.x
            || direction.x < 0 && Mathf.Floor(transform.position.x) > destination.x
            || direction.y > 0 && Mathf.Ceil(transform.position.y) < destination.y
            || direction.y < 0 && Mathf.Floor(transform.position.y) > destination.y) {
            transform.position = new Vector2(
                direction.x < 0 ? Mathf.Floor(transform.position.x) : (direction.x > 0 ? Mathf.Ceil(transform.position.x) : transform.position.x),
                direction.y < 0 ? Mathf.Floor(transform.position.y) : (direction.y > 0 ? Mathf.Ceil(transform.position.y) : transform.position.y)
            );
            destination = checkTurn(destination);
        }
        while (((Vector3)destination - transform.position).magnitude >= 1) {
            transform.position += (Vector3)direction;
            destination = checkTurn(destination);
        }
        transform.position = destination;
    }

    Vector2 checkTurn(Vector2 destination) {
        transform.position += -0.5f * (Vector3)direction;
        bool prevFloor = hasGround(-1);
        transform.position += (Vector3)direction;
        bool currentFloor = hasGround(-1);
        bool currentCeil = hasGround(1);
        transform.position += -0.5f * (Vector3)direction;
        if (currentFloor == currentCeil) {
            if (prevFloor == currentFloor) {
                transform.Rotate(new Vector3(0, 0, 90));
                direction = new Vector2(-direction.y, direction.x);
                return transform.position + new Vector3(transform.position.y - destination.y, destination.x - transform.position.x, 0);
            }
            else {
                transform.Rotate(new Vector3(0, 0, -90));
                direction = new Vector2(direction.y, -direction.x);
                return transform.position + new Vector3(destination.y - transform.position.y, transform.position.x - destination.x, 0);
            }
        }
        return destination;
    }

    bool hasGround(int dir) {
        return ground.HasTile(ground.WorldToCell(transform.position + transform.TransformDirection(new Vector2(0, dir * 0.5f))));
    }

    public override void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.layer != 10) return;
        EnemyController enemy = other.transform.parent == null ? other.GetComponent<EnemyWeapon>().getEnemyController() : other.GetComponentInParent<EnemyController>();
        if (enemy == null) return;
        int temp = enemy.onAttacked(weaponName, projectile, direction, other);
        if (temp == 0) return;
        if (!attackStatus.ContainsKey(enemy)) attackStatus.Add(enemy, new List<int>());
        attackStatus[enemy].Add(temp);
    }
}
