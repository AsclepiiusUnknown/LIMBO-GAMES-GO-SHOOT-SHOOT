using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Mirror;

public class LobbyMenu : MonoBehaviour
{
    public LobbyPlayerDisplay[] playerDisplays;
    public Button startButton;
    public Button readyButton;
    public TMP_InputField playerNameInput;
    public TextMeshProUGUI codeText;

    private GameNetworkManager network;
    private NetworkPlayer localPlayer;

    private void Start()
    {
        network = GameNetworkManager.singleton as GameNetworkManager;
        playerNameInput.onEndEdit.AddListener(OnEndEditName);
        startButton.interactable = false;

        codeText.text = NetworkManager.singleton.networkAddress;
    }

    private void Update()
    {
        if (network.IsHost)
        {
            foreach (LobbyPlayerDisplay display in playerDisplays)
            {
                if (!display.Ready && display.Filled)
                {
                    if (startButton.interactable)
                    {
                        startButton.interactable = false;
                    }
                    return;
                }
            }

            if (!startButton.interactable)
                startButton.interactable = true;
        }
    }

    public void OnPlayerConnect(NetworkPlayer _player)
    {
        for (int i = 0; i < playerDisplays.Length; i++)
        {
            LobbyPlayerDisplay dipslay = playerDisplays[i];
            if (!dipslay.Filled)
            {
                dipslay.AssignPlayer(_player, i);
                if (_player.isLocalPlayer)
                {
                    localPlayer = _player;
                    readyButton.onClick.AddListener(dipslay.ToggleReadyState);
                }
                break;
            }
        }
    }

    public void OnClickStart() => localPlayer.StartGame();

    public void SetReadyPlayer(int _index, bool _isReady) => playerDisplays[_index].SetReadyState(_isReady);

    void OnEndEditName(string _name)
    {
        if (localPlayer != null)
        {
            localPlayer.SetName(_name);
        }
    }
}
