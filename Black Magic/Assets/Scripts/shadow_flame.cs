using UnityEngine;
using UnityEngine.Tilemaps;

public class shadow_flame : MonoBehaviour
{

    public GameObject destructable_tileset; // object housing destructable tilemap
    public float dir = 1;
    public int wait = 0;

    float ticks = 0;
    
    Tilemap destructable;
    GridLayout grid;

    public Vector2 CheckGround() // gives hit point below or zero
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up,.4f); // slightly past player, change 1.1 to player height +.4
        if (hit) {
            return hit.point;
        }
        return Vector2.zero;
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        destructable = destructable_tileset.GetComponent<Tilemap>();
        grid = destructable_tileset.GetComponent<GridLayout>();
        transform.eulerAngles = new Vector3(0,0,15*dir);
    }

    // Update is called once per frame
    void Update()
    {
        if (CheckGround() == Vector2.zero) {
            //Destroy(gameObject);
        }
        if (wait > 0) {
            wait -= 1;
            return;
        }
        ticks += 4;
        transform.localScale = new Vector3(1.7f-ticks/50,ticks/6,1);
        foreach (Transform childTransform in this.GetComponentsInChildren<Transform>())
        {
            GameObject child = childTransform.gameObject;
            if (child == gameObject) {
                child.GetComponent<Renderer>().material.color = new Color(.1f, .1f, .1f, 1f-ticks/50f);
            } else {
                child.GetComponent<Renderer>().material.color = new Color(.5f, .2f, .8f, 1f-ticks/50f);
            }
        }
        if (ticks > 50) {
            Destroy(gameObject);
        }
    }

    Vector3Int tilepos;

    void OnCollisionEnter2D(Collision2D collision) {
        print(collision.gameObject.name);
        if (collision.gameObject.name == "destructable") {
            foreach (ContactPoint2D contact in collision.contacts) {
                Vector2 hit = contact.point;
                tilepos =  grid.LocalToCell(hit);
                destructable.SetTile(tilepos,null);
            }
        } 
    }

}
