using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deadzone : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other) {
        if(other.gameObject.GetComponent<BaseState>() != null)
        {
            var state = other.gameObject.GetComponent<BaseState>();
            state.ApplyChange((typeof(BaseState).GetProperty("HP"), 0));
        }   
    }
}
