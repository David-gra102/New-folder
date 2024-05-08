using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using UnityEditor;
using UnityEngine.EventSystems;
using System;
using System.Collections.Generic;

public class PAController : NetworkBehaviour
{
    [SerializeField] private GameObject camHolder;
    public static event EventHandler OnAnyPlayerSpawned;

    public static void ResetStaticData()
    {
        OnAnyPlayerSpawned = null;
    }

    public static PAController LocalInstance {get; private set;}

    [SerializeField] private List<Vector2> spawnPositionList;



    void Start()
    {
        PlayerData playerData = GameMultiplayer.Instance.GetPlayerDataFromClientId(OwnerClientId);
    }

    public override void OnNetworkSpawn()
    {
        if(IsOwner)
        {
            LocalInstance = this;
        }
        if(!IsOwner)
        {
            this.camHolder.SetActive(false);
        }

        transform.position = spawnPositionList[GameMultiplayer.Instance.GetPlayerDataIndexFromClientId(OwnerClientId)];

        OnAnyPlayerSpawned?.Invoke(this, EventArgs.Empty);

        if(IsServer)
        {
            NetworkManager.Singleton.OnClientDisconnectCallback += NetworkManager_OnClientDisconnectCallback;
        }
        base.OnNetworkSpawn();
    }

    private void NetworkManager_OnClientDisconnectCallback(ulong obj)
    {
        return; //do nothing?
    }

    


}
