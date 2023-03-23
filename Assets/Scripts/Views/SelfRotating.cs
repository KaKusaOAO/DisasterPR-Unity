using UnityEngine;

public class SelfRotating : MonoBehaviour
{
    public float speed = 1;

    // Update is called once per frame
    void Update()
    {
        transform.rotation = Quaternion.Euler(0, 0, Time.time * speed);
    }
}