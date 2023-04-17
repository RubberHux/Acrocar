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

    public PopupText GetPopupText(String message, Color color, Transform trans, int scale)
    {
        PopupText pt = Pool.Get();
        pt.SetText(message);
        pt.SetColor(color);
        pt.SetPosition(trans.position + Vector3.up * heightOffset);
        pt.SetScale(scale);
        pt.transform.SetParent(_parent.transform);
        return pt;
    }

    public void HidePopupText(PopupText pt)
    {
        pt.SetText("");
        Pool.Release(pt);
    }
}
public class PopupTextGenerator : MonoBehaviour
{
    [SerializeField] private PopupText ptPrefab;
    
    private PopupTextPool _popupTextPool;
    
    public static PopupTextGenerator Instance;

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

    public PopupText Generate(String message, Color color, Transform trans, int scale)
    {
        return _popupTextPool.GetPopupText(message, color, trans, scale);
    }

    public void HidePopupText(PopupText pt)
    {
        _popupTextPool.HidePopupText(pt);
    }
}
