using UnityEngine;
using UnityEngine.AI;
using System.Collections;
using System.Collections.Generic;

public class Spawner : MonoBehaviour {

    public static Spawner instance;

    public List<AI> enemies;

    private NavMeshSurface surface;
    [SerializeField]
    private GameObject enemyToSpawn = null;
    [SerializeField]
    private GameObject spawnParticle = null;

    public delegate void EndOfWave();
    public static EndOfWave endOfWaveEvent;

    public delegate void UpdateWave(int value);
    public static UpdateWave OnWaveEnd;


    //Получить количество оставшихся врагов - currentWave.amountOfEnemies
    public Wave currentWave;
    public int completedWaves;


    [System.Serializable]
    public class Wave{
        public int amountOfEnemies = 1;
        // 0-1, шанс получить больше хп
        public float waveDifficulty = .5f;
        public float spawnTime = 4f;

        public Wave(int count, float dif, float timer){
            amountOfEnemies = count;
            waveDifficulty = dif;
            spawnTime = timer;
        }
    }
    float lastSpawn;


	void Awake () {
        endOfWaveEvent = null;
        OnWaveEnd = null;
        instance = this;
        surface = GetComponent<NavMeshSurface>();
        surface.BuildNavMesh();
	}
	
    void Start(){
        currentWave.amountOfEnemies = 2;
        currentWave.spawnTime = 4;
        currentWave.waveDifficulty = .2f;
        lastSpawn = Time.time;
        enemies = new List<AI>();
    }

    void Update(){
        if (SizeController.instance != null && SizeController.instance.transform.localScale.x > .2f) {
            if (Time.time > lastSpawn && currentWave.amountOfEnemies > 0) {
                lastSpawn += currentWave.spawnTime;
                StartCoroutine(PrepareSpawn());
            }
        }
    }

    public void NewWave(){
        completedWaves++;

        if(OnWaveEnd != null)
            OnWaveEnd(completedWaves);

        currentWave = new Wave(2 + completedWaves, .1f + (completedWaves * .1f / 2), currentWave.spawnTime - .1f);
    }

    public void BakeNavmesh(){
        surface.BuildNavMesh();
    }

    IEnumerator PrepareSpawn() {
        Vector3 finalPos =  Vector3.zero;
        while (finalPos == Vector3.zero || finalPos.x == Mathf.Infinity){
            if (SizeController.instance.transform.localScale.x < .2f)
                break;

            Vector3 rndDirection = Random.insideUnitSphere * 15;
            rndDirection += transform.position;
            NavMeshHit hit;
            NavMesh.SamplePosition(rndDirection, out hit, 10f, 1);
            finalPos = hit.position;
        }

        Instantiate(spawnParticle, finalPos, transform.rotation);
        yield return new WaitForSeconds(3f);
        CreateEnemy(finalPos);
    }

    void CreateEnemy(Vector3 pos){
        AI enemy = Instantiate(enemyToSpawn, pos, Quaternion.identity).GetComponent<AI>();

        float desiredHealth = 1;
        if(Random.Range(0f, 1f) < currentWave.waveDifficulty){
            desiredHealth++;
            if (Random.Range(0f, 1f) < currentWave.waveDifficulty) {
                desiredHealth++;
                if (Random.Range(0f, 1f) < currentWave.waveDifficulty) {
                    desiredHealth++;
                }
            }
        }

        enemy.health = desiredHealth;
        enemies.Add(enemy);
        currentWave.amountOfEnemies--;
    }

    public void KilledEnemy(AI enemy){
        enemies.Remove(enemy);
        if (currentWave.amountOfEnemies <= 0 && enemies.Count <= 0)
            if (endOfWaveEvent != null)
                endOfWaveEvent();
    }
}
