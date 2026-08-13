using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AIBrain : MonoBehaviour
{
    public Vector3 MoveDirection { get; private set; }
    public bool WantsToDash { get; private set; }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Tick(Transform self, Transform target, AIData data, float deltaTime)
    {

    }
}
