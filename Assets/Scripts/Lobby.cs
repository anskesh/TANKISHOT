using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class Lobby : MonoBehaviourPunCallbacks
{
	[SerializeField] private TextMeshProUGUI _log;
	[SerializeField] private TMP_InputField _nickname;
	[SerializeField] private TMP_InputField _roomName;
	private void Start()
	{
		if (PhotonNetwork.NickName == string.Empty)
			PhotonNetwork.NickName = "Player" + Random.Range(0, 10000);
		
		_nickname.text = PhotonNetwork.NickName;
		PhotonNetwork.GameVersion = "V1.0";
		PhotonNetwork.AutomaticallySyncScene = true;
		PhotonNetwork.ConnectUsingSettings();
		Log("Connecting..");
		
		InvokeRepeating(nameof(ClearLog), 1, 20);
	}

	public void JoinRoom()
	{
		if (_roomName.text == string.Empty)
			PhotonNetwork.JoinRandomRoom();
		else
			PhotonNetwork.JoinRoom(_roomName.text);
		
		Log("Подключение к комнате...");
	}

	public void JoinRandomRoom()
	{
		Log("Подключение к комнате...");
		PhotonNetwork.JoinRandomOrCreateRoom();
	}

	public void CreateRoom()
	{
		if (_roomName.text == string.Empty)
			PhotonNetwork.CreateRoom(null);
		else
			PhotonNetwork.CreateRoom(_roomName.text);
		
		Log("Создание комнаты...");
	}

	public void ChangeNickname()
	{
		if (_nickname.text == string.Empty)
			return;
		else
		{
			if (PhotonNetwork.NickName == _nickname.text)
				return;
			
			PhotonNetwork.NickName = _nickname.text;
			Log($"Вы изменили никнейм на {PhotonNetwork.NickName}");
		}
	}
	
	public override void OnConnectedToMaster()
	{
		Log("Вы успешно подключились.");
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Log("Ошибка подключения.");
	}

	public override void OnJoinedRoom()
	{
		Log("Вы подключились в команту.");
		SceneManager.LoadScene(1);
	}
	
	public override void OnJoinRoomFailed(short returnCode, string message)
	{
		Log("Такой комнаты не существует.");
	}

	public override void OnCreatedRoom()
	{
		PhotonNetwork.LoadLevel(1);
	}

	public override void OnCreateRoomFailed(short returnCode, string message)
	{
		Log("Такая комната уже существует.");
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
