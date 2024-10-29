using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pickup : MonoBehaviour
{
    [SerializeField] Sprite[] sprites;
    [SerializeField] int[] values;
    [SerializeField] int[] probabilities;
    int index = 0;
    [SerializeField] string type;
    [SerializeField] float maxTime;
    float fadeOutTime = 1;
    float time;

    bool isPaused = false;
    SpriteRenderer renderer;

    // Start is called before the first frame update
    void Start()
    {
        int sum = 0;
        for (int i = 0; i < probabilities.Length; ++i) sum += probabilities[i];
        int rand = Random.Range(0, sum);
        sum = 0;
        for (int i = 0; i < probabilities.Length; ++i) {
            sum += probabilities[i];
            if (rand >= sum) continue;
            index = i;
            break;
        }
        renderer = GetComponent<SpriteRenderer>();
        renderer.sprite = sprites[index];
        GetComponent<BoxCollider2D>().size = sprites[index].bounds.size;
        time = maxTime;
    }

    // Update is called once per frame
    void Update()
    {
        if (isPaused) return;

        time -= Time.deltaTime;
        if (time <= 0) {
            Destroy(gameObject);
            return;
        }
        if (time < fadeOutTime) renderer.color = new Color(renderer.color.r, renderer.color.g, renderer.color.b, time / fadeOutTime);
    }

    void OnTriggerEnter2D(Collider2D other) {
        OnTriggerStay2D(other);
    }

    void OnTriggerStay2D(Collider2D other) {
        if (other.transform.parent == null) return;
        PlayerController playerController = other.GetComponentInParent<PlayerController>();
        if (playerController == null) return;
        playerController.onPickup(type, values[index]);
        Destroy(gameObject);
    }

    public void setIsPaused(bool paused) {
        isPaused = paused;
    }
}
