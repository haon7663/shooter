using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour
{
    private Rigidbody2D m_Rigidbody2D;
    private Transform m_PlayerSprite;

    [Space]
    [Header("Stats")]
    public float m_Speed;
    public float m_RotateSpeed;

    float x, y;

    private void Start()
    {
        m_Rigidbody2D = GetComponent<Rigidbody2D>();
        m_PlayerSprite = transform.GetChild(0);
    }

    private void Update()
    {
        x = Input.GetAxis("Horizontal");
        y = Input.GetAxis("Vertical");
    }
    private void FixedUpdate()
    {
        m_Rigidbody2D.velocity = new Vector2(x, y) * m_Speed * Time.deltaTime;

        m_PlayerSprite.Rotate(new Vector3(0, 0, (m_Rigidbody2D.velocity.x > 0 ? m_Rigidbody2D.velocity.x : -m_Rigidbody2D.velocity.x) + 
                                                (m_Rigidbody2D.velocity.y > 0 ? m_Rigidbody2D.velocity.y : -m_Rigidbody2D.velocity.y)) * m_RotateSpeed);
    }
}