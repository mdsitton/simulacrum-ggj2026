using UnityEngine;

public class ShipController : MonoBehaviour
{

    public bool takeOff = false;

    public float wait = 2f;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (takeOff && wait > 0)
        {
            wait -= Time.deltaTime;
        }
        else if (takeOff && wait <= 0)
        {
            transform.position += new Vector3(0, 1, 0) * Time.deltaTime * 1f;
        }
        
    }
}
