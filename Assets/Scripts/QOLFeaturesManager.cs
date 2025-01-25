using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QOLFeaturesManager : MonoBehaviour
{
    private static QOLFeaturesManager instance;
    public static QOLFeaturesManager Instance => instance;

    private List<CollectibleHeader> collectibleHeaders;
    
    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    private void Update()
    {
        if (collectibleHeaders != null && collectibleHeaders.Count > 0)
        {
            foreach (CollectibleHeader header in collectibleHeaders)
            {
                if (header == null)
                    continue;

                Vector3 directionToTarget = header.transform.position - Camera.main.transform.position;
                directionToTarget.y = 0;

                if (directionToTarget.sqrMagnitude > 0.001f)
                {
                    header.transform.rotation = Quaternion.LookRotation(directionToTarget);
                }

            }
        }
    }

    public void RegisterCollectibleHeader(CollectibleHeader header)
    {
        if (collectibleHeaders == null)
        {
            collectibleHeaders = new List<CollectibleHeader>();
        }
        collectibleHeaders.Add(header);
    }
}
