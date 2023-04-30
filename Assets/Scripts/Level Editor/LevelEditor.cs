using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class LevelEditor : MonoBehaviour
{
    public GameObject level; // container object for entire level
    public GameObject spawnPoint; // initial spawner for car
    public GameObject carLoader; // car loader object (prefab)
    public GameObject editorCamera; // camera for level editor (separate from when playing)
    public List<GameObject> objectList; // list of objects which can be created
    public Material normalMaterial; // default material to slap on objects
    public Material grappleMaterial; // material for grappleable stuff
    public Material lavaMaterial; // material for lava
    public Material breakableMaterial; // material for breakable stuff
    public TMP_Text addText; // text showing which object to add
    public TMP_Dropdown editorModes; // modes for the editor

    private GameObject currObject; // current object selected
    private GameObject copiedObject; // object currently copied
    private GameObject spawnedCarLoader; // actual car loader spawned when playtesting
    private bool interacting; // is current object being interacted with?
    private Vector3 clickPoint; // point on object where it was clicked
    private Vector3 prevMousePos; // previous mouse pos (used for delta calcs)
    private Vector3 moveOffset; // offset between click point and object pos
    private int objectIndex; // current index of objectList
    private bool playing; // playtesting the level?

    private enum EditorMode { Move, Scale, Rotate };
    private EditorMode currentMode;

    private void Start()
    {
        objectIndex = 0;
        interacting = false;
        playing = false;
        editorModes.onValueChanged.AddListener(delegate { ChangeMode(); });
        currentMode = EditorMode.Move;
        carLoader.GetComponent<CarLoader>().is2D = true;
    }

    // Update is called once per frame
    void Update()
    {
        if (!playing)
        {
            Vector3 mousePos = Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, Camera.main.transform.position.x));
            //Debug.Log(mousePos);
            if (!interacting && (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1)))
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;

                if (Physics.Raycast(ray, out hit, 100))
                {
                    Debug.Log(hit.transform.name);
                    currObject = hit.transform.gameObject;
                    HighlightObject(true);

                    clickPoint = new Vector3(0, mousePos.y, mousePos.z);
                    moveOffset = clickPoint - currObject.transform.position;
                    interacting = true;
                }
                else
                {
                    if (currObject) HighlightObject(false);
                    currObject = null;

                    if (Input.GetMouseButtonDown(1)) CreateObject(objectList[objectIndex], mousePos);
                }
            }
            else interacting = false;

            if (currObject)
            {
                if (Input.GetMouseButton(0))
                {
                    Vector3 mouseDelta = mousePos - prevMousePos;

                    switch (currentMode)
                    {
                        case EditorMode.Move:
                            currObject.transform.SetPositionAndRotation(mousePos - moveOffset, currObject.transform.rotation);
                            break;

                        case EditorMode.Scale:
                            currObject.transform.localScale += mouseDelta;
                            break;

                        case EditorMode.Rotate:
                            Quaternion rot = currObject.transform.rotation;
                            rot.x += mouseDelta.z / 10;
                            currObject.transform.SetPositionAndRotation(currObject.transform.position, rot);
                            break;
                    }

                }

                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.C)) copiedObject = currObject;
                if (Input.GetKeyDown(KeyCode.Backspace) && currObject != spawnPoint) Destroy(currObject);
            }

            if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.V)) CreateObject(copiedObject, mousePos);

            prevMousePos = mousePos;

            objectIndex = (int)Mathf.Clamp(objectIndex + Input.mouseScrollDelta.y, 0, objectList.Count - 1);
            addText.text = "Add object: " + objectList[objectIndex].name;
        }
    }

    private void ChangeMode()
    {
        currentMode = (EditorMode)editorModes.value;
    }

    private void CreateObject(GameObject origObject, Vector3 spawnPos)
    {
        Debug.Log("Creating " + objectList[objectIndex].name);
        GameObject newObject = Instantiate(origObject, spawnPos, Quaternion.identity);
        newObject.name = origObject.name;
        newObject.transform.parent = level.transform;
    }

    private void HighlightObject(bool highlight)
    {
        Color objColor = currObject.GetComponent<MeshRenderer>().material.color;
        //if (highlight) objColor = new Color(objColor.r, objColor.g, objColor.b, 1.0f);
        //else objColor = new Color(objColor.r, objColor.g, objColor.b, 0.0f);
        currObject.GetComponent<MeshRenderer>().material.SetColor("_Color", objColor);
    }

    public void Play()
    {
        if (!playing)
        {
            spawnedCarLoader = Instantiate(carLoader, spawnPoint.transform.position, Quaternion.identity);
            //Instantiate(carLoader, spawnPoint.transform);

            spawnPoint.SetActive(false);
            editorCamera.SetActive(false);
            editorModes.gameObject.SetActive(false);
            addText.gameObject.SetActive(false);

            playing = true;
        }
        else
        {
            Destroy(spawnedCarLoader);

            spawnPoint.SetActive(true);
            editorCamera.SetActive(true);
            editorModes.gameObject.SetActive(true);
            addText.gameObject.SetActive(true);

            playing = false;
        }
    }
}
