using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;
using UnityEngine.Animations;

public class MovingPlatform : MonoBehaviour
{
   public float speed = 2.5f;
   [Range(0.01f, 0.5f)] public float fadePercent = 0.1f;
   public AnimationCurve speedFadeCurve;
   
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

   public enum MovementTypes
   {
      ConstantVelocity,
      Smooth
   }

   private LinkedList<Vector3> _wayPoints;
   private LinkedListNode<Vector3> _targetPoint;
   private LinkedListNode<Vector3> _prevPoint;
   private float _disBetweenPoints;

   private Rigidbody _body;
   private Vector3 _velocity;

   private bool _inversed;
   private bool _playerEntered;

   public Vector3 GetVelocity()
   {
      return _velocity;
   }
   
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
      if (Application.isPlaying)
      {
         Debug.DrawLine(transform.position, transform.position + _velocity * 5f, Color.red);
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
      for (int i = transform.childCount-1; i > 0; --i)
      {
         if (transform.GetChild(i).name == "[Mesh]")
         {
            continue;
         }
         var pointPos = transform.GetChild(i).position;
         transform.GetChild(i).gameObject.SetActive(false);
         _wayPoints.AddLast(pointPos);
      }
      
      _body = GetComponent<Rigidbody>();
      _prevPoint = _wayPoints.First;
      _targetPoint = _prevPoint.Next;
      _velocity = (_targetPoint.Value - _body.transform.position).normalized * speed;
      _disBetweenPoints = Vector3.Distance(_targetPoint.Value, _prevPoint.Value);
      _body.velocity = _velocity;
   }
   
   private void OnTriggerEnter(Collider other)
   {
      if (_playerEntered)
      {
         return;
      }
      if (other.gameObject.CompareTag("Player"))
      {
         _playerEntered = true;
         other.transform.SetParent(transform);
      }
   }

   private void OnTriggerStay(Collider other)
   {
      if (other.gameObject.CompareTag("Player"))
      {
         other.transform.SetParent(transform);
      }
   }

   private void OnTriggerExit(Collider other)
   {
      if (other.gameObject.CompareTag("Player"))
      {
         other.transform.SetParent(null);
      }
   }

   private void FixedUpdate()
   {
      float distanceToTarget = Vector3.Distance(transform.position, _targetPoint.Value);
      float distanceFromPrev = Vector3.Distance(transform.position, _prevPoint.Value);
      
      if (distanceToTarget <= 0.1f) // == transform.position)
      {
         Debug.Log("Update Target");
         UpdateTargetPoint();
      }
      
      // float stopSpeedFade = Mathf.InverseLerp(0.0f, fadePercent * _disBetweenPoints, distanceToTarget);
      // float startSpeedFade = Mathf.InverseLerp(0.0f, fadePercent * _disBetweenPoints, distanceFromPrev);
      //float magnitude = Mathf.Min(stopSpeedFade, startSpeedFade) * speed;

      float fadeFactor = Mathf.InverseLerp(0, _disBetweenPoints, distanceFromPrev);
      Debug.Log(fadeFactor);
      float speedFactor = speedFadeCurve.Evaluate(fadeFactor);
      
      _body.velocity = _velocity.normalized * speed * speedFactor;
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
      
      Vector3 preTarget = _targetPoint.Value;
      _targetPoint = _inversed ? _targetPoint.Previous : _targetPoint.Next;
      _prevPoint = _inversed ? _targetPoint.Next : _targetPoint.Previous;
      
      if (_targetPoint != null) _velocity = (_targetPoint.Value - preTarget).normalized * speed;
      _body.velocity = _velocity;

      _disBetweenPoints = Vector3.Distance(_targetPoint.Value,preTarget);
   }
}
