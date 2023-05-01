using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEditor;
using UnityEngine;

// Icon used in editor
public enum IconColor
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

public class MovingPlatform : MonoBehaviour
{
   [Header("Speed Settings")]
   public float speed = 2.5f;
   public AnimationCurve speedFactorCurve = new AnimationCurve(
      new Keyframe[]
      {
         new Keyframe(0,1),
         new Keyframe(1,1)
      });
   
   public enum MovementTypes
   {
      ConstantVelocity,
      Smooth
   }

   private GameObject _meshObject;     // actual mesh child object
   
   private LinkedList<Vector3> _wayPoints;
   private LinkedListNode<Vector3> _targetPoint;
   private LinkedListNode<Vector3> _prevPoint;
   
   private float _disBetweenPoints;
   
   private Rigidbody _playerRb;
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
      if (Application.isPlaying)
      {
         return;
      }
      for (int i=0; i < transform.childCount - 1; i++)
      {
         if (transform.GetChild(i).name == "[Mesh]")
         {
            continue;
         }
         Vector3 curPoint = transform.GetChild(i).position;
         Vector3 nextPoint = transform.GetChild(i + 1).position;
         Gizmos.DrawLine(curPoint, nextPoint);
      }
   }
   
   public void ClearAllWayPoints()
   {
      if (UnityEditor.PrefabUtility.IsPartOfPrefabInstance(transform))
         UnityEditor.PrefabUtility.UnpackPrefabInstance(gameObject,
            UnityEditor.PrefabUnpackMode.Completely,
            UnityEditor.InteractionMode.AutomatedAction);
      
      GameObject[] childObjects = new GameObject[transform.childCount];
      for (int i = 0; i < transform.childCount; i++)
      {
         childObjects[i] = transform.GetChild(i).gameObject;
      }
      
      for(int i = 0; i < childObjects.Length; i++)
      {
         if (childObjects[i].name == "[Mesh]")
         {
            continue;
         }
#if UNITY_EDITOR
         DestroyImmediate(childObjects[i].gameObject);
          childObjects[i] = null;
#else
          Destory(childObjects[i].gameObject);
#endif
      }
   }
   
   public void InitializeWayPoints()
   {
      // Create two default start and end point
      var position = transform.position;
      GetNewWayPoint(position - Vector3.down * 5f, "start point", IconColor.Orange);
      GetNewWayPoint(position - Vector3.up * 5f, "end point", IconColor.Orange);
   }

   public GameObject GetNewWayPoint(Vector3? position, String n = "way point", IconColor color = IconColor.Teal)
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
            _meshObject = transform.GetChild(i).gameObject;
            continue;
         }
         var pointPos = transform.GetChild(i).position;
         transform.GetChild(i).gameObject.SetActive(false);
         _wayPoints.AddLast(pointPos);
      }
      
      _prevPoint = _wayPoints.First;
      _targetPoint = _prevPoint.Next;
      // Start at the middle between first and second point
      transform.position = Vector3.Lerp(_prevPoint.Value, _targetPoint.Value, 0.5f);
      
      _velocity = (_targetPoint.Value - transform.position).normalized * speed;
      _disBetweenPoints = Vector3.Distance(_targetPoint.Value, _prevPoint.Value);
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
         _playerRb = other.GetComponent<Rigidbody>();
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
         _playerRb.AddForce(other.transform.forward * _playerRb.mass, ForceMode.Impulse);
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
      
      float fadeFactor = Mathf.InverseLerp(0, _disBetweenPoints, distanceFromPrev);
      float speedFactor = speedFactorCurve.Evaluate(fadeFactor);
      transform.position = Vector3.MoveTowards(transform.position, _targetPoint.Value, 
         speedFactor * speed * Time.fixedDeltaTime);

      // float stopSpeedFade = Mathf.InverseLerp(0.0f, fadePercent * _disBetweenPoints, distanceToTarget);
      // float startSpeedFade = Mathf.InverseLerp(0.0f, fadePercent * _disBetweenPoints, distanceFromPrev);
      //float magnitude = Mathf.Min(stopSpeedFade, startSpeedFade) * speed;

      // float fadeFactor = Mathf.InverseLerp(0, _disBetweenPoints, distanceFromPrev);
      // float speedFactor = speedFadeCurve.Evaluate(fadeFactor);
      //
      // _body.velocity = _velocity.normalized * speed * speedFactor;
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

      _disBetweenPoints = Vector3.Distance(_targetPoint.Value,preTarget);
   }
}

[CustomEditor(typeof(MovingPlatform))]
public class MovingPlatformEditor : Editor
{
   // temporary list, for displaying data
   private List<GameObject> _wayPoints = new List<GameObject>();
   
   public override void OnInspectorGUI()
   {
      // In play mode, switch to default inspector
      if (Application.isPlaying)
      {
         base.OnInspectorGUI();
         return;
      }
      
      MovingPlatform platform = (MovingPlatform)target;
      
      _wayPoints.Clear();
      for (int i = 0; i < platform.transform.childCount; ++i)
      {
         if (platform.transform.GetChild(i).name == "[Mesh]")
         {
            continue;
         }
         _wayPoints.Add(platform.transform.GetChild(i).gameObject);
      }
      
      GUILayout.BeginHorizontal();
      GUILayout.Label("Way Points" + " (" + _wayPoints.Count + ")");
      GUILayout.EndHorizontal();
      
      for (int i = 0; i < _wayPoints.Count; i++)
      {
         GUILayout.BeginHorizontal();
         GUILayout.Label(_wayPoints[i].name, GUILayout.Width(120));

         if (GUILayout.Button("Select"))
         {
            Selection.activeGameObject = _wayPoints[i];
         }
         if (GUILayout.Button("Add Before"))
         {
            int childIndex = _wayPoints[i].transform.GetSiblingIndex();
            GameObject newGO = platform.GetNewWayPoint(null, "way point", IconColor.Blue);
            newGO.transform.SetSiblingIndex(childIndex);
         }
         if (GUILayout.Button("Add After"))
         {
            int childIndex = _wayPoints[i].transform.GetSiblingIndex();
            GameObject newGO = platform.GetNewWayPoint(null, "way point", IconColor.Blue);
            newGO.transform.SetSiblingIndex(childIndex+1);
         }
         if (GUILayout.Button("Remove"))
         {
            DestroyImmediate(_wayPoints[i]);
         }
         GUILayout.EndHorizontal();
      }
      
      GUILayout.Space(10);
      
      if (GUILayout.Button("Reset waypoints"))
      {
         platform.ClearAllWayPoints();
         platform.InitializeWayPoints();
      }
      
      GUILayout.Space(10);
      
      base.OnInspectorGUI();
   }
}