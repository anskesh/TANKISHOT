using System.Linq;
using Photon.Pun;
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class Bullet : MonoBehaviourPun
{
	[SerializeField] private float _speed;

	private PlayerControl _player;


	private void Update()
	{
		transform.Translate(Vector2.right * Time.deltaTime * _speed, Space.Self);
	}

	private void OnTriggerEnter2D(Collider2D col)
	{
		if (!photonView.IsMine)
			return;
		
		if (col.TryGetComponent(out IDamageable enemy))
		{
			if (!col.GetComponent<PhotonView>().IsMine)
			{
				enemy.ApplyDamage();
				var player = PlayersView.Instance.GetPlayers()
					.Where(p => p.GetComponent<PhotonView>().Owner.UserId == photonView.Owner.UserId);
				player.First().IncreaseScore();
				PhotonNetwork.Destroy(photonView);
			}
		}
		else
		{
			PhotonNetwork.Destroy(photonView);
		}
	}

}
