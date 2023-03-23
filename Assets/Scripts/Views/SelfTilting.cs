using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SelfTilting : MonoBehaviour
{
    private Vector3 _angles;

    public float speed = 1;
    public float scale = 3;
    
    // Start is called before the first frame update
    void Start()
    {
        _angles = transform.eulerAngles;
    }

    // Update is called once per frame
    void Update()
    {
        var delta = Mathf.Sin(Time.time * speed) * scale;
        transform.rotation = Quaternion.Euler(_angles.x, _angles.y, _angles.z + delta);
    }
}