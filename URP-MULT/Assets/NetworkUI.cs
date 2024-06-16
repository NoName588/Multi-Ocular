using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class NetworkUI : NetworkBehaviour
{
    [SerializeField] private Button hostbutton;
    [SerializeField] private TextMeshProUGUI userCount;
    [SerializeField] private Button clientbutton;

    public GameObject fakecam;
    private NetworkVariable<int> UserNum = new NetworkVariable<int>(0, NetworkVariableReadPermission.Everyone);

    private void Awake()
    {
        hostbutton.onClick.AddListener(() =>
        {

            NetworkManager.Singleton.StartHost();
            fakecam.SetActive(false);

        });
        clientbutton.onClick.AddListener(() =>
        {

            NetworkManager.Singleton.StartHost();
            fakecam.SetActive(false);

        });
    }

    private void Update()
    {
        userCount.text = "Usuarios: " + UserNum.Value.ToString();
        if (!IsServer) return;
        UserNum.Value = NetworkManager.Singleton.ConnectedClients.Count;
        
    }
}
