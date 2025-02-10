using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;
    bool dir = false; 

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        if(dir){
            rb.linearVelocity = new Vector2(speed, rb.linearVelocity.y);
        }else{
            rb.linearVelocity = new Vector2((speed * -1), rb.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name == "destructable") {
            dir = !dir;
        }
        else if (collision.gameObject.name == "player"){
            if(collision.transform.position.y > transform.position.y + 1){
                Destroy(gameObject);
            }
        }
    }
}