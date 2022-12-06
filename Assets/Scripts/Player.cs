using System.Reflection;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;


public class Player : NetworkBehaviour {


    public NetworkVariable<Vector3> PositionChange = new NetworkVariable<Vector3>();
    public NetworkVariable<Vector3> RotationChange = new NetworkVariable<Vector3>();
    public NetworkVariable<Color> PlayerColor = new NetworkVariable<Color>(Color.red);
    public NetworkVariable <int> Score = new NetworkVariable<int>(5);

    public NetworkVariable<int> Hp = new NetworkVariable<int>(5);
    //public NetworkVariable <int> Health = new NerworkVariable<int>(100);
    public TMPro.TMP_Text txtScoreDisplay;

    private GameManager _gameMgr;
    private Camera _camera;
    public float movementSpeed = 4f;
    private float rotationSpeed = 4f;
    private BulletSpawner _bulletSpawner;
    public Image HPBar;
    public Image HeadHPBar;
    public GameObject playerScrollContent;
    public ScorePanel ScorePanelPreferb;

    public HPController hPController;
    public GameObject PlayerUi;
    public Slider HpSlider;
    //----------------------------------------
    // Behaviour
    //----------------------------------------
    private void Start() 
    {
        ApplyPlayerColor();
        PlayerColor.OnValueChanged += OnPlayerColorChanged;
        if (!IsLocalPlayer)
            PlayerUi.SetActive(false);
    }

    public override void OnNetworkSpawn() 
    {
        _camera = transform.Find("Camera").GetComponent<Camera>();
        _camera.enabled = IsOwner;

        Score.OnValueChanged += ClientOnScoreChanged;
        Hp.OnValueChanged += ClientOnHpChanged;
        _bulletSpawner = transform.Find("RArm").transform.Find("BulletSpawner").GetComponent<BulletSpawner>();
        if (IsHost) {
            _bulletSpawner.BulletDamage.Value = 1;
        }
        DisplayScore();
    }

    void Update() 
    {
        if (IsOwner) {
            Vector3[] results = CalcMovement();
            RequestPositionForMovementServerRpc(results[0], results[1]);
            if (Input.GetButtonDown("Fire1")) {
                _bulletSpawner.FireServerRpc();
            }
        }

        if(!IsOwner || IsHost){
            transform.Translate(PositionChange.Value);
            transform.Rotate(RotationChange.Value);
        }

      //  AddScorePanel();
    }

//---------------------------------
//Private
//------------------------------
    // horiz changes y rotation or x movement if shift down, vertical moves forward and back.
    private Vector3[] CalcMovement() 
    {
        bool isShiftKeyDown = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        float x_move = 0.0f;
        float z_move = Input.GetAxis("Vertical");
        float y_rot = 0.0f;

        if (isShiftKeyDown) {
            x_move = Input.GetAxis("Horizontal");
        } else {
            y_rot = Input.GetAxis("Horizontal");
        }

        Vector3 moveVect = new Vector3(x_move, 0, z_move);
        moveVect *= movementSpeed;

        Vector3 rotVect = new Vector3(0, y_rot, 0);
        rotVect *= rotationSpeed;

        return new[] { moveVect, rotVect };
    }

    private void HostHandleBulletCollision(GameObject bullet) 
    {
        Bullet bulletScript = bullet.GetComponent<Bullet>();
        //   Score.Value -= bulletScript.Damage.Value;
        Hp.Value -= bulletScript.Damage.Value;
       // ulong ownerClientId = bullet.GetComponent<NetworkObject>().OwnerClientId;
        //Player otherPlayer = NetworkManager.Singleton.ConnectedClients[ownerClientId].PlayerObject.GetComponent<Player>();
        //otherPlayer.Score.Value += bulletScript.Damage.Value;

        Destroy(bullet);
    }

    private void HostHandleDamageBoostPickup(Collider other) 
    {
        if (!_bulletSpawner.IsAtMaxDamage()){
            _bulletSpawner.IncreaseDamage();
            Score.Value += 15;
           
            other.GetComponent<NetworkObject>().Despawn();
        }
    }
    private void HostHandleHpAddPickup(Collider other)
    {
            Hp.Value += 1;
            other.GetComponent<NetworkObject>().Despawn();
        
    }
    
    private void AddScorePanel()
    {
        ScorePanel newPanel = Instantiate(ScorePanelPreferb);
        newPanel.SetColor(PlayerColor.Value);
        


    }

// ---------------------
// Events
// ---------------------
    private void ClientOnScoreChanged(int previous, int current)
    {
        DisplayScore();
    }


    private void ClientOnHpChanged(int previous, int current)
    {
        DisplayHp();
    }


    private void OnPlayerColorChanged(Color previous, Color current) 
    {
        ApplyPlayerColor();
    }
    public void OnCollisionEnter(Collision collision) 
    {
        if (IsHost) {
            if (collision.gameObject.CompareTag("Bullet")) {
                if(Hp.Value <= 0){
                    // NetworkManager.SceneManager.LoadScene("DIE", UnityEngine.SceneManagement.LoadSceneMode.Single);

                    ulong ownerClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                    Player otherPlayer = NetworkManager.Singleton.ConnectedClients[ownerClientId].PlayerObject.GetComponent<Player>();
                   
                    otherPlayer.Score.Value += 20;
                   
                    Destroy(this.gameObject);
                }
                HostHandleBulletCollision(collision.gameObject);
            }
        }
    }

    private void OnPlayerKicked(ulong clientId)
    {
        NetworkManager.Singleton.DisconnectClient(clientId);
        GameData.Instance.RemovePlayerFromList(clientId);
    }

    public void OnTriggerEnter(Collider other)
    {
        if(IsHost){
            if(other.gameObject.CompareTag("DamageBoost")) {
                HostHandleDamageBoostPickup(other);
            }

            if (other.gameObject.CompareTag("HpAdd"))
            {
                HostHandleHpAddPickup(other);
            }
        }
    }
//------------------------
// RPC
//------------------------
    [ServerRpc]
    void RequestPositionForMovementServerRpc(Vector3 posChange, Vector3 rotChange) 
    {
        if (!IsServer && !IsHost) return;

        PositionChange.Value = posChange;
        RotationChange.Value = rotChange;
    }

    [ServerRpc]
    public void RequestSetScoreServerRpc(int value) 
    {
        Score.Value = value;
    }
//---------------------------
//publuc
//------------------------------
    public void ApplyPlayerColor() 
    {
        GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
        
        transform.Find("RArm").GetComponent<MeshRenderer>().material.color = PlayerColor.Value;
    }
    public void DisplayScore() {

        if (Score.Value > 50) {
            NetworkManager.SceneManager.LoadScene("Win", UnityEngine.SceneManagement.LoadSceneMode.Single);
        }
        //if (Score.Value > 0&&IsOwner && GameObject.Find("GameManager").GetComponent<GameManager>()) {
        //    GameObject.Find("GameManager").GetComponent<GameManager>().ChangeRankServerRpc();
        //}
     
        txtScoreDisplay.text = Score.Value.ToString();
    }


    public void DisplayHp()
    {
        HpSlider.value = Hp.Value / 5f;
        hPController.ShowHp(Hp.Value);
    }



}