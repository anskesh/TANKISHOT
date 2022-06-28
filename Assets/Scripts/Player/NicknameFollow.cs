using Photon.Pun;
using TMPro;
using UnityEngine;

public class NicknameFollow : MonoBehaviour
{
    private readonly Vector3 _offset = new Vector3(0, 0.85f, 0);
    private PlayerControl _player;
    private TextMeshProUGUI _text;

    private void Awake()
    {
        _text = GetComponentInChildren<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (_player == null)
        {
            Destroy(gameObject);
            return;
        }
        
        if (_player.gameObject.activeSelf == false)
            _text.gameObject.SetActive(false);
        
        if (transform.position - _offset == _player.transform.position)
            return;

        if (_text.gameObject.activeSelf == false)
        {
            _text.gameObject.SetActive(true);
            _player.SetTrigger();
        }            
            
        transform.position = _player.transform.position + _offset;
    }

    public void Render(PlayerControl player)
    {
        _player = player;
        transform.position = player.transform.position + _offset;
        _text.text = player.GetComponent<PhotonView>().Owner.NickName;
    }
}
