using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class MovingPlatform : MonoBehaviour
{
   public float speed = 2.5f;
   
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
   
   private LinkedList<Vector3> _wayPoints;
   private LinkedListNode<Vector3> _targetPoint;
   private bool _inversed;

   private void Reset()
   {
      ClearAllWayPoints();
      InitializeWayPoints();
   }

   private void OnDrawGizmos()
   {
      for (int i=0; i < transform.childCount - 1; i++)
      {
         Vector3 curPoint = transform.GetChild(i).position;
         Vector3 nextPoint = transform.GetChild(i + 1).position;
         Gizmos.DrawLine(curPoint, nextPoint);
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
   
   void InitializeWayPoints()
   {
      // Create two default start and end point
      var position = transform.position;
      GetNewWayPoint(position - Vector3.down * 5f, "start point", IconColor.Orange);
      GetNewWayPoint(position - Vector3.up * 5f, "end point", IconColor.Orange);
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
      
      // Add icon to way point in editor
      GUIContent iconContent = EditorGUIUtility.IconContent( $"sv_label_{(int)color}");
      EditorGUIUtility.SetIconForObject(newWayPoint, (Texture2D) iconContent.image);
      return newWayPoint;
   }

   private void Start()
   {
      _wayPoints?.Clear();
      _wayPoints = new LinkedList<Vector3>();
      for (int i = transform.childCount; i > 0; --i)
      {
         var pointPos = transform.GetChild(0).position;
         DestroyImmediate(transform.GetChild(0).gameObject);
         _wayPoints.AddLast(pointPos);
      }
      _targetPoint = _wayPoints.First;
   }

   private void OnTriggerEnter(Collider other)
   {
      if (other.gameObject.CompareTag("Player"))
      {
         other.transform.SetParent(transform);
      }
   }

   private void OnCollisionExit(Collision other)
   {
      if (other.gameObject.CompareTag("Player"))
      {
         other.transform.SetParent(null);
      }
   }

   private void FixedUpdate()
   {
      if (_targetPoint.Value == transform.position)
      {
         UpdateTargetPoint();
      }

      var step = speed * Time.fixedDeltaTime;
      transform.position = Vector3.MoveTowards(transform.position, _targetPoint.Value, step);
   }

   private void UpdateTargetPoint()
   {
      if (_targetPoint.Next == null)
      {
         _inversed = true;
      }

      if (_targetPoint.Previous == null)
      {
         _inversed = false;
      }

      _targetPoint = _inversed ? _targetPoint.Previous : _targetPoint.Next;
   }
}
