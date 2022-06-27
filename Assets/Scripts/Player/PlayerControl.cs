using System;
using ExitGames.Client.Photon;
using Photon.Pun;
using Photon.Realtime;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(SpriteRenderer))]
public class PlayerControl : MonoBehaviour, IDamageable, IPunObservable, IOnEventCallback
{
	public int Score { get; private set; } = 0;
	public event Action<PlayerControl> PlayerDeath;

	[SerializeField] private float _speed;
	[SerializeField] private Transform _bulletSpawnPoint;
	[SerializeField] private Bullet _template;
	[SerializeField] private float _cooldownBetweenShoot;
	[SerializeField] private Sprite _enemySkin;
 	
	private PhotonView _photonView;
	private Camera _camera;
	private Joystick _joystick;
	private TextMeshProUGUI _score;
	private SpriteRenderer _spriteRenderer;
	
	private double _lastShootTime;
	private bool _isDeath;

	private void Awake()
	{
		_photonView = GetComponent<PhotonView>();
		_lastShootTime = PhotonNetwork.Time;
		transform.SetParent(PlayersView.Instance.transform);
		_spriteRenderer = GetComponent<SpriteRenderer>();

		if (!_photonView.IsMine)
			_spriteRenderer.sprite = _enemySkin;
	}

	private void Update()
	{
		ChangePosition();

		if (Input.GetKey(KeyCode.Space) && _photonView.IsMine)
			Shoot();
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

		if (_photonView.IsMine)
			return;
		
		RaiseEventOptions options = new RaiseEventOptions() {TargetActors = new []{_photonView.Owner.ActorNumber}};
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

	public void SetCamera(Camera camera)
	{
		_camera = camera;
		_camera.transform.position = new Vector3(transform.position.x, transform.position.y, -10);
	}

	public void SetScore(TextMeshProUGUI score)
	{
		_score = score;
		_score.text = $"Мой счёт: {Score}";
	}
	
	public void SetJoystick(GameObject canvas)
	{
		var joystick = canvas.GetComponentInChildren<Joystick>();
		if (joystick != null)
			_joystick = joystick;
		var button = canvas.GetComponentInChildren<Button>();
		button.onClick.AddListener(Shoot);
	}
	
	private void ChangePosition()
	{
		if (!_photonView.IsMine)
		{
			return;
		}
		#if UNITY_ANDROID
		var x = _joystick.Horizontal;
		var y = _joystick.Vertical;
		#endif
		
		#if !UNITY_ANDROID
		var x = Input.GetAxis("Horizontal");
		var y = Input.GetAxis("Vertical");
		#endif
		
		if (x == 0 && y == 0) 
			return;

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
			
			gameObject.SetActive(!_isDeath);
			PlayersTop.Instance.RenderTop();
			
			if (_score != null)
				_score.text = $"Мой счёт: {Score}";
		}
		else
		{
			Score = (int) stream.ReceiveNext();
			_isDeath = (bool) stream.ReceiveNext();
			
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
				if (_photonView.IsMine)
					ApplyDamage();
				break;
			}
		}
	}
}
