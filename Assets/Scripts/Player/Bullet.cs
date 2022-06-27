using System.Linq;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviour
{
	[SerializeField] private float _speed;

	private PhotonView _view;
	private PlayerControl _player;

	private void Awake()
	{
		_view = GetComponent<PhotonView>();
	}

	private void Update()
	{
		transform.Translate(Vector2.right * Time.deltaTime * _speed, Space.Self);
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (!_view.IsMine)
			return;
		
		if (col.TryGetComponent(out IDamageable enemy))
		{
			if (!col.GetComponent<PhotonView>().IsMine)
			{
				Debug.Log(col.name);
				enemy.ApplyDamage();
				var player = PlayersView.Instance.GetPlayers()
					.Where(p => p.GetComponent<PhotonView>().Owner.UserId == _view.Owner.UserId);
				player.First().IncreaseScore();
				if (_view.IsMine)
					PhotonNetwork.Destroy(_view);
			}
		}
		else
		{
			if (_view.IsMine)
				PhotonNetwork.Destroy(_view);
		}
	}

}
