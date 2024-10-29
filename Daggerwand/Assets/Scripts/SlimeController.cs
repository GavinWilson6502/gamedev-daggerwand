using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SlimeController : SideHopper
{
    [SerializeField] GameObject smallSlime;

    // Start is called before the first frame update
    /*void Start()
    {
        
    }*/

    // Update is called once per frame
    public override void Update()
    {
        if (smallSlime == null) {
            base.Update();
            return;
        }

        if (gameplayManager.isPaused()) return;

        if (health <= 0) {
            SlimeController slimeController = smallSlime.GetComponent<SlimeController>();
            Instantiate(smallSlime, transform.position + new Vector3(0, 0.5f * (getSize() - slimeController.getSize())), Quaternion.identity, gameplayManager.transform);
            Instantiate(smallSlime, transform.position + new Vector3(0, 0.5f * (slimeController.getSize() - getSize())), Quaternion.identity, gameplayManager.transform);
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
}
