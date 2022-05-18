using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Toph : SkillSet
{
    public GameObject AoEPrefab, placeholderPrefab, pasivePrefab;
    public float timeBtwnSpikes, distanceBtwnSpikes, timeForPassive, minSpeedForPasive;
    public int amountOfSpikes;

    protected override void SetPasive()
    {
        if (skillPlaceholder == null)
            skillPlaceholder = Instantiate(placeholderPrefab);
        skillPlaceholder.SetActive(false);
    }

    private void Update()
    {
        if (pView == null)
            pView = GetComponent<PhotonView>();
        if (!pView.IsMine)
            return;
        if (pasiveCooldownLeft<=0)
        {
            if (Vector3.Distance(GetComponent<Rigidbody>().velocity, Vector3.zero) > minSpeedForPasive)
            {
                pView.RPC("SpawnPasive", RpcTarget.AllViaServer, transform.position-GetComponent<Rigidbody>().velocity.normalized*2, transform.position);
                pasiveCooldownLeft = pasiveCooldown;
            }
        }
    }

    internal override void UseActive(bool pressedLeft)
    {
        if (cooldownLeft > 0)
            return;
        cooldownLeft = cooldown;
        StartCoroutine(LoadAttacks(pressedLeft));
       
    }

    private IEnumerator LoadAttacks(bool pressedLeft)
    {
        Vector3 initialPos = skillPlaceholder.transform.position;
        Vector3 whatIsForward = Camera.main.transform.forward;
        whatIsForward = new Vector3(whatIsForward.x, 0, whatIsForward.z);
        whatIsForward.Normalize();
        
        Quaternion initialRot = skillPlaceholder.transform.rotation;
        
        for (int i = 0; i < amountOfSpikes; i++)
        {
            pView.RPC("SpawnAttack", RpcTarget.AllViaServer, initialPos+ whatIsForward*i,initialRot,!pressedLeft);
            
            yield return new WaitForSeconds(timeBtwnSpikes);
        }
    }

    [PunRPC]
    public void SpawnAttack(Vector3 pos, Quaternion rotation, bool collision)
    {
        GameObject instance=Instantiate(AoEPrefab, pos, rotation);
        instance.GetComponent<Collider>().isTrigger = collision;
        instance.GetComponent<Spike>().inmune = transform;
        if (activeSFX != -1)
            GameControl.main.GetComponent<SoundReproducer>().ReproduceSound(activeSFX, 1f);
    }

    [PunRPC]
    public void SpawnPasive(Vector3 pos, Vector3 castPos)
    {
        GameObject instance = Instantiate(pasivePrefab, pos, Quaternion.identity);
        instance.transform.LookAt(castPos);
        if (activeSFX != -1)
            GameControl.main.GetComponent<SoundReproducer>().ReproduceSound(activeSFX, 1f);
    }
}
