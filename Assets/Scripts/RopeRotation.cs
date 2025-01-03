using UnityEngine;

public class RopeRotation : MonoBehaviour
{
    public GameObject player;
    public float rotationSpeed = 50f;
    
    public GameObject thisRope;
    
    public static RopeRotation instance;
    
    private void Awake()
    {
        instance = this;
    }
    
    void Start()
    {
        thisRope = this.gameObject;
    }

    void Update()
    {
        transform.RotateAround(player.transform.position, Vector3.forward, rotationSpeed * Time.deltaTime);
    }
}