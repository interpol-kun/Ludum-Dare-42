using UnityEngine;
using UnityEngine.AI;
using System.Collections;

public class AI : MonoBehaviour, IDamagable {

    public float health;
    private bool dead;

    [SerializeField]
    private float bulletSpread;
    [SerializeField]
    private Projectile projectile;
    [SerializeField]
    private Transform shotPos;
    private float damage = 1f;
    [SerializeField]
    private float shootCD = 1f;
    private float attackDistance = 10f;

    [SerializeField]
    private Mesh[] meshes;
    private MeshFilter meshFilter;
    [SerializeField]
    private GameObject hitParticle = null;

    float nextShotTime;
    bool moving = false;

    [SerializeField]
    private Player player;
    private NavMeshAgent myAgent;
    private Rigidbody myRB;

    void Start() {
        meshFilter = GetComponent<MeshFilter>();
        myAgent = GetComponent<NavMeshAgent>();
        myRB = GetComponent<Rigidbody>();

        player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();

        meshFilter.mesh = meshes[Mathf.RoundToInt(health) - 1];

        myRB.isKinematic = true;

        StartCoroutine(Behaviour());
    }

    void Update(){
        if(!IsGrounded() && myAgent.enabled){
            StopAllCoroutines();
            myAgent.isStopped = true;
            myAgent.enabled = false;
            myRB.isKinematic = false;
        }
    }

    bool IsGrounded(){
        if (Physics.Raycast(transform.position, transform.up * -1, 1f))
            return true;
        else
            return false;
    }

    IEnumerator Behaviour() {
        while (!dead) {
            if (Vector3.Distance(transform.position, player.transform.position) > attackDistance)
                yield return StartCoroutine(MoveCloser());
            else {
                if (Time.time >= nextShotTime)
                    TryShoot();
                if (!moving)
                    RandomMovement();
            }
            yield return null;
        }
    }

    IEnumerator MoveCloser(){
        myAgent.isStopped = false;
        while(Vector3.Distance(transform.position, player.transform.position) > attackDistance * .75f && !dead){
            myAgent.SetDestination(player.transform.position);
            yield return null;
        }
        myAgent.isStopped = true;
        yield return null;
    }

    void RandomMovement() {
        myAgent.isStopped = false;
        moving = true;
        Vector3 dest = new Vector3(transform.position.x + Random.Range(-5f, 5f), transform.position.y, transform.position.z + Random.Range(-5f, 5f));
        myAgent.SetDestination(dest);
    }

    void TryShoot(){
        if (Vector3.Angle(transform.forward, player.transform.position - transform.position) > 5f) {
            Quaternion qTo = Quaternion.LookRotation(player.transform.position - transform.position, transform.up);
            transform.rotation = Quaternion.Lerp(transform.rotation, qTo, Time.deltaTime * 10f);
        } else
            Shoot();
    }

    void Shoot(){
        myAgent.isStopped = true;
        moving = false;
        Quaternion bulletRot = Quaternion.Euler(0, Random.Range(-bulletSpread, bulletSpread), 0);
        Projectile prj = Instantiate(projectile, shotPos.position, shotPos.rotation * bulletRot) as Projectile;
        prj.SetStats(damage, this);
        nextShotTime = Time.time + shootCD;
    }

    public void TakeDamage(float damageToTake, IDamagable damager){
        health -= damageToTake;
        if (health <= 0) {
            //add points to damager
            Death();
        } else {
            Instantiate(hitParticle, transform.position, Quaternion.identity);
            meshFilter.mesh = meshes[Mathf.RoundToInt(health) - 1];
        }
    }

    public void Death(){
        Instantiate(hitParticle, transform.position, Quaternion.identity);
        dead = true;
        if(myAgent.enabled)
            myAgent.isStopped = true;
        Spawner.instance.KilledEnemy(this);
        Destroy(this.gameObject);
    }
}
