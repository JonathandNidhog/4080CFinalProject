using System.Collections.Generic;
using Unity.Netcode;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
//using static System.Net.Mime.MediaTypeNames;

public class GameManager : NetworkBehaviour {
    public Player playerPrefab;
    public GameObject enemyPrefab;
    public Transform[] enemyPostions;
    public GameObject spawnPoints;
    public GameObject chatUiParent;
    public GameObject chatUi;
    public TMPro.TMP_Text UiText;
    public GameObject HscoreColor;

    private int spawnIndex = 0;
    private List<Vector3> availableSpawnPositions = new List<Vector3>();

    public List<Player> players = new List<Player>();

    
    private void Start(){
        GameData.dbgRun.StartGameWithSceneIfNotStarted();
      
    }

    public void Update()
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
                SetColor(allPlayers[i].GetComponent<Player>().PlayerColor.Value);
            }
        }
        var  str = "Player" + playerName+ ":" + maxSorce.ToString();
       
        UiText.text = str; 

    }

    //[ServerRpc(RequireOwnership = false)]
    //public void ChangeRankServerRpc() {
    //    int maxSorce = 0;
    //    Player player = null;
    //    for (var i = 0; i < players.Count; i++)
    //    {

    //        if (players[i].Score.Value > maxSorce)
    //        {
    //            maxSorce = players[i].Score.Value;
    //            player = players[i];
    //        }
    //    }
    //    var str = "";
    //    if (player != null)
    //        str = "Player" + player.gameObject.GetComponent<NetworkObject>().OwnerClientId + ":" + maxSorce.ToString();
    //    else
    //    {
    //        str = "";
    //    }
    //    ShowTextClientRpc(str);
    //}
    //[ClientRpc]
    //public void ShowTextClientRpc(string message, ClientRpcParams clientRpcParams = default)
    //{
    //    UiText.text = message;

    //}
    public void Awake(){
        refreshSpawnPoints();
    }

    public override void OnNetworkSpawn(){
        if(IsHost){
            SpawnPlayers();
            SpawnEnemys();
            SpawnEnemys();
        }
    }

    public void SetColor(Color c)
    {
        HscoreColor.GetComponent<Image>().color = c;
    }

    private void refreshSpawnPoints() {
        Transform[] allPoint = spawnPoints.GetComponentsInChildren<Transform>();
        availableSpawnPositions.Clear();
        foreach(Transform point in allPoint){
            if (point != spawnPoints.transform){
            availableSpawnPositions.Add(point.localPosition);
            }
        }
    }

    public Vector3 GetNextSpawnLocation(){
        var newPosition = availableSpawnPositions[spawnIndex];
        newPosition.y = 1.5f;
        spawnIndex += 1;

        if (spawnIndex > availableSpawnPositions.Count - 1){
            spawnIndex = 0;
        }
        return newPosition;
    }

    private void SpawnPlayers() {
        foreach (PlayerInfo info in GameData.Instance.allPlayers){
            SpawnPlayers(info);
        }
    }
    private void SpawnPlayers(PlayerInfo info){
        Player playerSpawn = Instantiate(
            playerPrefab,
            GetNextSpawnLocation(),
            Quaternion.identity);

        playerSpawn.GetComponent<NetworkObject>().SpawnAsPlayerObject(info.clientId);
        playerSpawn.PlayerColor.Value = info.color;
        players.Add(playerSpawn);
        //playerSpawn.Score.OnValueChanged += HostOnPlayerScoreChanged;
    }
    private void SpawnEnemys()
    {
      var enemy=  Instantiate(
               enemyPrefab, enemyPostions[Random.Range(0, enemyPostions.Length)].position,
               Quaternion.identity,null);
        enemy.GetComponent<NetworkObject>().Spawn();
    }

    private void HostOnClientConnected(ulong clientId){
        int playerIndex = GameData.Instance.FindPlayerIndex(clientId);
        if(playerIndex != -1){
            PlayerInfo newPlayerInfo = GameData.Instance.allPlayers[playerIndex];
            SpawnPlayers(newPlayerInfo);
        }
    }



  
}