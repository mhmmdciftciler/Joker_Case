using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class Dice : NetworkBehaviour
{
    [SerializeField] Animator throwAnimator;
    [SerializeField] Animator rotateAnimator;

    [ServerRpc(RequireOwnership =false)]
    public void ThrowDiceServerRpc(int value)
    {
        ThrowDiceClientRpc(value);

    }
    [ClientRpc]
    public void ThrowDiceClientRpc(int value)
    {
        throwAnimator.Play("DiceRoll " + value.ToString());
        rotateAnimator.Play("Dice " + value.ToString());
        StartCoroutine(ResetAnimators());
    }
    IEnumerator ResetAnimators()
    {
        yield return new WaitForSeconds(3);
        throwAnimator.SetTrigger("Reset");
        rotateAnimator.SetTrigger("Reset");
    }
}
