using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimateCamera : MonoBehaviour
{
    [SerializeField]
    private bool m_enable;
    [SerializeField]
    private bool m_animateParent;
    [SerializeField]
    private bool m_moveX, m_xMovePositive;
    [SerializeField]
    private bool m_moveZ, m_yMovePostivie;
    [SerializeField]
    private bool m_rotate;

    private enum MovementState
    {
        xPosZPos = 0,
        xPosZNeg = 1,
        xNegZPos = 2,
        xNegZNeg = 3
    }
    [SerializeField]
    private MovementState m_movementState = MovementState.xPosZPos;
    [SerializeField]
    private Vector3 m_movementDirection;
    [SerializeField]
    [Range(0, 10)]
    private float m_rotationSpeed = 1;
    [SerializeField]
    [Range(0, 1000)]
    private float m_movementSpeed = 1;

    [SerializeField]
    private Transform m_objectToMove;

    private void Awake()
    {
        if (m_animateParent)
        {
            m_objectToMove = this.transform.parent;
        }
        else
        {
            m_objectToMove = this.transform;
        }
        CheckState();
    }

    private void CheckState()
    {
        if (m_xMovePositive && m_yMovePostivie)
        {
            m_movementState = MovementState.xPosZPos;
        }
        else if (m_xMovePositive && !m_yMovePostivie)
        {
            m_movementState = MovementState.xPosZNeg;
        }
        else if (!m_xMovePositive && m_yMovePostivie)
        {
            m_movementState = MovementState.xNegZPos;
        }
        else if (!m_xMovePositive && !m_yMovePostivie)
        {
            m_movementState = MovementState.xNegZNeg;
        }

        switch (m_movementState)
        {
            case MovementState.xNegZNeg:
                m_movementDirection = new Vector3(-1 * m_movementSpeed * Time.deltaTime, 0, -1 * m_movementSpeed * Time.deltaTime);
                break;
            case MovementState.xNegZPos:
                m_movementDirection = new Vector3(-1 * m_movementSpeed * Time.deltaTime, 0, 1 * m_movementSpeed * Time.deltaTime);
                break;
            case MovementState.xPosZNeg:
                m_movementDirection = new Vector3(1 * m_movementSpeed * Time.deltaTime, 0, -1 * m_movementSpeed * Time.deltaTime);
                break;
            case MovementState.xPosZPos:
                m_movementDirection = new Vector3(1 * m_movementSpeed * Time.deltaTime, 0, 1 * m_movementSpeed * Time.deltaTime);
                break;
            default:
                break;
        }

        if (!m_moveX)
        {
            m_movementDirection.x = 0;
        }

        if (!m_moveZ)
        {
            m_movementDirection.z = 0;
        }

    }

    private void UpdateMovement(MovementState i_movementState)
    {
        m_objectToMove.transform.Translate(m_movementDirection);
    }

    // Update is called once per frame
    void Update()
    {
        if (m_objectToMove != null && m_enable)
        {
            CheckState();
            UpdateMovement(m_movementState);

            if (m_rotate)
            {
                m_objectToMove.transform.Rotate(new Vector3(0, 1 * m_rotationSpeed * Time.deltaTime, 0));
            }
        }
    }
}
