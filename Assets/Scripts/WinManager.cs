using Unity.Netcode;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;

public class WinManager : NetworkBehaviour
{

    public TMPro.TMP_Text UiText;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        int maxSorce = 0;
        string playerName = "";
        var allPlayers = GameObject.FindGameObjectsWithTag("player");
        for (var i = 0; i < allPlayers.Length; i++)
        {

            if (allPlayers[i].GetComponent<Player>().Score.Value > maxSorce)
            {
                maxSorce = allPlayers[i].GetComponent<Player>().Score.Value;
                playerName = allPlayers[i].GetComponent<Player>().OwnerClientId.ToString();
            }
        }
        var str = "Player" + playerName;
        UiText.text = str;
    }
}
