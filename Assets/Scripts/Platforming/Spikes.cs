using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Spikes : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D _other)
    {
        if (_other.CompareTag("Player"))
        {
            StartCoroutine(RespawnPoint());
        }
    }

    IEnumerator RespawnPoint()
    {
        PlayerController.Instance.pState.cutscene = true;
        PlayerController.Instance.pState.invincible = true;
        PlayerController.Instance.rb.linearVelocity = Vector2.zero;
        Time.timeScale = 0;
        yield return new WaitForSecondsRealtime(1);
        PlayerController.Instance.transform.position = GameManager.Instance.platformingRespawnPoint;
        PlayerController.Instance.pState.cutscene = false;
        PlayerController.Instance.pState.invincible = false;
        Time.timeScale = 1;
    }
}
