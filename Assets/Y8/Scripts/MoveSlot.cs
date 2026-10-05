using System.Collections;
using UnityEngine;

public class MoveSlot : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    private bool isBannerUp = false;
    private Vector3 originalPos;
    public float moveDistance = 100f; // adjust how far up it moves
    public float moveSpeed = 5f;

    void Start()
    {
        originalPos = transform.position;
    }

    public void ButtonMoveBanners()
    {
        isBannerUp = !isBannerUp;

        Vector3 targetPos = isBannerUp
            ? originalPos + new Vector3(0, moveDistance, 0)
            : originalPos;

        StopAllCoroutines();
        StartCoroutine(MoveToPosition(targetPos));
    }

    private IEnumerator MoveToPosition(Vector3 target)
    {
        while (Vector3.Distance(transform.position, target) > 0.01f)
        {
            transform.position = Vector3.MoveTowards(transform.position, target, moveSpeed * Time.deltaTime);
            yield return null;
        }
        transform.position = target;
    }
}
