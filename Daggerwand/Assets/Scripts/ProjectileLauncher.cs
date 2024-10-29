using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ProjectileLauncher : MonoBehaviour
{
    [SerializeField] Vector2 offset;
    [SerializeField] GameObject projectilePrefab;
    bool flipX;
    bool flipY;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public GameObject launch(int facingX, int facingY) {
        flipX = facingX < 0;
        flipY = facingY < 0;
        return Instantiate(projectilePrefab, transform.position + transform.TransformDirection(new Vector3(facingX * offset.x, facingY * offset.y, 0)), transform.rotation, transform);
    }

    public bool getFlipX() {
        return flipX;
    }

    public bool getFlipY() {
        return flipY;
    }
}
