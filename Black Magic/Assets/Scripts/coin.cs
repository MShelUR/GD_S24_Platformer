using UnityEngine;

public class coin : MonoBehaviour
{
    void Start()
    {

    }

    void Update()
    {

    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name == "player"){
            Destroy(gameObject);
        }
    }
}