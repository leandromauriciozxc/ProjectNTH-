using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ElevatorDoor : MonoBehaviour
{
    [SerializeField]
    private GameObject m_leftDoor;
    [SerializeField]
    private GameObject m_rightDoor;
    [SerializeField]
    private float m_openDistanceModifier;
    [SerializeField]
    private float m_durationModifier;
    [SerializeField]
    private float m_stayOpenDuration;

    public bool m_willOpen;

    private Vector3 m_leftDoorOpenPos;
    private Vector3 m_rightDoorOpenPos;
    private Vector3 m_leftDoorClosePos;
    private Vector3 m_rightDoorClosePos;


    void Start()
    {
        m_leftDoorClosePos = m_leftDoor.transform.position;
        m_rightDoorClosePos = m_rightDoor.transform.position;
        m_leftDoorOpenPos = m_leftDoorClosePos + Vector3.left * m_openDistanceModifier;
        m_rightDoorOpenPos = m_rightDoorClosePos + Vector3.right * m_openDistanceModifier;
    }
    void Update()
    {
        OpenDoor();
    }

    public void OpenDoor()
    {
        if (m_willOpen)
        {
            StartCoroutine(DoorSequence());
        }
    }
    public IEnumerator DoorSequence()
    {
        m_willOpen = false;
        yield return MoveDoors(m_leftDoorClosePos, m_leftDoorOpenPos, m_rightDoorClosePos, m_rightDoorOpenPos);
        yield return new WaitForSeconds(m_stayOpenDuration);
        yield return MoveDoors(m_leftDoorOpenPos, m_leftDoorClosePos, m_rightDoorOpenPos, m_rightDoorClosePos);
    }
    private IEnumerator MoveDoors(Vector3 leftStart, Vector3 leftEnd, Vector3 rightStart, Vector3 rightEnd)
    {
        float elapsedTime = 0f;
        while (elapsedTime < m_durationModifier)
        {
            elapsedTime += Time.deltaTime;
            float t = elapsedTime / m_durationModifier;
            m_leftDoor.transform.position = Vector3.Lerp(leftStart, leftEnd, t);
            m_rightDoor.transform.position = Vector3.Lerp(rightStart, rightEnd, t);
            yield return null;
        }
        m_leftDoor.transform.position = leftEnd;
        m_rightDoor.transform.position = rightEnd;
    }
}
