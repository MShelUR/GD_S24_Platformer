using UnityEngine;

public class Billboard : MonoBehaviour
{
    
    bool opened = false;
    int open_frames = 0;

    //GameObject background = transform.GetChild(0).GameObject;
    //GameObject billboard = transform.GetChild(1).GameObject;
    //GameObject text = billboard.transform.GetChild(0).GameObject;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (opened) {
            open_frames += 1;
            if (open_frames < 80) {
                // TODO: make the open animation
                return;
            }
        }
    }


    void OnTriggerEnter2D(Collider2D trigger) { // when trigger zones are enetered
        if (trigger.gameObject.name == "player" && opened == false) {
            opened = true;
        }
    }
}
