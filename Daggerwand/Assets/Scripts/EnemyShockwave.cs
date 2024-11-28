using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemyShockwave : EnemyWeapon
{
    [SerializeField] Tilemap ground;
    Vector2 direction = Vector2.right;
    [SerializeField] Vector2 initialV;
    [SerializeField] float maxDecayTime;
    float decayTime;
    
    // Start is called before the first frame update
    void Start()
    {
        ground = GetComponentInParent<EnemyController>().getGround();
        //if (ground.HasTile(ground.WorldToCell(new Vector3(transform.position.x, transform.position.y + 0.5f, 0)))
        //    || !ground.HasTile(ground.WorldToCell(new Vector3(transform.position.x, transform.position.y - 0.5f, 0)))) {
        Tile tile;
        if (((tile = (Tile)ground.GetTile(ground.WorldToCell(new Vector3(transform.position.x, transform.position.y + 0.5f, 0)))) != null && tile.colliderType != Tile.ColliderType.None)
            || (tile = (Tile)ground.GetTile(ground.WorldToCell(new Vector3(transform.position.x, transform.position.y - 0.5f, 0)))) == null || tile.colliderType == Tile.ColliderType.None) {
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
        Rigidbody2D[] rigidbodies = GetComponentsInChildren<Rigidbody2D>();
        foreach (Rigidbody2D rigidbody in rigidbodies) rigidbody.simulated = !isPaused;
        if (isPaused) return;

        if (decayTime <= 0) {
            Destroy(gameObject);
            return;
        }
        decayTime -= Time.deltaTime;
        
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
        //return ground.HasTile(ground.WorldToCell(transform.position + transform.TransformDirection(new Vector2(0, dir * 0.5f))));
        Tile tile = (Tile)ground.GetTile(ground.WorldToCell(transform.position + transform.TransformDirection(new Vector2(0, dir * 0.5f))));
        return tile != null && tile.colliderType != Tile.ColliderType.None;
    }

    public override void OnTriggerStay2D(Collider2D other) {
        if (other.gameObject.name.Equals("Shield Block")) return;
        base.OnTriggerStay2D(other);
    }

    public void refresh() {
        decayTime = maxDecayTime;
    }
}
