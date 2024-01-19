using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using YukiFrameWork.Knaspack;
using YukiFrameWork.ABManager;

public class KnapsackExample : MonoBehaviour
{
    [Header("资源模块配置名")]
    public string projectName;  
    // Start is called before the first frame update    
    IEnumerator Start()
    {
        while (true)
        {
            CheckResUpdateRequest checkRes = AssetBundleManager.CheckResUpdate(projectName);
            yield return checkRes;

            if (string.IsNullOrEmpty(checkRes.error))
            {
                if (checkRes.result.updateType == UpdateType.DontNeedUpdate)
                    break;
            }
            else continue;

            ReadyResRequest request = AssetBundleManager.ReadyRes(checkRes.result);

            if (string.IsNullOrEmpty(request.error))
            {
                while (!request.isDone)
                {
                    yield return null;

                    switch (request.ExecutionType)
                    {
                        case ExecutionType.Download:
                            break;
                        case ExecutionType.Decompression:
                            break;
                        case ExecutionType.Verify:
                            break;
                        case ExecutionType.ExtractLocal:
                            break;
                        default:
                            break;
                    }
                }

                break;
            }
            
        }

        ItemKit.Init(projectName,"ItemUI");
       
    }

    // Update is called once per frame
    void Update()
    {
        var panel = FindObjectOfType<Inventory>();

        if (Input.GetKeyDown(KeyCode.A))
        {
            int id = Random.Range(1, 15);
           // Debug.Log(id);
            panel.StoreItem(ItemKit.Config.GetItemByID(id));
        }
    }

  
}
