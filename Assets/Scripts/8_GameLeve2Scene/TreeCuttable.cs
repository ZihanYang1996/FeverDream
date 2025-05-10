using UnityEngine;

public class TreeCuttable : MonoBehaviour
{
    public bool hasBeenCut = false;
    public Transform topPart; // Assign in Inspector

    public float moveUpDistance = 0.5f;
    public float rotateAngle = 10f;
    public float fadeOutDuration = 1.0f;
    public float moveDuration = 0.4f;

    public void Cut()
    {
        if (hasBeenCut) return;
        hasBeenCut = true;

        ActorController actor = topPart.GetComponent<ActorController>();
        if (actor != null)
        {
            actor.MoveByDelta(Vector3.up * moveUpDistance, moveDuration);
            actor.RotateByDelta(rotateAngle, moveDuration);
            actor.FadeToAlpha(0f, fadeOutDuration, () => Destroy(topPart.gameObject));
        }
        else
        {
            Debug.LogWarning("No ActorController found on top part.");
        }
    }
}
