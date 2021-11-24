using Mirror;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MonsterCreater : NetworkBehaviour
{
    public Vector3 Pos;
    public GameObject projectilePrefab;
    private NetworkIdentity identity;
    // Start is called before the first frame update

    public override void OnStartServer()
    {
        var networkManager = GameObject.FindObjectOfType<NetworkManager>();
        identity = this.gameObject.GetComponent<NetworkIdentity>();
        CreaterMonster();
    }
    public void CreaterMonster()
    {
        GameObject projectile = Instantiate(projectilePrefab, Pos, transform.rotation);
        NetworkServer.Spawn(projectile);
    }
}
