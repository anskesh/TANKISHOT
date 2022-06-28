using System;
using System.Linq;
using Photon.Pun;
using TMPro;
using UnityEngine;

public class PlayersTop : MonoBehaviour
{
    public static PlayersTop Instance;

    private TextMeshProUGUI[] _playersText = new TextMeshProUGUI[3];

    private void Awake()
    {
        _playersText = GetComponentsInChildren<TextMeshProUGUI>().ToArray();
        if (Instance == null)
            Instance = this;
        
        ClearTop();
    }

    private void Start()
    {
        RenderTop();
    }

    public void RenderTop()
    {
        ClearTop();
        
        var players = PlayersView.Instance.GetPlayers();
        if (players.Length == 0) return;
        
        var topPlayers = PlayersView.Instance.GetPlayers()
            .OrderByDescending(p => p.Score)
            .Take(3).ToArray();
        
        for (int i = 0; i < topPlayers.Length; i++)
        {
            _playersText[i].text = $"{i + 1}. {topPlayers[i].GetComponent<PhotonView>().Owner.NickName}     {topPlayers[i].Score}";
        }
    }

    private void ClearTop()
    {
        for (int i = 0; i < _playersText.Length; i++)
        {
            _playersText[i].text = "";
        }
    }
}
