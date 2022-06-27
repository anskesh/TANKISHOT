using System.Collections.Generic;
using UnityEngine;

public class PlayersView : MonoBehaviour
{
    public static PlayersView Instance;

    private void Awake()
    {
        if (Instance == null)
            Instance = this;
    }

    public PlayerControl[] GetPlayers()
    {
        List<PlayerControl> childrens = new List<PlayerControl>();
        
        for (int i = 0; i < transform.childCount; i++)
        {
            var children = transform.GetChild(i).GetComponent<PlayerControl>();
            childrens.Add(children);
        }

        return childrens.ToArray();
    }
}
