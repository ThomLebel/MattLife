using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
	public Transform spawn;
	public GameObject monsterToSpawn;
	public int nbrToSpawn;
	public float delayBetweenSpawn;
	public List<GameObject> monsterList;
	public GameObject effect;
	public AudioSource spawnSound;

	[SerializeField]
	private int monsterSpawned = 0;
	[SerializeField]
	private bool spawning = false;

	[SerializeField]
	private float respawnDistance = 10f;
	private Camera cam;
	private float camHorizontalExtend;
	private float camVerticalExtend;

	private IEnumerator spawnCoroutine;

	// Start is called before the first frame update
	void Start()
	{
		cam = Camera.main;
		monsterList = new List<GameObject>();
		camHorizontalExtend = cam.orthographicSize * Screen.width / Screen.height;
	}

	private void Update()
	{
		//Allow spawner to spawn monsters again
		if (transform.position.x >= cam.transform.position.x + camHorizontalExtend + respawnDistance ||
			transform.position.x <= cam.transform.position.x - camHorizontalExtend - respawnDistance)
		{
			if (monsterList.Count <= 0)
			{
				spawning = false;
				monsterSpawned = 0;
				if(spawnCoroutine != null)
					StopCoroutine(spawnCoroutine);
			}
		}
	}

	private void OnTriggerExit2D(Collider2D collision)
	{
		if (!collision.CompareTag("Player"))
		{
			return;
		}

		if (collision.transform.position.x > transform.position.x && !spawning)
		{
			SpawnMonster();
		}
	}

	private void SpawnMonster()
	{
		if (monsterSpawned >= nbrToSpawn)
		{
			spawning = false;
			StopCoroutine(spawnCoroutine);
			return;
		}

		spawning = true;
		GameObject monster = Instantiate(monsterToSpawn, spawn.position, Quaternion.identity);
		//monster.GetComponent<EnemyScript>().spawner = this;
		monster.GetComponent<Enemy>().SetEnemySpawner(this);
		monsterList.Add(monster);
		monsterSpawned++;
		Instantiate(effect, spawn.position, Quaternion.identity);
		spawnSound.Play();

		spawnCoroutine = SpawnDelay();
		StartCoroutine(spawnCoroutine);
	}

	private IEnumerator SpawnDelay()
	{
		yield return new WaitForSeconds(delayBetweenSpawn);
		SpawnMonster();
	}

	private void DeleteMonster()
	{
		if (monsterList.Count <= 0)
		{
			return;
		}

		for (int i = monsterList.Count - 1; i >= 0; i--)
		{
			//monsterList[i].GetComponent<EnemyScript>().DeleteMonster();
			monsterList[i].GetComponent<Enemy>().DeleteMonster();
		}

		monsterList = new List<GameObject>();
	}
}
