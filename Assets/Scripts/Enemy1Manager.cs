using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy1Manager : MonoBehaviour
{
    public Transform _startPosition;
    public Transform _targetPosition;
    [SerializeField] private float _speed = 1000f;
    public bool pingPong = true;  // İleri-geri hareket
    private float progress = 0.0f;       // Hareket ilerlemesi
    private bool isReversing = false;    // Geri hareket kontrolü

    void Start()
    {
        
    }

    void FixedUpdate()
    {
        MoveTowardsTarget();
    }



    void MoveTowardsTarget()
    {
        if (!transform) return;

        progress += Time.deltaTime * _speed * (isReversing ? -1 : 1);

        transform.position = Vector3.Lerp(_startPosition.position, _targetPosition.position, progress);

        if (progress >= 1.0f)
        {
            if (pingPong)
            {
                isReversing = true;
                progress = 1.0f; 
                transform.rotation = Quaternion.Euler(0, 0, 270);
            }
            else
            {
                progress = 0.0f; 
                transform.rotation = Quaternion.Euler(0, 0, 90);
            }
        }
        else if (progress <= 0.0f && pingPong)
        {
            isReversing = false;
            progress = 0.0f;
            transform.rotation = Quaternion.Euler(0, 0, 90);
        }
    }



}