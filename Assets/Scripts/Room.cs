using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using TMPro;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Room : MonoBehaviourPunCallbacks
{
    [SerializeField] private TextMeshProUGUI _log;
    
    [SerializeField] private GameObject[] _mapsTemlate;
    [SerializeField] private PlayerControl[] _playersTemplate;
    
    [SerializeField] private Camera _cameraTemplate;
    [SerializeField] private GameObject _canvasTemplate;
    
    [SerializeField] private float _radius;
    [SerializeField] private float _respawnCooldown;
    [SerializeField] private GameObject _deathScreen;
    [SerializeField] private TextMeshProUGUI _score;

    private void Start()
    {
        InvokeRepeating(nameof(ClearLog), 1, 10);
        if (PhotonNetwork.IsMasterClient)
            PhotonNetwork.InstantiateRoomObject(_mapsTemlate[Random.Range(0, _mapsTemlate.Length)].name, Vector3.zero, Quaternion.identity);
        
        PhotonNetwork.AutomaticallySyncScene = true;
        SpawnPlayer();
    }

    public void LeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        Log("Вы покинули комнату.");
        SceneManager.LoadScene(0);
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        Log($"Игрок {newPlayer.NickName} зашёл в комнату.");
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        Log($"Игрок {otherPlayer.NickName} покинул комнату.");
        PlayersTop.Instance.RenderTop();
    }

    private void SpawnPlayer()
    {
        var camera = Instantiate(_cameraTemplate, new Vector3(0,0, -10), Quaternion.identity);
        
        for (int i = 0; i < 20; i++)
        {
            var position = Random.insideUnitCircle * _radius;
            Collider2D[] hitColliders = new Collider2D[1];
            var numsCollider = Physics2D.OverlapCircleNonAlloc(position, 0.7f, hitColliders);
            
            if (numsCollider == 0)
            {
                var player = PhotonNetwork.Instantiate(_playersTemplate[Random.Range(0, _playersTemplate.Length)].name, position, Quaternion.identity);
                var playerControl = player.GetComponent<PlayerControl>();
                playerControl.PlayerDeath += OnDeathPlayer;
                playerControl.SetCamera(camera);
                playerControl.SetScore(_score);
                
                #if UNITY_ANDROID
                var canvas = Instantiate(_canvasTemplate, Vector3.zero, Quaternion.identity);
                playerControl.SetJoystick(canvas);
                #endif
                
                return;
            }
        }
    }
    

    private void OnDeathPlayer(PlayerControl player)
    {
        StartCoroutine(Respawning(player));
    }

    private void Respawn(PlayerControl playerControl)
    {
        for (int i = 0; i < 20; i++)
        {
            var position = Random.insideUnitCircle * _radius;
            Collider2D[] hitColliders = new Collider2D[1];
            var numsCollider = Physics2D.OverlapCircleNonAlloc(position, 0.7f, hitColliders);
            if (numsCollider == 0)
            {
                playerControl.Respawn(position);
                return;
            }
        }
    }
    
    private IEnumerator Respawning(PlayerControl player)
    {
        _deathScreen.SetActive(true);
        float elapsedTime = 0;
        var text = _deathScreen.transform.GetChild(2).GetComponent<TextMeshProUGUI>();
        
        while (elapsedTime < _respawnCooldown)
        {
            text.text = $"До респавна осталось {_respawnCooldown - elapsedTime} секунд.";
            yield return new WaitForSeconds(0.5f);
            elapsedTime += 0.5f;
        }
        
        Respawn(player);
        _deathScreen.SetActive(false);
    }
    
    private void Log(string message)
    {
        Debug.Log(message);
        _log.text += "\n" + message;
    }
    
    private void ClearLog()
    {
        _log.text = "Инфо:\n";
    }

}
