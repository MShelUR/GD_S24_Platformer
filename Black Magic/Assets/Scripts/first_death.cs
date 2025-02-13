using UnityEngine;

public class first_death : MonoBehaviour
{

    public bool did = false;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnTriggerEnter2D(Collider2D trigger) { // when trigger zones are enetered
        print(trigger);
        if (trigger.gameObject.name == "player") {
            did = true;
        }
    }
}
