using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelEditor : MonoBehaviour
{
    public GameObject level; // container object for entire level
    public List<GameObject> objectList; // list of objects which can be created
    public Material normalMaterial; // default material to slap on objects
    public Material grappleMaterial; // material for grappleable stuff
    public Material lavaMaterial; // material for lava
    public Material breakableMaterial; // material for breakable stuff
    public TMP_Text addText; // text showing which object to add

    private GameObject currObject; // current object selected
    private Vector3 clickPoint; // point on object where it was clicked
    private Vector3 prevMousePos; // previous mouse pos (used for delta calcs)
    private Vector3 moveOffset; // offset between click point and object pos
    private float objectIndex; // current index of objectList

    private void Start()
    {
        objectIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.x));
        //Debug.Log(mousePos);
        if (!currObject && (Input.GetMouseButtonDown(0) || Input.GetMouseButton(1)))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, 100))
            {
                Debug.Log(hit.transform.name);
                currObject = hit.transform.gameObject;
                clickPoint = new Vector3(0, mousePos.y, mousePos.z);

                moveOffset = clickPoint - currObject.transform.position;
            }
            else CreateObject();
        }
        if ((Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1)) && currObject) currObject = null;

        if (currObject)
        {
            if (Input.GetMouseButton(0))
            {
                Quaternion rot = Quaternion.identity;
                //rot.x = Input.mouseScrollDelta.y;
                currObject.transform.SetPositionAndRotation(mousePos - moveOffset, rot);
            }
            else if (Input.GetMouseButton(1))
            {
                Vector3 mouseDelta = mousePos - prevMousePos;
                currObject.transform.localScale += mouseDelta;
            }
        }

        prevMousePos = mousePos;

        objectIndex = Mathf.Clamp(objectIndex + Input.mouseScrollDelta.y, 0, objectList.Count - 1);
        addText.text = "Add object: " + objectIndex;
    }

    public void CreateObject()
    {
        Debug.Log("Bazinga!!!");
        Instantiate(objectList[(int)objectIndex], level.transform);
    }
}
