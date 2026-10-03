using System.Collections;
using UnityEngine;
using Yarn.Unity;
public class PlayerMover : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 3f;
    [SerializeField] private Transform target;
    [SerializeField] private AudioSource runSFX;

    private Coroutine moveRoutine;

    private void Start()
    {
      
    }
    [YarnCommand("move")]
    public void MoveTo()
    {
        if (target == null)
            return;

        if (moveRoutine != null)
            StopCoroutine(moveRoutine);
        runSFX.Play();
        moveRoutine = StartCoroutine(MoveRoutine(target));
    }

    private IEnumerator MoveRoutine(Transform target)
    {
        while (Vector3.Distance(transform.position, target.position) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(
                transform.position,
                target.position,
                moveSpeed * Time.deltaTime
            );

            yield return null;
        }

        transform.position = target.position;
        moveRoutine = null;
        runSFX.Stop();
    }
}