using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class WallCrawler : EnemyController
{
    Tilemap ground;
    [SerializeField] float speed;
    Vector2 direction;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        ground = gameplayManager.getGround();
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void fixedUpdateIfUnpaused() {
        if (stunTime > 0 || shockTime > 0) return;
        direction = transform.TransformDirection(new Vector2(Mathf.Sign(speed), 0));
        Vector2 destination = (Vector2)(transform.position + transform.TransformVector(new Vector2(speed * Time.deltaTime, 0)));
        if (direction.x > 0 && Mathf.Ceil(transform.position.x - 0.5f) + 0.5f < destination.x
         || direction.x < 0 && Mathf.Floor(transform.position.x + 0.5f) - 0.5f > destination.x
         || direction.y > 0 && Mathf.Ceil(transform.position.y - 0.5f) + 0.5f < destination.y
         || direction.y < 0 && Mathf.Floor(transform.position.y + 0.5f) - 0.5f > destination.y) {
            transform.position = new Vector2(
                direction.x < 0 ? Mathf.Floor(transform.position.x + 0.5f) - 0.5f : (direction.x > 0 ? Mathf.Ceil(transform.position.x - 0.5f) + 0.5f : transform.position.x),
                direction.y < 0 ? Mathf.Floor(transform.position.y + 0.5f) - 0.5f : (direction.y > 0 ? Mathf.Ceil(transform.position.y - 0.5f) + 0.5f : transform.position.y)
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
        bool onFloor = ground.HasTile(ground.WorldToCell(transform.position + transform.TransformDirection(Vector2.down)));
        bool hitWall = onFloor && ground.HasTile(ground.WorldToCell(transform.position + (Vector3)direction));
        if (!onFloor && speed < 0 || hitWall && speed > 0) {
            transform.Rotate(new Vector3(0, 0, 90));
            direction = new Vector2(-direction.y, direction.x);
            return transform.position + new Vector3(transform.position.y - destination.y, destination.x - transform.position.x);
        }
        if (!onFloor && speed > 0 || hitWall && speed < 0) {
            transform.Rotate(new Vector3(0, 0, -90));
            direction = new Vector2(direction.y, -direction.x);
            return transform.position + new Vector3(destination.y - transform.position.y, transform.position.x - destination.x);
        }
        return destination;
    }
}
