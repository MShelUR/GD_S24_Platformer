using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 3f;
    private Rigidbody2D rb;
    int dir = 1; // right 
    int rot_cd = 0;
    bool died = false;
    int despawn_ticks = 0;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    bool CheckGround() { // true if swap direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up,1.4f); // slightly past player, change 1.1 to player height +.4
        if (hit) {
            return false;
        }
        return true;
    }

    bool CheckWall() { // true if swap direction
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.right*dir,.5f); // slightly past player, change 1.1 to player height +.4
        if (hit && hit.collider.gameObject.name != "player") {
            return true;
        }
        return false;
    }


    void Update()
    {
        if (died) {
            despawn_ticks += 1;
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.y,
                15*despawn_ticks
            );
            if (despawn_ticks > 100) {
                Destroy(gameObject);
            }
            return;
        }
        if ((CheckGround() || CheckWall()) && rot_cd <= 0) {
            dir = -dir;
            rot_cd = 5;
        }
        rot_cd -= 1;
        rb.linearVelocity = new Vector2(speed*dir, rb.linearVelocity.y);
        
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name.StartsWith("shadow_flame") || collision.gameObject.name.StartsWith("deadspike") || collision.gameObject.name == "player" && collision.transform.position.y > transform.position.y + collision.transform.localScale.y/1.2f && died == false){
            Rigidbody2D character = collision.gameObject.GetComponent<Rigidbody2D>();
            character.linearVelocity = (collision.gameObject.transform.position-transform.position)*3f + Vector3.up*2f;
            rb.linearVelocity = (collision.gameObject.transform.position-transform.position)*3f + Vector3.up*1f;
            died = true;
            Destroy(gameObject.GetComponent<BoxCollider2D>());
            //Destroy(gameObject);
        }
    }
}