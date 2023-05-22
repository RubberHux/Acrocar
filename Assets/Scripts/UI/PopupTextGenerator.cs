using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Pool;
using Object = UnityEngine.Object;

public class PopupTextPool
{
    [SerializeField] private float heightOffset = 1.0f;
    private PopupText _prefab;
    private GameObject _parent;
    public PopupTextPool(PopupText prefab, GameObject parent)
    {
        _prefab = prefab;
        _parent = parent;
        CreatePool();
    }
    
    private int maxPoolSize = 10;

    private ObjectPool<PopupText> _pool;

    public ObjectPool<PopupText> Pool
    {
        get
        {
            if (_pool == null)
            {
                CreatePool();
            }

            return _pool;
        }
    }

    private void CreatePool()
    {
        _pool = new ObjectPool<PopupText>(
                OnCreatePoolItem, OnGetFromPool, OnReleaseToPool, OnDestroyPool, 
                true, 10, maxPoolSize);
    }

    private void OnDestroyPool(PopupText pt)
    {
       Object.Destroy(pt.gameObject);
    }

    private void OnReleaseToPool(PopupText pt)
    {
        pt.gameObject.SetActive(false);
    }

    private void OnGetFromPool(PopupText pt)
    {
        pt.gameObject.SetActive(true);
    }

    private PopupText OnCreatePoolItem()
    {
        PopupText newPopupText = Object.Instantiate(_prefab);
        return newPopupText;
    }
    
    public PopupText GetPopupText(String message, Color color, Vector3 position, float scale, Transform playerTransform)
    {
        PopupText pt = Pool.Get();
        pt.SetText(message);
        pt.SetColor(color);
        pt.SetPosition(position + Vector3.up * heightOffset);
        pt.SetScale(scale);
        pt.transform.SetParent(_parent.transform);
        pt.SetFollowTarget(playerTransform);
        return pt;
    }
    
    public PopupText GetPopupText(String message, Color color, Vector3 position, float scale)
    {
        PopupText pt = Pool.Get();
        pt.SetText(message);
        pt.SetColor(color);
        pt.SetPosition(position + Vector3.up * heightOffset);
        pt.SetScale(scale);
        pt.transform.SetParent(_parent.transform);
        return pt;
    }

    public async void HidePopupText(PopupText pt)
    {
        await pt.HideText();
        pt.SetText("");
        Pool.Release(pt);
    }
}
public class PopupTextGenerator : MonoBehaviour
{
    [SerializeField] private PopupText ptPrefab;
    
    private PopupTextPool _popupTextPool;
    
    public static PopupTextGenerator Instance;  // Singleton

    private void OnEnable()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
        
        // if null create new pool
        _popupTextPool ??= new PopupTextPool(ptPrefab, gameObject);
    }

    public PopupText Generate(String message, Color color, Vector3 position, float scale)
    {
        return _popupTextPool.GetPopupText(message, color, position, scale);
    }
    
    public PopupText Generate(String message, Color color, Vector3 position, float scale, Transform playerTransform)
    {
        return _popupTextPool.GetPopupText(message, color, position, scale, playerTransform);
    }

    public void HidePopupText(PopupText pt)
    {
        _popupTextPool.HidePopupText(pt);
    }
}
