using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour
{
    GameplayManager gameplayManager;
    PlayerController player;
    int selection = 0;

    // Start is called before the first frame update
    void Start()
    {
        gameplayManager = GetComponent<GameplayManager>();
        player = gameplayManager.getPlayer();
    }

    // Update is called once per frame
    void Update()
    {
        player.jump(Input.GetKeyDown(KeyCode.Space) ? 1 : (Input.GetKey(KeyCode.Space) ? 0 : -1));

        if (gameplayManager.isPaused() && !gameplayManager.getSelectPause() && !gameplayManager.getGamePause()) return;

        if (Input.GetKeyDown(KeyCode.Escape)) gameplayManager.toggleGamePause();
        if (gameplayManager.getGamePause()) return;

        if (player.getAttackTime() <= 0 && gameplayManager.setSelectPause(Input.GetKey(KeyCode.K))) {
            player.setSprite();
            player.destroyProjectile();
            gameplayManager.setSelectMenuActive(true);
        }
        if (gameplayManager.getSelectPause()) {
            if (Input.GetKeyDown(KeyCode.Alpha0)) selection = 0;
            else if (Input.GetKeyDown(KeyCode.Alpha1)) selection = 1;
            else if (Input.GetKeyDown(KeyCode.Alpha2)) selection = 2;
            else if (Input.GetKeyDown(KeyCode.Alpha3)) selection = 3;
            else if (Input.GetKeyDown(KeyCode.Alpha4)) selection = 4;
            else if (Input.GetKeyDown(KeyCode.Alpha5)) selection = 5;
            else if (Input.GetKeyDown(KeyCode.Alpha6)) selection = 6;
            else if (Input.GetKeyDown(KeyCode.Alpha8)) player.quaff();
            if (((gameplayManager.fetchWeapons() >> selection) & 1) == 1) gameplayManager.updateHUDSelection(selection);
            return;
        }
        if (Input.GetKeyUp(KeyCode.K)) {
            if (((gameplayManager.fetchWeapons() >> selection) & 1) == 1) player.setWeapon(selection);
            gameplayManager.setSelectMenuActive(false);
        }

        if (gameplayManager.isPaused() || player.getKnockback()) return;

        int direction = 0;
        if (Input.GetKey(KeyCode.A)) direction -= 1;
        if (Input.GetKey(KeyCode.D)) direction += 1;
        player.turn(direction);
        int aim = 0;
        if (Input.GetKey(KeyCode.W)) aim += 1;
        if (Input.GetKey(KeyCode.S)) aim -= 1;
        player.aim(direction, aim);
        if ((player.crouch(Input.GetKey(KeyCode.LeftShift)) && player.isGrounded()) || direction == 0) player.stop();
        if (Input.GetKey(KeyCode.J)) {
            player.holdAttack();
            if (Input.GetKeyDown(KeyCode.J)) player.pressAttack();
        }
        else player.releaseAttack();
        player.setSprite();
    }

    void FixedUpdate() {
        if (gameplayManager.isPaused() || player.getKnockback()) return;

        int direction = 0;
        if (Input.GetKey(KeyCode.A)) direction -= 1;
        if (Input.GetKey(KeyCode.D)) direction += 1;
        if (player.crouch(Input.GetKey(KeyCode.LeftShift)) && player.isGrounded()) direction = 0;
        player.walk(direction);
    }
}
