using UnityEngine;

public interface IPathOffsetProvider
{
    Vector3 GetOffset(float t); // 0~1，返回当前时间点的偏移
}