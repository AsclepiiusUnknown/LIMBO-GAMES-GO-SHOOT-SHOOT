using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class LobbyPlayerDisplay : MonoBehaviour
{
    public bool Filled { get { return button.interactable; } }
    public bool Ready { get; private set; } = false;

    public TextMeshProUGUI playerName;
    public Button button;
    public Image readyIndicator;
    public Color readyColor = Color.green;
    public Color unreadyColor = Color.red;

    private NetworkPlayer player;
    private int index;

    private void Start()
    {
        button.interactable = false;
    }

    private void Update()
    {
        playerName.text = (player != null && !string.IsNullOrEmpty(player.username)) ? player.username : "Player";
        readyIndicator.color = Ready ? readyColor : unreadyColor;
    }

    public void AssignPlayer(NetworkPlayer _player, int _index)
    {
        player = _player;
        index = _index;
        button.interactable = true;
        readyIndicator.color = unreadyColor;
    }

    public void ToggleReadyState()
    {
        SetReadyState(!Ready);
        player.ReadyPlayer(index, Ready);
    }

    public void SetReadyState(bool _isReady) => Ready = _isReady;
}
