using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Temeo : SkillSet
{
    public GameObject placeholderPrefab, shroomPrefab;

    private Rigidbody rb;
    private bool pasiveActivated = false;

    protected override void SetPasive()
    {
        if (skillPlaceholder == null)
            skillPlaceholder = Instantiate(placeholderPrefab);
        skillPlaceholder.SetActive(false);
        rb = GetComponent<Rigidbody>();
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        base.UseActive(pressedLeft);
        CancelPasive();

        Vector3 midPoint = skillPlaceholder.transform.position + (transform.position - skillPlaceholder.transform.position) / 2;
        midPoint += Vector3.up * (Vector3.Distance(transform.position, skillPlaceholder.transform.position) / 2);
        Vector3 startingPoint = Vector3.MoveTowards(transform.position, skillPlaceholder.transform.position, 1);

        pView.RPC("SpawnShroom", RpcTarget.AllViaServer, startingPoint, midPoint, skillPlaceholder.transform.position);
    }

    private void Update()
    {
        if (pView == null||rb==null)
            return;
        if (!pView.IsMine)
            return;
        if (Vector3.Distance(rb.velocity, Vector3.zero) > 0.5f)
        {
            CancelPasive();
        }
        if (pasiveCooldownLeft <= 0)
        {
            if (!pasiveActivated)
            {
                pView.RPC("Invisibility", RpcTarget.All, true);
            }
            pasiveActivated = true;
        }
    }

    internal override void OnThrow()
    {
        CancelPasive();
    }
    internal override void OnHit(Transform obj)
    {
        CancelPasive();
    }

    private void CancelPasive()
    {
        if (pasiveActivated)
        {
            pView.RPC("Invisibility", RpcTarget.All, false);
        }
        pasiveCooldownLeft = pasiveCooldown;
        pasiveActivated = false;
    }

    [PunRPC]
    public void Invisibility(bool state)
    {
        transform.GetChild(0).GetComponent<MeshRenderer>().enabled = !state;
        transform.GetChild(1).gameObject.SetActive(!state);
        if (pView.IsMine)
        {
            // nicknameInstance.SetActive(!state);//VER COMO ENCONTRAR ESTO
            nicknameInstance.GetComponent<PhotonView>().RPC("ChangeActive", RpcTarget.Others, !state);
        }
    }

    [PunRPC]
    public void SpawnShroom(Vector3 startingPoint,Vector3 midPoint, Vector3 finalPoint)
    {
        GameObject instance = Instantiate(shroomPrefab, startingPoint, Quaternion.identity);
        instance.GetComponent<TemeoShroom>().midPoint = midPoint;
        instance.GetComponent<TemeoShroom>().endPoint = finalPoint;
    }
}
