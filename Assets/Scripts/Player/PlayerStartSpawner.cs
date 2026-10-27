using UnityEngine;
[RequireComponent(typeof(Rigidbody))]
public class PlayerStartSpawner : MonoBehaviour
{
    [Header("Başlangıç Noktaları")]
    [SerializeField] private Transform spawnA;
    [SerializeField] private Transform spawnB;
    [SerializeField] private Transform spawnC;
    private const string LastSpawnKey = "PZ_LastPlayerSpawn";
    private void Awake()
    {
        Transform[] spawns = { spawnA, spawnB, spawnC };
        if (spawnA == null || spawnB == null || spawnC == null)
        {
            Debug.LogError("[PLAYER SPAWN] Başlangıç noktaları eksik!");
            return;
        }
        int previousIndex = PlayerPrefs.GetInt(LastSpawnKey, -1);
        int selectedIndex;
        if (previousIndex >= 0 && previousIndex < spawns.Length)
        {
            selectedIndex = Random.Range(0, spawns.Length - 1);
            if (selectedIndex >= previousIndex)
            {
                selectedIndex++;
            }
        }
        else
        {
            selectedIndex = Random.Range(0, spawns.Length);
        }
        Transform selectedSpawn = spawns[selectedIndex];
        Rigidbody rb = GetComponent<Rigidbody>();
        transform.position = selectedSpawn.position;
        rb.position = selectedSpawn.position;
        rb.linearVelocity = Vector3.zero;
        rb.angularVelocity = Vector3.zero;
        PlayerPrefs.SetInt(LastSpawnKey, selectedIndex);
        PlayerPrefs.Save();
        Debug.Log("[PLAYER SPAWN] Başlangıç: " + selectedSpawn.name + " | Konum: " + selectedSpawn.position);
    }
}
