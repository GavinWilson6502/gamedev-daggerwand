using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelVertex : MonoBehaviour
{
    [SerializeField] LevelVertex left;
    [SerializeField] LevelVertex right;
    [SerializeField] LevelVertex down;
    [SerializeField] LevelVertex up;
    LevelVertex partner;
    [SerializeField] Vector2 direction;
    [SerializeField] bool checkpoint;
    [SerializeField] bool bossRoom;
    [SerializeField] BossController boss;

    // Start is called before the first frame update
    void Start()
    {
        init();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void init() {
        if (direction.x < 0) partner = left;
        else if (direction.x > 0) partner = right;
        else if (direction.y < 0) partner = down;
        else if (direction.y > 0) partner = up;
        else partner = this;
    }
    public LevelVertex getLeft() {
        return left;
    }
    public LevelVertex getRight() {
        return right;
    }
    public LevelVertex getUp() {
        return up;
    }
    public LevelVertex getDown() {
        return down;
    }
    public LevelVertex getPartner() {
        return partner;
    }
    public Vector2 getDirection() {
        return direction;
    }
    public bool isCheckpoint() {
        return checkpoint;
    }
    public bool isBossRoom() {
        return bossRoom;
    }
    public BossController getBoss() {
        return boss;
    }
}
