using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Endzone : MonoBehaviour
{

    public AudioClip scoreSound;


    #region touchdownStuff
    void OnTriggerStay2D(Collider2D other)
    {
        ReceiverController receiver = other.GetComponent<ReceiverController>();
        if (receiver != null)
        {
            if (receiver.curState == ReceiverController.State.SCORING)
            {
                //tells receiver to stop running
                receiver.curState = ReceiverController.State.HAS_SCORED;
                GameManagerScript.instance.PlaySound(scoreSound);
                GetComponent<ParticleSystem>().Play();
                other.GetComponent<CircleCollider2D>().enabled = false;

                GameplayManager.instance.Touchdown();

            }
        }
    }


    #endregion
}
