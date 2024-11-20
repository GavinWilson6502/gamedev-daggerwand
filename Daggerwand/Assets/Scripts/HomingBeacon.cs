using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HomingBeacon : EnemyController
{
    Transform player;
    [SerializeField] float flySpeed;
    
    Rigidbody2D[] beacons;
    int beaconIndex = 0;
    float[] spread;
    bool active = false;

    // Start is called before the first frame update
    void Start()
    {
        health = maxHealth;
        player = gameplayManager.getPlayer().transform;
        beacons = GetComponentsInChildren<Rigidbody2D>();
        spread = new float[beacons.Length];
        for (int i = 0; i < spread.Length; ++i) spread[i] = Random.Range(1f, 3f) * (spread.Length / 2 - i);
    }

    // Update is called once per frame
    /*void Update()
    {
        
    }*/

    protected override void fixedUpdateIfUnpaused() {
        if (!active) {
            if (player != null && Mathf.Abs(transform.position.x - gameplayManager.getMainCameraX()) <= gameplayManager.getMainCameraWidth() + 1 && Mathf.Abs(transform.position.y - gameplayManager.getMainCameraY()) <= gameplayManager.getMainCameraHeight() - 1) {
                transform.position = new Vector2(transform.position.x, Mathf.Min(player.position.y + 4.5f, gameplayManager.getMainCameraY() + gameplayManager.getMainCameraHeight() - 1));
                active = true;
            }
            return;
        }
        //if (transform.position.x - gameplayManager.getMainCameraX() - Mathf.Sign(flySpeed) * gameplayManager.getMainCameraWidth() > 1) {
        //if (Mathf.Sign(flySpeed) * (transform.position.x - gameplayManager.getMainCameraX() - Mathf.Sign(flySpeed) * gameplayManager.getMainCameraWidth()) > 1) {
        if (Mathf.Sign(flySpeed) * (transform.position.x - gameplayManager.getMainCameraX()) - gameplayManager.getMainCameraWidth() > 1) {
            recursiveDestroy(transform);
            return;
        }
        if (beaconIndex < beacons.Length && player != null) {
            Vector2 target = new Vector2(-Mathf.Sign(flySpeed) * spread[beaconIndex], 0.25f);
            target += Physics2D.Raycast(player.position, Vector2.down, Mathf.Infinity, 64).point;
            if (float.IsInfinity(target.y)) target.y = player.position.y - 0.75f;
            float lead = Mathf.Sqrt(2 * (target.y - beacons[beaconIndex].transform.position.y) / gameplayManager.getGravity().y);
            //if (float.IsInfinity(lead)) lead = Mathf.Sqrt(2 * (player.position.y - beacons[beaconIndex].transform.position.y) / gameplayManager.getGravity().y);
            if (float.IsNaN(lead)) lead = 0;
            lead = Mathf.Abs(flySpeed * lead);
            //if (-Mathf.Sign(flySpeed) * (transform.position.x - player.position.x + Mathf.Sign(flySpeed) * lead) <= spread[beaconIndex]) {
            if (Mathf.Sign(flySpeed) * (target.x - beacons[beaconIndex].transform.position.x) <= lead) {
                beacons[beaconIndex].transform.SetParent(null);
                beacons[beaconIndex].bodyType = RigidbodyType2D.Dynamic;
                beacons[beaconIndex].velocity = new Vector2(flySpeed, 0);
                beacons[beaconIndex].GetComponent<Beacon>().setTarget(target.x, target.y);
                ++beaconIndex;
            }
        }
        transform.position += new Vector3(flySpeed * Time.deltaTime, 0);
    }

    private void recursiveDestroy(Transform t) {
        for (int i = 0; i < t.transform.childCount; ++i) recursiveDestroy(t.transform.GetChild(i));
        Destroy(t.gameObject);
    }
}
