<<<<<<< HEAD
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"--- UI CLICK DEBUG REPORT (TimeScale: {Time.timeScale}) ---");
            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    Debug.Log($"HIT: {result.gameObject.name} (Depth: {result.depth}, SortOrder: {result.sortingOrder})");
                }
            }
            else
            {
                Debug.Log("HIT NOTHING! (EventSystem is working, but found no Raycast Targets)");
            }
        }
    }
=======
using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections.Generic;

public class UIDebugger : MonoBehaviour
{
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PointerEventData pointerData = new PointerEventData(EventSystem.current)
            {
                position = Input.mousePosition
            };

            List<RaycastResult> results = new List<RaycastResult>();
            EventSystem.current.RaycastAll(pointerData, results);

            Debug.Log($"--- UI CLICK DEBUG REPORT (TimeScale: {Time.timeScale}) ---");
            if (results.Count > 0)
            {
                foreach (var result in results)
                {
                    Debug.Log($"HIT: {result.gameObject.name} (Depth: {result.depth}, SortOrder: {result.sortingOrder})");
                }
            }
            else
            {
                Debug.Log("HIT NOTHING! (EventSystem is working, but found no Raycast Targets)");
            }
        }
    }
>>>>>>> upstream/dev
}