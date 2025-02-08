using UnityEngine;
using System.Collections.Generic; // dictionaries
using System; // Func class
using System.Collections;
using UnityEngine.Tilemaps;


public class NewMonoBehaviourScript : MonoBehaviour
{
    float jump_height = 1.5f;
    float max_speed = 10f;
    int direction = 1; // right

    string cur_move;
    int cur_frames = 0; // count frames for specials
    bool attack = false; // whether you can start a special
    bool can_move = true; // whether normal character controls work
    float cast_time = 0f; // how much time left for this combo buffer
    string cur_combo = ""; // what the current string of inputs is

    bool did_sax_bounce = false;

    string valid_inputs = "wasd"; // inputs in combo (i.e. wad in wadq)
    string valid_activators = "qzxc"; // activators for combo (i.e. q in wadq)

    void sax() {
        if (cur_frames <= 1) {
            did_sax_bounce = false;
        }
        print("sax!");
        if (cur_frames <= 20) {
            Vector2 new_vel = new Vector2(direction*20,5);
            if (did_sax_bounce) {
                new_vel = new Vector2(direction*5,15); // more vertical if bounce
            }
            character.linearVelocity = new_vel;
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                transform.eulerAngles.x,
                36*cur_frames
            );
            return;
        }

        cur_move = null;
        can_move = true;
        attack = false;
    }

    void ddsc() {
        print("ddsc!");
    }

    Tilemap destructable;
    Rigidbody2D character;
    Dictionary<string, System.Action> move_map; // init movelist here so update sees it
    Camera cam;
    public GameObject spawn_point; // object with reference point for spawn
    public GameObject destructable_tileset; // object housing destructable tilemap

    GridLayout grid;

    bool will_die = false; // ominous
    public bool died = false;

    bool respawning = false;

    public Vector2 CheckGround() // gives hit point below or zero
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, -Vector2.up,1.4f); // slightly past player, change 1.1 to player height +.4
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
        character = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        spawn_point = this.spawn_point;
        // set up movelist
        move_map = new Dictionary<string, System.Action>();
        // add all moves in reverse length order so longer combos are checked first
        move_map.Add("ddsc", ddsc);
        move_map.Add("sax", sax);
        Application.targetFrameRate = 60;
    }

    // Update is called once per frame
    void Update()
    {
        cur_frames += 1;
        // handle camera

        Vector3 charpos = character.transform.position;
        Vector3 newpos = cam.transform.position;

        newpos.x = Math.Clamp(newpos.x,charpos.x-8,charpos.x+8);
        //newpos.x = Math.Max(Math.Min(newpos.x,charpos.x+50),charpos.x-50); // move cam when player gets near bounds
        newpos.y = charpos.y + Math.Min(0,character.linearVelocity.y*.02f+.1f);

        //newpos -= Vector3.forward*100; // move it back so no near clip

        cam.transform.position = newpos;
        
        if (cur_move != null) {
            print(cur_move);
            move_map[cur_move](); // do move again with new frame
            return;
        }

        // handle combos

        if (respawning) { // actively moving towards spawn
            Vector2 move_dir = (spawn_point.transform.position-transform.position);
            if (move_dir.magnitude < .2) {
                transform.position = spawn_point.transform.position;
                character.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);
                died = false;
                respawning = false;
                Time.timeScale = 1;
            }
            move_dir.Normalize();
            transform.position += new Vector3(move_dir.x, move_dir.y,0)*(.1f+(float)Math.Sqrt(move_dir.magnitude)*.2f);
            return;
        }
        if (died) { // do respawn stuff instead of movement
            respawning = true;
            Time.timeScale = 0;
            character.GetComponent<Renderer>().material.color = new Color(1.0f, 1.0f, 1.0f, 0.5f);
            return;
        }
        foreach (char c in Input.inputString) { // inputString is deprecated, typecast keycode eventually
            if (valid_inputs.IndexOf(c) != -1) { // woah it's a valid input!
                if (cast_time <= 0f) { // new combo!
                    cast_time = 0.5f; // start .5 second window for casting
                }
                cur_combo += c.ToString();
                //print(cur_combo);
            } else if (valid_activators.IndexOf(c) != -1) { // player ended combo
                cur_combo += c.ToString();
                foreach (string move in move_map.Keys) { // for every move combo
                    if (cur_combo.Contains(move) && attack == false) { // if the combo = cur combo
                        attack = true;
                        can_move = false;
                        cur_frames = 0;
                        cur_move = move;
                        move_map[move](); // do that move
                        break;
                    }
                }
                cur_combo = "";
                cast_time = 0f;
            }
        }
        if (cast_time >= 0 && cur_combo != "") { // mid combo
            cast_time -= Time.deltaTime; // reduce time
            if (cast_time <= 0) { // ran out of time
                //print("combo reset");
                cur_combo = ""; // reset combo
            }
        }


        // handle base movement

        if (can_move == false) { // movement override, ignore movement
            return;
        }

        var velocity = character.linearVelocity;
        var normal = Vector2.zero;
        var right = Vector2.right;
        var floor = CheckGround();

        var mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        if (mousePos.x < transform.position.x) {
            direction = -1;
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                180,
                transform.eulerAngles.z
            );
        } else {
            direction = 1;
            transform.eulerAngles = new Vector3(
                transform.eulerAngles.x,
                0,
                transform.eulerAngles.z
            );
        }

        if (floor != Vector2.zero) {
            normal = floor;
            normal.Normalize();
            normal *= .3f;
            right = Vector2.Perpendicular(floor - new Vector2(transform.position.x,transform.position.y));
        }


        if (Input.GetKey(KeyCode.D)) { // moving right
            velocity += right;
        }
        if (Input.GetKey(KeyCode.A)) {
            velocity -= right;
        }
        velocity.x = Math.Clamp(velocity.x*.98f,-max_speed,max_speed);
        if (floor != Vector2.zero && Input.GetKey(KeyCode.Space)) {
            //character.linearVelocity += Vector2.up * 15;
            velocity.y = 10*jump_height;
            //character.AddForce(new Vector2(0, 15*jump_height), ForceMode2D.Impulse);
        } /* else if (floor != Vector2.zero) {
            velocity += normal; // staying force Unity normals are not normals, this doesn't work :(
        }*/
        if (floor == Vector2.zero && Input.GetKey(KeyCode.S)) { // fast fall, doesn't work well w/ combos
            //velocity.y = Math.Min(velocity.y,-10);
            //character.AddForce(new Vector2(0, -15), ForceMode2D.Impulse);
        }

        if (velocity.y < -25) {
            will_die = true;
        } else if (velocity.y > -1 && will_die) {
            will_die = false;
            died = true;
        }

        // set velocity
        character.linearVelocity = velocity;


        //character.linearVelocity += Vector2.right * right_dir * speed * Time.deltaTime;
    }

    Vector3Int tilepos;

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.name == "hazards") {
            died = true;
        }
        if (cur_move == "sax" && did_sax_bounce == false && collision.gameObject.name != "destructable") {
            did_sax_bounce = true;
            direction = -direction;
        }
        if (cur_move == "sax" && collision.gameObject.name == "destructable") {
            foreach (ContactPoint2D contact in collision.contacts) {
                Vector2 hit = contact.point;
                tilepos =  grid.LocalToCell(hit);
                destructable.SetTile(tilepos,null);
            }
        }
    }
}