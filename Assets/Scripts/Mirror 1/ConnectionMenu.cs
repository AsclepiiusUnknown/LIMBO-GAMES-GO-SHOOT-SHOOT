using UnityEngine;
using UnityEngine.UI;
using Mirror;
using TMPro;

public class ConnectionMenu : MonoBehaviour
{
    public TMP_InputField ipInput;
    public CodeManager codeManager;

    public void OnClickHost()
    {
        // NetworkManager.singleton.GetComponent<kcp2k.KcpTransport>().Port = codeManager.GenerateCode();
        // print(NetworkManager.singleton.networkAddress);
        NetworkManager.singleton.StartHost();
    }

    public void OnClickConnect()
    {
        NetworkManager.singleton.networkAddress = string.IsNullOrEmpty(ipInput.text) ? "localhost" : ipInput.text;
        NetworkManager.singleton.StartClient();
    }
}
