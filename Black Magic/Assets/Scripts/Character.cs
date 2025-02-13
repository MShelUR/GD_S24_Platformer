using UnityEngine;
using System.Collections.Generic; // dictionaries
using System; // Func class
using System.Collections;
using UnityEngine.Tilemaps;
using UnityEngine.SceneManagement;


public class Character : MonoBehaviour
{
    bool debug_mode = false; // DEBUG, unlock everything!

    public GameObject spawn_point; // object with reference point for spawn
    public GameObject destructable_tileset; // object housing destructable tilemap
    public GameObject ground_destructable;
    public GameObject shadow_flame_object; // flame for shadow flame
    public GameObject deadspike_object;

    bool ended = false;

    Color background_color = new Color(.7f,.9f,.8f,1f);
    Scene scene;

    Dictionary<float, Color> height_color_map;
    
    float jump_height = 1.5f;
    float max_speed = 10f;
    int direction = 1; // right

    System.Random rnd = new System.Random();

    string cur_move;
    int cur_frames = 0; // count frames for specials
    bool attack = false; // whether you can start a special
    bool can_move = true; // whether normal character controls work
    float cast_time = 0f; // how much time left for this combo buffer
    string cur_combo = ""; // what the current string of inputs is

    bool sax_unlock = false;
    bool sax_cd = false;
    bool did_sax_bounce = false;

    bool sdq_unlock = false;
    bool shadow_flaming = false;
    int shadow_flame_dist = 0;
    Vector3 shadow_flame_dir;
    Vector3 shadow_flame_pos;

    bool asdq_unlock = false;
    bool ddsc_unlock = false;
    bool aadq_unlock = false;
    bool wwq_unlock = false;

    string valid_inputs = "wasd"; // inputs in combo (i.e. wad in wadq)
    string valid_activators = "qzxc"; // activators for combo (i.e. q in wadq)

