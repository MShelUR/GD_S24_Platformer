using UnityEngine;

public class coin : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    void OnTriggerEnter2D(Collider2D trigger) {
        if (trigger.gameObject.name == "player"){
            Destroy(gameObject);
        }
    }
}