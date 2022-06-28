using System.Collections.Generic;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class Lobby : MonoBehaviourPunCallbacks
{
	[SerializeField] private TextMeshProUGUI _log;
	[SerializeField] private TMP_InputField _nickname;
	[SerializeField] private TMP_InputField _roomName;
	[SerializeField] private Button _exit;
	[SerializeField] private RoomsView _roomsView;

	private void Start()
	{
		string nickname = "";
		
		if (!GetNickname(out nickname) && PhotonNetwork.NickName == string.Empty)
			PhotonNetwork.NickName = "Player" + Random.Range(0, 10000);
		else
			PhotonNetwork.NickName = nickname;

		_nickname.text = PhotonNetwork.NickName;

		if (PhotonNetwork.InLobby)
		{
			Log("Вы уже в лобби.");
			return;
		}
		
		PhotonNetwork.GameVersion = GlobalVariables.GameVersion;
		PhotonNetwork.AutomaticallySyncScene = true;

		PhotonNetwork.ConnectUsingSettings();
		Log("Подключение..");

		InvokeRepeating(nameof(ClearLog), 1, 20);
	}

	public override void OnEnable()
	{
		base.OnEnable();
		_exit.onClick.AddListener(Exit);
	}

	public override void OnDisable()
	{
		base.OnDisable();
		_exit.onClick.RemoveListener(Exit);
	}

	private void Update()
	{
		if (Input.GetKey(KeyCode.Escape))
			Exit();
	}

	public void JoinRoom()
	{
		if (!PhotonNetwork.InLobby)
			Log("Вы не в лобби, переподключитесь.");
		
		if (_roomName.text == string.Empty)
			PhotonNetwork.JoinRandomRoom();
		else
			PhotonNetwork.JoinRoom(_roomName.text);

		Log("Подключение к комнате...");
	}

	public void JoinRandomRoom()
	{
		if (!PhotonNetwork.InLobby)
			Log("Вы не в лобби, переподключитесь.");
		
		Log("Подключение к комнате...");
		PhotonNetwork.JoinRandomOrCreateRoom(null, 20, MatchmakingMode.FillRoom, TypedLobby.Default, null, $"Room№{PhotonNetwork.CountOfRooms + 1}");
	}

	public void CreateRoom()
	{
		if (!PhotonNetwork.InLobby)
			Log("Вы не в лобби, переподключитесь.");
		
		if (_roomName.text == string.Empty)
			PhotonNetwork.CreateRoom($"Room№{PhotonNetwork.CountOfRooms + 1}");
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
			SaveNickname(PhotonNetwork.NickName);
			
			Log($"Вы изменили никнейм на {PhotonNetwork.NickName}");
		}
	}

	public override void OnConnectedToMaster()
	{
		PhotonNetwork.JoinLobby();
	}

	public override void OnRoomListUpdate(List<RoomInfo> roomList)
	{
		base.OnRoomListUpdate(roomList);
		_roomsView.RenderRooms(roomList);
	}

	public override void OnDisconnected(DisconnectCause cause)
	{
		Log("Ошибка подключения.");
		Log("Попытка восстановить подключение.");
		if (PhotonNetwork.LocalPlayer.HasRejoined)
			PhotonNetwork.ReconnectAndRejoin();
		
	}

	public override void OnJoinedLobby()
	{
		Log("Вы подключились к лобби.");
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

	private bool GetNickname(out string nickname)
	{
		if (PlayerPrefs.HasKey(GlobalVariables.NicknamePrefs))
		{
			nickname = PlayerPrefs.GetString(GlobalVariables.NicknamePrefs);
			return true;
		}
		else
		{
			nickname = "";
			return false;
		}
	}

	private void SaveNickname(string nickname)
	{
		PlayerPrefs.SetString(GlobalVariables.NicknamePrefs, nickname);
	}

	private void Exit()
	{
		Application.Quit();
	}
}
