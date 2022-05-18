using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Planty : SkillSet
{
    public GameObject AoEPrefab, placeholderPrefab;

    protected override void SetPasive()
    {
        if (skillPlaceholder == null)
            skillPlaceholder = Instantiate(placeholderPrefab);
        skillPlaceholder.SetActive(false);
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);
        pView.RPC("SpawnAttack", RpcTarget.AllViaServer, skillPlaceholder.transform.position);
    }

    [PunRPC]
    public void SpawnAttack(Vector3 pos)
    {
        Instantiate(AoEPrefab, pos, Quaternion.identity);
    }
}
