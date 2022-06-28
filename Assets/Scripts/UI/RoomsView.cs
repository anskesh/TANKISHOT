using System;
using System.Collections.Generic;
using System.Linq;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;
using UnityEngine.UI;

public class RoomsView : MonoBehaviour
{
    [SerializeField] private RoomItem _template;

    private List<RoomItem> _items = new List<RoomItem>();
    private ContentSizeFitter _container;

    private void Awake()
    {
        _container = GetComponentInChildren<ContentSizeFitter>();
        var items = GetComponentsInChildren<RoomItem>();
        
        foreach (var roomItem in items)
        {
            Destroy(roomItem.gameObject);
        }
    }

    public void RenderRooms(List<RoomInfo> rooms)
    {
        for (int i = 0; i < rooms.Count; i++)
        {
            if (rooms[i].PlayerCount == 0)
                continue;
            
            if (i < _items.Count)
                _items[i].Render(rooms[i]);
            else
            {
                var item = Instantiate(_template, _container.transform);
                item.Render(rooms[i]);
                _items.Add(item);
            }
        }

        if (rooms.Count < _items.Count)
        {
            for (int i = rooms.Count - 1; i < _items.Count; i++)
            {
                Destroy(_items[i].gameObject); 
            }
            
            Debug.Log(_items.Count);
            _items = _items.Where(item => item != null).ToList();
            Debug.Log(_items.Count);
        }
    }
}
