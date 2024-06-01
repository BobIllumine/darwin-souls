using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Deadzone : MonoBehaviour
{
    private void OnTriggerExit2D(Collider2D other) {
        if(other.gameObject.GetComponent<BaseState>() != null)
        {
            var state = other.gameObject.GetComponent<BaseState>();
            Stats newStats = new Stats(state.stats);
            newStats.HP = 0;
            state.ApplyChanges(newStats);
            return;
        }
        if(other.gameObject.CompareTag("Projectile")) {
            Destroy(other.gameObject);
            return;
        }   
    }
}
