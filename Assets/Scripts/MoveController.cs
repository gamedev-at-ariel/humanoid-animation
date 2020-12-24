using UnityEngine;

public class MoveController : MonoBehaviour
{
    Animator anim;

    void Start()
    {
        anim = GetComponent<Animator>();
    }

    void Update()
    {
        anim.SetBool("Jump", Input.GetKey(KeyCode.Space));
        anim.SetBool("Walk", Input.GetKey(KeyCode.UpArrow));
        anim.SetBool("Left", Input.GetKey(KeyCode.LeftArrow));
        anim.SetBool("Right", Input.GetKey(KeyCode.RightArrow));
        anim.SetBool("Pick-Put", Input.GetKey(KeyCode.DownArrow));
    }
}