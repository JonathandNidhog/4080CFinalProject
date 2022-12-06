using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;

public class EnemyAi : NetworkBehaviour
{
    public NetworkVariable<int> Hp = new NetworkVariable<int>(5);

    public float attackDis = 10;

    public float attackTime = 5f;
    public float timer = 0f;

    private BulletSpawner _bulletSpawner;

    public Slider HpSlider;
    // Start is called before the first frame update
    void Start()
    {
        Hp.OnValueChanged += ClientOnHpChanged;
        _bulletSpawner = transform.Find("RArm").transform.Find("BulletSpawner").GetComponent<BulletSpawner>();
    }

    public void ClientOnHpChanged(int previous, int current)
    {
        HpSlider.value = Hp.Value / 5f;
      
    }

    // Update is called once per frame
    void Update()
    {
        if (IsHost) {

          var players=  GameObject.FindGameObjectsWithTag("player");

            float dis=Mathf.Infinity;
            GameObject target=null;
            for (var i = 0; i < players.Length; i++) {
                var newDis = Vector3.Distance(players[i].transform.position, this.transform.position);
                if (dis > newDis) {
                    target = players[i];
                    dis = newDis;
                }
            }

            if (dis< attackDis) {
                GetComponent<NavMeshAgent>().isStopped = true;
                if ( Time.realtimeSinceStartup- timer > attackTime) {
                    Attack();
                }
               
            }
            else {
                GetComponent<NavMeshAgent>().isStopped = false;
                GetComponent<NavMeshAgent>().SetDestination(target.transform.position);
            }
              
        }
    }
    public void OnCollisionEnter(Collision collision)
    {
        if (IsHost)
        {
            if (collision.gameObject.CompareTag("Bullet"))
            {
                if (Hp.Value <= 0)
                {

                    //»÷É±µÃ·Ö
                    ulong ownerClientId = collision.gameObject.GetComponent<NetworkObject>().OwnerClientId;
                    Player otherPlayer = NetworkManager.Singleton.ConnectedClients[ownerClientId].PlayerObject.GetComponent<Player>();
                    otherPlayer.Score.Value += 10;
                    Destroy(this.gameObject);
                }
                HostHandleBulletCollision(collision.gameObject);
            }
        }
    }

    private void HostHandleBulletCollision(GameObject bullet)
    {
        Bullet bulletScript = bullet.GetComponent<Bullet>();

        Hp.Value -= bulletScript.Damage.Value;
        Destroy(bullet);
    }
    public void Attack() {
        _bulletSpawner.FireEnemy();
        timer = Time.realtimeSinceStartup;
    }
}
