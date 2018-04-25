using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour
{

    protected LineRenderer lineRenderer;

    protected virtual void Awake()
    {
        if (this.lineRenderer == null)
        {
            this.lineRenderer = new GameObject("LineRenderer").AddComponent<LineRenderer>();
        }
    }

    protected virtual void Update()
    {
        RaycastHit hit;
        Vector3 origin = Camera.main.ViewportToWorldPoint(new Vector3(0.5f, 0.5f, 0f));
        if (Physics.Raycast(origin, Camera.main.transform.forward, out hit, 100f, LayerMask.GetMask("Terrain")))
        {
            this.lineRenderer.SetPositions(new Vector3[] {
                origin,
                hit.point
            });
            Vector3 normal = hit.normal;
            normal.x = (float)decimal.Truncate((decimal)normal.x);
            normal.y = (float)decimal.Truncate((decimal)normal.y);
            normal.z = (float)decimal.Truncate((decimal)normal.z);
            Vector3 position = hit.point;
            Debug.LogFormat("Point: {0}", hit.point);
            Debug.LogFormat("Normal: {0}", normal);
            if (normal.x > 0)
            {
                position.x -= normal.x;
            }
            if (normal.y > 0)
            {
                position.y -= normal.y;
            }
            if (normal.z > 0)
            {
                position.z -= normal.z;
            }
            position.x = Mathf.Floor(position.x);
            position.y = Mathf.Floor(position.y);
            position.z = Mathf.Floor(position.z);
            Debug.LogFormat("Position: {0}", position);
        }
    }
}