using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "YeniSahne", menuName = "Hikaye/Sahne")]
public class SahneVerisi : ScriptableObject
{
    [Header("Sahne Genel Ayarları")]
    public string sahneIsmi;
    public Sprite arkaplanGorseli;
    [TextArea(5, 10)] public string hikayeMetni;

    // --- HATAYI ÇÖZEN KISIM BURASI ---
    [Header("3. Seçenek İçin Gereksinim")]
    // Eğer buraya bir sahne adı yazarsan (Örn: "Magara_Giris"),
    // oyuncu o sahneye daha önce gitmişse 3. buton açılır.
    public string gerekliSahneIsmi; 
    // ---------------------------------

    [Header("Alternatif Sahne Sistemi")]
    // Eğer şartlar sağlanırsa oyun bu sahne yerine buradaki alternatifi açar.
    public SahneVerisi alternatifVersiyon; 
    
    // Alternatifin açılması için oyuncunun DAHA ÖNCE gitmiş olması gereken sahneler:
    public List<SahneVerisi> gerekliDigerSahneler; 

    [Header("Sahne Sonu Olayları")]
    public bool buBirOlumSahnesiMi;
    public string buSahnedeOlenKisi; 
    public string tanisilanKarakter;
    [TextArea] public string eklenecekBilgiNotu;
    public bool tekSecenekVar; 

    // --- SEÇENEKLER ---
    
    [Header("Seçenek 1 (Sol)")]
    public string secenek1Metni;
    public Sprite secenek1Resmi;
    public string secenek1SonucYazisi;
    public string s1KazanilacakEsya;
    public SahneVerisi secenek1Sonucu; // Sonraki Sahne
    public int s1EnerjiEtkisi;
    [Header("Seçenek 1 - Etkiler")]
    public int s1Zeka; public int s1Cesaret; public int s1Merhamet; public int s1Itaat;
    public string s1IliskiKarakteri; public int s1IliskiPuani;

    [Header("Seçenek 2 (Sağ)")]
    public string secenek2Metni;
    public Sprite secenek2Resmi;
    public string secenek2SonucYazisi;
    public string s2KazanilacakEsya;
    public SahneVerisi secenek2Sonucu; // Sonraki Sahne
    public int s2EnerjiEtkisi;
    [Header("Seçenek 2 - Etkiler")]
    public int s2Zeka; public int s2Cesaret; public int s2Merhamet; public int s2Itaat;
    public string s2IliskiKarakteri; public int s2IliskiPuani;

    [Header("Seçenek 3 (Orta/Gizli)")]
    public string secenek3Metni;
    public Sprite secenek3Resmi;
    public string secenek3SonucYazisi;
    public string s3KazanilacakEsya;
    public SahneVerisi secenek3Sonucu; // Sonraki Sahne
    public int s3EnerjiEtkisi;
    [Header("Seçenek 3 - Etkiler")]
    public int s3Zeka; public int s3Cesaret; public int s3Merhamet; public int s3Itaat;
    public string s3IliskiKarakteri; public int s3IliskiPuani;
}