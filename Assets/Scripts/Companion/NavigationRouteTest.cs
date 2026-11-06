using System.Collections;
using UnityEngine;
using UnityEngine.AI;
public class NavigationRouteTest : MonoBehaviour
{
    [Header("Referanslar")]
    [SerializeField] private NavMeshAgent companion;
    [SerializeField] private Transform player;
    [SerializeField] private Transform spawnA;
    [SerializeField] private Transform spawnB;
    [SerializeField] private Transform spawnC;
    [Header("Test Ayarları")]
    [SerializeField] private int tekrarSayisi = 3;
    [SerializeField] private float maksimumSure = 25f;
    [SerializeField] private float ulasmaMesafesi = 2.2f;
    private IEnumerator Start()
    {
        if (companion == null || player == null || spawnA == null || spawnB == null || spawnC == null)
        {
            Debug.LogError("[NAV TEST] Referanslar eksik!");
            yield break;
        }
        Transform[] baslangiclar = { spawnA, spawnB, spawnC };
        int basarili = 0;
        int toplam = 0;
        float toplamSure = 0f;
        yield return new WaitForSeconds(0.5f);
        foreach(Transform baslangic in baslangiclar)
        {
            for(int i = 1; i <= tekrarSayisi; i++)
            {
                toplam++;
                if(!NavMesh.SamplePosition(baslangic.position,out NavMeshHit hit, 2f, companion.areaMask))
                {
                    Debug.LogWarning($"[NAV TEST] {baslangic.name} | NavMesh bulunamadı.");
                    continue;
                }

                if (!companion.isOnNavMesh)
                {
                    Debug.LogWarning("[NAV TEST] Companion NavMesh üzerinde değil.");
                    continue;
                }
                companion.ResetPath();

                if (!companion.Warp(hit.position))
                {
                    Debug.LogWarning($"[NAV TEST] {baslangic.name} | Warp başarısız.");
                    continue;
                }
                yield return null;
                if(!NavMesh.SamplePosition(player.position, out NavMeshHit hedefHit, 3f, companion.areaMask))
                {
                    Debug.LogWarning($"[NAV TEST] {baslangic.name} | " + "Player yakınında NavMesh bulunamadı!");
                    continue;
                }
                NavMeshPath rota = new NavMeshPath();
                bool rotaBulundu = companion.CalculatePath(hedefHit.position, rota);
                Debug.Log(
                    $"[NAV TEST KONTROL] {baslangic.name} | " +
                    $"Başlangıç: {companion.transform.position} | " +
                    $"Hedef: {hedefHit.position} | " +
                    $"Rota: {rota.status}"
                    );

                if (!rotaBulundu||rota.status!=NavMeshPathStatus.PathComplete)
                {
                    Debug.LogWarning($"[NAV TEST] {baslangic.name} | Rota: {rota.status}");
                    continue;
                }
                float rotaUzunlugu = 0f;
                for(int k = 1; k < rota.corners.Length; k++)
                {
                    rotaUzunlugu += Vector3.Distance(rota.corners[k - 1], rota.corners[k]);
                }
                companion.isStopped = false;
                if (!companion.SetDestination(hedefHit.position))
                {
                    Debug.LogWarning($"[NAV TEST] {baslangic.name} | Hedef atanamadı.");
                    continue;
                }
                float baslangicZamani = Time.time;
                while(Time.time - baslangicZamani < maksimumSure && Vector3.Distance
                    (companion.transform.position, player.position) > ulasmaMesafesi)
                {
                    yield return null;
                }
                float sure = Time.time - baslangicZamani;
                bool sonuc = Vector3.Distance(companion.transform.position, player.position) <= ulasmaMesafesi;
                if (sonuc)
                {
                    basarili++;
                    toplamSure += sure;
                }
                Debug.Log(
                    $"[NAV TEST] {baslangic.name} | " +
                    $"Deneme: {i} | " +
                    $"Rota: {rota.status} | " +
                    $"Uzunluk: {rotaUzunlugu:F2} m | " +
                    $"Sure: {sure:F2} sn | " +
                    $"Sonuc: {(sonuc ? "BAŞARILI" : "BAŞARISIZ")}"
                    );
                yield return new WaitForSeconds(0.5f);
            }
        }
        float basariOrani = toplam > 0
            ? (float)basarili / toplam * 100f
            : 0f;
        float ortalamaSure = basarili > 0
            ? toplamSure / basarili
            : 0f;
        Debug.Log(
             $"[NAV TEST ÖZET]" +
             $"Başarı: {basarili}/{toplam} | " +
             $"Başarı Oranı: %{basariOrani:F1} | " +
             $"Ortalama süre: {ortalamaSure:F2} sn"
            );
    }
}