    void sax() {
        if (sax_unlock == false) {
            cur_move = null;
            can_move = true;
            attack = false;
            return;
        }
        cur_frames += 1; // tick up framerate every call
        if (cur_frames == 1) { // first frame
            did_sax_bounce = false;
            if (sax_cd) {
                cur_move = null;
                can_move = true;
                attack = false;
                cur_move = null; // uhoh, tried doing it on cd
                return;
            }
            sax_cd = true;
        }
        if (cur_frames <= 20) {
            Vector2 new_vel = new Vector2(direction*20,5);
            if (did_sax_bounce) {
                new_vel = new Vector2(direction*5,15); // more vertical if bounce
            }
            if (will_die == false) {
                character.linearVelocity = new_vel;
            }
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

    void sdq_loop() {
        shadow_flame_pos += shadow_flame_dir; // move SF
        GameObject sf = (GameObject)Instantiate(shadow_flame_object, shadow_flame_pos, Quaternion.identity); // spawn SF
        sf.GetComponent<shadow_flame>().dir = shadow_flame_dir.x;
        sf.GetComponent<shadow_flame>().wait = shadow_flame_dist*2;
        sf.GetComponent<shadow_flame>().destructable_tileset = destructable_tileset;
        shadow_flame_dist += 1; // add to distance tracker
        if (shadow_flame_dist > 7) { // past max distance, stop
            shadow_flaming = false;
        }
    }

    void sdq() {
        print("sdq!");
        if (shadow_flaming == false && sdq_unlock == true) {
            shadow_flame_dist = 0;
            shadow_flame_pos = transform.position + new Vector3(.5f+1*direction,-1f,0);
            shadow_flame_dir = new Vector3(1.5f*direction,0,0);
            shadow_flaming = true;
        }
        cur_move = null;
        can_move = true;
        attack = false;
    }

    void asdq() {
        if (asdq_unlock == false) {
            sdq();
            return;
        }
        cur_frames += 1;
        if (cur_frames == 1) {
            character.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation;
            GameObject asdq = (GameObject)Instantiate(deadspike_object, transform.position, Quaternion.identity); // spawn deadspike
            asdq.GetComponent<deadspike_script>().destructable_tileset = ground_destructable;
        }
        if (cur_frames <= 20) {
            return;
        }
        
        cur_move = null;
        can_move = true;
        attack = false;
        character.constraints = RigidbodyConstraints2D.FreezeRotation;
    }

    void ddsc() {
        print("ddsc!");
    }

    void aadq() {
        print("aadq!");
    }

    void wwq() {
        print("wwq!");
    }

    Tilemap destructable;
    Rigidbody2D character;
    Dictionary<string, System.Action> move_map; // init movelist here so update sees it
    Camera cam;

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
        scene = SceneManager.GetActiveScene();
        destructable = destructable_tileset.GetComponent<Tilemap>();
        grid = destructable_tileset.GetComponent<GridLayout>();
        character = GetComponent<Rigidbody2D>();
        cam = Camera.main;
        //spawn_point = this.spawn_point;
        // set up movelist
        move_map = new Dictionary<string, System.Action>();
        // add all moves in reverse length order so longer combos are checked first
        Application.targetFrameRate = 60;
        
        transform.position = spawn_point.transform.position;

        height_color_map = new Dictionary<float, Color>(); // y map for colors
        // add in descending order
        height_color_map.Add(1000f,background_color); // above ground
        height_color_map.Add(-20f,new Color(.9f,.6f,.9f,1f)); // placeholder
        height_color_map.Add(-30f,new Color(.7f,.7f,.7f,1f)); // deep caves
        height_color_map.Add(-50f,new Color(.7f,.5f,.5f,1f)); // deeper caves
        height_color_map.Add(-65f,new Color(.8f,.5f,.5f,1f)); // heck
        height_color_map.Add(-80f,new Color(.8f,.3f,.7f,1f)); // hecker
        height_color_map.Add(-90f,new Color(.6f,.3f,.6f,1f)); // heckest
        height_color_map.Add(-100f,new Color(.4f,.7f,.9f,1f)); // necro


        move_map.Add("asdq", asdq);
        move_map.Add("sax", sax);
        move_map.Add("sdq", sdq);
        
        if (debug_mode) { // debug, unlock all moves
            sax_unlock = true;
            sdq_unlock = true;
            asdq_unlock = true;
            //ddsc_unlock = true;
            //aadq_unlock = true;
            //wwq_unlock = true;
            //move_map.Add("ddsc", ddsc);
            //move_map.Add("aadq", aadq);
            //move_map.Add("wwq", wwq);
        }
    }

    Vector3 death_shake = new Vector3(0,0);


    // Update is called once per frame
    void Update()
    {
        // handle camera

        Vector3 charpos = character.transform.position;
        Vector3 newpos = cam.transform.position-death_shake;

        if (ended) {
            var mousePos2 = Camera.main.ScreenToWorldPoint(Input.mousePosition);

            if (mousePos2.x < transform.position.x) {
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
            return;
        }

        newpos.x = Math.Clamp(newpos.x,charpos.x-8,charpos.x+8);
        //newpos.x = Math.Max(Math.Min(newpos.x,charpos.x+50),charpos.x-50); // move cam when player gets near bounds
        newpos.y = charpos.y + Math.Min(0,character.linearVelocity.y*.02f+.1f);

        if (will_die && died == false) { // if will die, shake screen
            death_shake.x += rnd.Next(-5,6)*.03f;
            death_shake.y += rnd.Next(-5,6)*.03f;
        } else {

        }

        Color goal = cam.backgroundColor;
        foreach(KeyValuePair<float, Color> height in height_color_map) {
            if (transform.position.y < height.Key) {
                goal = height.Value;
            }
        }

        Color lerpedColor = Color.Lerp(cam.backgroundColor, goal, .01f);
        cam.backgroundColor = lerpedColor;

        //newpos -= Vector3.forward*100; // move it back so no near clip

        cam.transform.position = newpos+death_shake;
        
        if (shadow_flaming) {
            sdq_loop();
        }

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
            character.GetComponent<Renderer>().material.color = new Color(1.0f, .5f, .5f, 0.5f);
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
            sax_cd = false; // on ground, reset cd
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
                cur_frames -= 3; // extend sax
                
            }
        }
        if (collision.gameObject.transform.parent.gameObject.name == "enemies") {
            print("enemy!!");
            if(collision.transform.position.y + collision.transform.localScale.y/1.2f > transform.position.y){
                died = true;
            }
        }
    }

    void OnTriggerEnter2D(Collider2D trigger) { // when trigger zones are enetered
        string t_name = trigger.gameObject.name;
        print(trigger.gameObject.name);
        if (t_name == "sax_unlock" && sax_unlock == false) { // unlock sax!
            sax_unlock = true;
            //move_map.Add("sax", sax);
            //trigger.gameObject.GetComponent<Script>;
        } else if (t_name == "sdq_unlock" && sdq_unlock == false) { // unlock sdq!
            sdq_unlock = true;
            //move_map.Add("sdq", sdq);
        } else if (t_name == "asdq_unlock" && asdq_unlock == false) { // unlock asdq!
            asdq_unlock = true;
            //move_map.Add("asdq", asdq);
        } else if (t_name == "end_credits") { // end
            ended = true;
            transform.position = trigger.gameObject.transform.position;
            character.constraints = RigidbodyConstraints2D.FreezePositionX | RigidbodyConstraints2D.FreezeRotation | RigidbodyConstraints2D.FreezePositionY;
        } else if (t_name == "dieded") {
            trigger.gameObject.GetComponent<first_death>().did = true;
        }
    }
}