using UnityEngine;

public interface IActorMotion
{
    void StartMotion(Vector3? newInitialLocalPos = null);
    void StopMotion();
}