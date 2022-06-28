using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    private TextMeshProUGUI _label;
    private TextMeshProUGUI _playerCount;

    private void Awake()
    {
        var texts = GetComponentsInChildren<TextMeshProUGUI>().ToArray();
        _label = texts[0];
        _playerCount = texts[1];
    }

    private void OnEnable()
    {
        GetComponent<Button>().onClick.AddListener(() => PhotonNetwork.JoinRoom(_label.text));
    }

    private void OnDisable()
    {
        GetComponent<Button>().onClick.RemoveAllListeners();
    }

    public void Render(Photon.Realtime.RoomInfo room)
    {
        _label.text = room.Name;
        _playerCount.text = $"Игроков: {room.PlayerCount}";
    }
}
