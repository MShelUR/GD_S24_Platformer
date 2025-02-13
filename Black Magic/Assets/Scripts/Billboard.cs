using UnityEngine;
using System.Collections;
using System.Collections.Generic; // dictionaries
using System;

public class Billboard : MonoBehaviour
{
    
    bool opened = false;
    int open_frames = 0;
    Vector3 orig_position;
    GameObject watch_open;

    //GameObject background = transform.GetChild(0).GameObject;
    //GameObject billboard = transform.GetChild(1).GameObject;
    //GameObject text = billboard.transform.GetChild(0).GameObject;

    Dictionary<string, GameObject> parts; // init movelist here so update sees it

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        parts = new Dictionary<string, GameObject>();
        orig_position = transform.position;

        foreach (Transform childTransform in this.GetComponentsInChildren<Transform>())
        {
            GameObject child = childTransform.gameObject;
            //print(child);
            if (parts.ContainsKey(child.name) == false) { // no duplicates
                parts.Add(child.name, child);
            }
            foreach (Transform childTransform2 in child.GetComponentsInChildren<Transform>()) {
                GameObject child2 = childTransform2.gameObject;
                if (parts.ContainsKey(child2.name) == false) { // no duplicates
                    parts.Add(child2.name, child2);
                    print(child2.name);
                    if (child2.name == "dieded") {
                        print("found dieded");
                        transform.position = transform.position + new Vector3(999,999,999);
                        child2.transform.position = child2.transform.position - new Vector3(999,999,999);
                        watch_open = child2;
                    }
                }
            }
        }

        //print(parts["background"]);
        //print(parts["billboard"]);
        //print(parts["text"]);

        // .5, .1, 1
    }

    // Update is called once per frame
    void Update()
    {
        if (opened) {
            open_frames += 1;
            if (open_frames <= 100) {
                this.GetComponent<Renderer>().material.color = new Color(.5f, .1f, 1.0f, (100-open_frames)/100f);
                parts["background"].transform.eulerAngles = new Vector3(0,open_frames*1.8f,open_frames*1.8f);
                parts["background"].transform.localScale = new Vector3(2,2+3*open_frames/100f,1);
                parts["billboard"].transform.eulerAngles = new Vector3(0,270f+Math.Max(0,open_frames*1.8f-90f),0);
                // TODO: make the open animation
                return;
            }
        }
        
        if (watch_open != null && watch_open.GetComponent<first_death>().did == true) {
            transform.position = orig_position;
        }
        
    }


    void OnTriggerEnter2D(Collider2D trigger) { // when trigger zones are enetered
        if (trigger.gameObject.name == "player" && opened == false) {
            opened = true;
        }
    }
}
