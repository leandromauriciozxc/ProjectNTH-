using UnityEngine;

public class HeadBob : MonoBehaviour
{
    [SerializeField]
    private float walkSpeed = 6f;
    [SerializeField]
    private float runSpeed = 9f;
    [SerializeField]
    private float bobAmount = 0.03f;

    float timer;
    Vector3 startPos;

    void Start()
    {
        startPos = transform.localPosition;
    }

    public void UpdateBob(Vector3 moveDir, bool running)
    {
        if (moveDir.magnitude > 0.1f)
        {
            float speed = running ? runSpeed : walkSpeed;

            timer += Time.deltaTime * speed;

            float y = Mathf.Sin(timer) * bobAmount;
            float x = Mathf.Cos(timer * 0.5f) * bobAmount * 0.5f;

            transform.localPosition = startPos + new Vector3(x, y, 0);
        }
        else
        {
            timer = 0;
            transform.localPosition = Vector3.Lerp(transform.localPosition, startPos, Time.deltaTime * 5f);
        }
    }
}