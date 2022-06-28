using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerControl : MonoBehaviourPun, IDamageable, IPunObservable, IOnEventCallback
{
	public int Score { get; private set; } = 0;
	public event Action<PlayerControl> PlayerDeath;

	[SerializeField] private float _speed;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private Bullet _template;
	[SerializeField] private float _cooldownBetweenShoot;
	
	[SerializeField] private Canvas _nickname;
 	
	private Camera _camera;
	private Joystick _joystick;
	private TextMeshProUGUI _score;
	private Animator _animator;
	
	private double _lastShootTime;
	private bool _isDeath;
	private bool _isMove;
	private bool _settingsAreFinished;

	private void Awake()
	{
		_lastShootTime = PhotonNetwork.Time;
		_animator = GetComponent<Animator>();
	}

	private void Start()
	{
		if (!photonView.IsMine)
		{
			var nicknameCanvas = Instantiate(_nickname, Vector3.zero, Quaternion.identity);
			nicknameCanvas.GetComponentInChildren<NicknameFollow>().Render(this);
			
			_animator.SetTrigger("enemy");
		}
		
		transform.SetParent(PlayersView.Instance.transform);
	}
	
	private void Update()
	{
		if (!_settingsAreFinished)
			return;

		if (photonView.IsMine)
		{
			ChangePosition();

			if (Input.GetKey(KeyCode.Space))
				Shoot();
		}
	}

	private void OnEnable()
	{
		PhotonNetwork.AddCallbackTarget(this);
	}

	private void OnDisable()
	{
		PhotonNetwork.RemoveCallbackTarget(this);
	}

	public void ApplyDamage()
	{
		PlayerDeath?.Invoke(this);
		_isDeath = true;

		if (photonView.IsMine)
			return;
		
		RaiseEventOptions options = new RaiseEventOptions() {TargetActors = new []{photonView.Owner.ActorNumber}};
		SendOptions sendOptions = new SendOptions() {Reliability = true};
		PhotonNetwork.RaiseEvent(20, _isDeath, options, sendOptions);
	}

	public void IncreaseScore()
	{
		Score++;
	}
	
	public void Respawn(Vector3 position)
	{
		transform.position = position;
		_camera.transform.position = new Vector3(position.x, position.y, -10);
		transform.rotation = new Quaternion(0, 0, 0, 0);
		_isDeath = false;
		gameObject.SetActive(!_isDeath);
	}

	public void SetSettings(Camera camera, TextMeshProUGUI score, GameObject canvas)
	{
		SetCamera(camera);
		SetScore(score);
		SetJoystick(canvas);
		_settingsAreFinished = true;
	}

	public void SetTrigger()
	{
		_animator.SetTrigger("enemy");
	}
	
	private void SetCamera(Camera camera)
	{
		_camera = camera;
		_camera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}

	private void SetScore(TextMeshProUGUI score)
	{
		_score = score;
		_score.text = $"Мой счёт: {Score}";
	}
	
	private void SetJoystick(GameObject canvas)
	{
		var joystick = canvas.GetComponentInChildren<Joystick>();
		if (joystick != null)
			_joystick = joystick;
		var button = canvas.GetComponentInChildren<Button>();
		button.onClick.AddListener(Shoot);
		
		#if !UNITY_ANDROID
		canvas.SetActive(false);
		#endif
	}
	
	private void ChangePosition()
	{
		#if UNITY_ANDROID
		var x = _joystick.Horizontal;
		var y = _joystick.Vertical;
		#endif
		
		#if !UNITY_ANDROID
		var x = Input.GetAxis("Horizontal");
		var y = Input.GetAxis("Vertical");
		#endif

		if (MathF.Abs(x) <= 0.02 && MathF.Abs(y) <= 0.02)
		{
			_isMove = false;
			_animator.SetBool("isMoveFriend", _isMove);
			base.photonView.RPC(nameof(SetState), RpcTarget.Others, _isMove);
			return;
		}

		_isMove = true;
		_animator.SetBool("isMoveFriend", _isMove);
		base.photonView.RPC(nameof(SetState), RpcTarget.Others, _isMove);

		var direction = new Vector3(x, y, 0);

		transform.position = Vector3.MoveTowards(transform.position, transform.position + direction, _speed * Time.deltaTime);
		var angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;
		transform.rotation = Quaternion.Euler(0,0 ,angle);
		
		_camera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}

	private void Shoot()
	{
		if (_lastShootTime + _cooldownBetweenShoot <= PhotonNetwork.Time)
		{
			var bullet = PhotonNetwork.Instantiate(_template.name, _bulletSpawnPoint.position, transform.rotation);
			_lastShootTime = PhotonNetwork.Time;
		}
	}

	public void OnPhotonSerializeView(PhotonStream stream, PhotonMessageInfo info)
	{
		if (stream.IsWriting)
		{
			stream.SendNext(Score);
			stream.SendNext(_isDeath);
			stream.SendNext(_isMove);
			
			gameObject.SetActive(!_isDeath);
			PlayersTop.Instance.RenderTop();
			
			if (_score != null)
				_score.text = $"Мой счёт: {Score}";
		}
		else
		{
			Score = (int) stream.ReceiveNext();
			_isDeath = (bool) stream.ReceiveNext();
			_isMove = (bool) stream.ReceiveNext();
 			
			gameObject.SetActive(!_isDeath);
			PlayersTop.Instance.RenderTop();
			
			if (_score != null)
				_score.text = $"Мой счёт: {Score}";
		}
	}
	
	public void OnEvent(EventData photonEvent)
	{
		switch (photonEvent.Code)
		{
			case 20:
			{
				if (photonView.IsMine)
					ApplyDamage();
				break;
			}
		}
	}

	[PunRPC]
	void SetState(bool isMove)
	{
		_isMove = isMove;
		_animator.SetBool("isMoveEnemy", _isMove);
	}
}
