using System;
using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;
using Object = UnityEngine.Object;

public class MovingPlatform : MonoBehaviour
{
   // Icon used in editor
   private enum IconColor
   {
      Gray = 0,
      Blue,
      Teal,
      Green,
      Yellow,
      Orange,
      Red,
      Purple
   }
   
   [SerializeField] private List<GameObject> wayPoints;
   
   private void Reset()
   {
      if (wayPoints!=null)
      {
         ClearAllWayPoints();
      }
      
      wayPoints = InitializeWayPoints();
   }

   private void OnDrawGizmos()
   {
      foreach (var wayPoint in wayPoints)
      {
         
      }
   }
   
   void ClearAllWayPoints()
   {
      for(int i = transform.childCount; i>0; --i)
      {
         #if UNITY_EDITOR
          DestroyImmediate(transform.GetChild(0).gameObject);
        #else
          Destory(transform.GetChild(0).gameObject);
        #endif
      }
   }
   
   List<GameObject> InitializeWayPoints()
   {
      List<GameObject> startAndEndpoints = new List<GameObject>();
      // Create two default point and add to list
      var position = transform.position;
      GameObject startPoint = GetNewWayPoint(position - Vector3.down * 5f, "start point", IconColor.Orange);
      GameObject endPoint = GetNewWayPoint(position - Vector3.up * 5f, "end point", IconColor.Orange);
      startAndEndpoints.Add(startPoint);
      startAndEndpoints.Add(endPoint);
      return startAndEndpoints;
   }

   GameObject GetNewWayPoint(Vector3? position, String n = "way point", IconColor color = IconColor.Teal)
   {
      GameObject newWayPoint = new GameObject(n)
      {
         transform =
         {
            position = position ?? transform.position,
            parent = transform
         }
      };

      newWayPoint.AddComponent<Rigidbody>();
      // Add icon to way point in editor
      GUIContent iconContent = EditorGUIUtility.IconContent( $"sv_label_{(int)color}");
      EditorGUIUtility.SetIconForObject(newWayPoint, (Texture2D) iconContent.image);
      return newWayPoint;
   }

   private void OnDisable()
   {
      ClearAllWayPoints();
   }

   private void FixedUpdate()
   {
      
   }
}
