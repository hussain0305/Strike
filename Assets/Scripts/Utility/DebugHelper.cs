using UnityEngine;

public static class DebugHelper
{
    public static void DrawOverlapBox(Vector3 center, Vector3 halfExtents, Quaternion rotation)
    {
        Vector3[] corners = new Vector3[8];
        int i = 0;

        for (int x = -1; x <= 1; x += 2)
        {
            for (int y = -1; y <= 1; y += 2)
            {
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 localCorner = new Vector3(x * halfExtents.x, y * halfExtents.y, z * halfExtents.z);
                    corners[i++] = center + rotation * localCorner;
                }
            }
        }

        Debug.DrawLine(corners[0], corners[1], Color.yellow, 20);
        Debug.DrawLine(corners[1], corners[3], Color.yellow, 20);
        Debug.DrawLine(corners[3], corners[2], Color.yellow, 20);
        Debug.DrawLine(corners[2], corners[0], Color.yellow, 20);

        Debug.DrawLine(corners[4], corners[5], Color.yellow, 20);
        Debug.DrawLine(corners[5], corners[7], Color.yellow, 20);
        Debug.DrawLine(corners[7], corners[6], Color.yellow, 20);
        Debug.DrawLine(corners[6], corners[4], Color.yellow, 20);

        Debug.DrawLine(corners[0], corners[4], Color.yellow, 20);
        Debug.DrawLine(corners[1], corners[5], Color.yellow, 20);
        Debug.DrawLine(corners[2], corners[6], Color.yellow, 20);
        Debug.DrawLine(corners[3], corners[7], Color.yellow, 20);
    }

}
