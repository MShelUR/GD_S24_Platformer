using UnityEngine;
using System.Collections.Generic; // dictionaries
using System; // Func class


public class NewMonoBehaviourScript : MonoBehaviour
{
    float speed = 50f;
    float jump_height = 1.5f;
    float max_speed = 10f;

    float cast_time = 0f; // how much time left for this combo buffer
    string cur_combo = ""; // what the current string of inputs is

    string valid_inputs = "wasd"; // inputs in combo (i.e. wad in wadq)
    string valid_activators = "qzxc"; // activators for combo (i.e. q in wadq)

    void sax() {
        print("sax!");
    }

    void ddsc() {
        print("ddsc!");
    }

    Rigidbody2D character;
    Dictionary<string, System.Action> move_map; // init movelist here so update sees it

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        character = GetComponent<Rigidbody2D>();
        // set up movelist
        move_map = new Dictionary<string, System.Action>();
        // add all moves in reverse length order so longer combos are checked first
        move_map.Add("ddsc", ddsc);
        move_map.Add("sax", sax);
    }

    // Update is called once per frame
    void Update()
    {
        foreach (char c in Input.inputString) { // inputString is deprecated, saves KeyCode typecasting
            if (valid_inputs.IndexOf(c) != -1) { // woah it's a valid input!
                if (cast_time <= 0f) { // new combo!
                    cast_time = 0.5f; // start .5 second window for casting
                }
                cur_combo += c.ToString();
                print(cur_combo);
            } else if (valid_activators.IndexOf(c) != -1) { // player ended combo
                cur_combo += c.ToString();
                foreach (string move in move_map.Keys) { // for every move combo
                    if (cur_combo.Contains(move)) { // if the combo = cur combo
                        move_map[move](); // do that move
                        break;
                    }
                }
                cur_combo = "";
                cast_time = 0f;
            }
        }
        if (cast_time >= 0 && cur_combo != "") {
            cast_time -= Time.deltaTime;
            if (cast_time <= 0) {
                print("dieded");
                cur_combo = "";
            }
        }
        var velocity = character.linearVelocity;
        if (Input.GetKey(KeyCode.D)) { // moving right
            velocity.x += 1;
        }
        if (Input.GetKey(KeyCode.A)) {
            velocity.x -= 1;
        }
        velocity.x = Math.Clamp(velocity.x*.98f,-max_speed,max_speed);
        if (character.linearVelocity.y == 0 && Input.GetKey(KeyCode.Space)) {
            //character.linearVelocity += Vector2.up * 15;
            velocity.y = 10*jump_height;
            //character.AddForce(new Vector2(0, 15*jump_height), ForceMode2D.Impulse);
        }
        if (Input.GetKey(KeyCode.S) && character.linearVelocity.y != 0) {
            velocity.y = Math.Min(velocity.y,0);
            //character.AddForce(new Vector2(0, -15), ForceMode2D.Impulse);
        }
        character.linearVelocity = velocity;
        //character.linearVelocity += Vector2.right * right_dir * speed * Time.deltaTime;
    }
}