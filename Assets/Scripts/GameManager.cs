using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;

// --- TİP TANIMLAMALARI ---

public enum GereksinimTipi 
{ 
    Esya, Iliski, Enerji, Unvan, OzelBayrak, DostSayisi, HerhangiBirKiz 
}

[System.Serializable]
public class KacisGereksinimi
{
    public string aciklama;
    public GereksinimTipi tip;
    public string hedefIsim;
    public int hedefDeger;
}

[System.Serializable]
public class KacisRotasi
{
    public string rotaIsmi;
    public Sprite rotaResmi;
    public List<KacisGereksinimi> gereksinimler;
}

[System.Serializable]
public class KarakterSayfasi
{
    public string isim;
    public Sprite portre; 
    [TextArea] public string temelHikaye; 
    public List<string> kazanilanBilgiler = new List<string>(); 
    public int iliskiPuani = 0;
    public bool kilidiAcildiMi = false; 
}

public class GameManager : MonoBehaviour
{
    [Header("Durum Takibi")]
    public List<string> olenKarakterler = new List<string>();

    [Header("UI Elemanları")]
    public Image arkaplanImg;
    public GameObject parsomenPanel;
    public TextMeshProUGUI hikayeText;
    public TextMeshProUGUI unvanText;

    [Header("Paneller")]
    public GameObject envanterPaneli;
    public GameObject sayfaEsyalar;
    public GameObject sayfaKacislar;
    public GameObject sayfaAnsiklopedi;
    public TextMeshProUGUI envanterListesiText;

    [Header("Kaçış Sistemi")]
    public Sprite soruIsaretiResmi;
    public GameObject kacisKartiPrefab;
    public Transform kacisListesiContainer;
    public List<KacisRotasi> kacisRotalari = new List<KacisRotasi>();
    public HashSet<string> ozelBayraklar = new HashSet<string>();

    [Header("Ansiklopedi")]
    public Image uiKarakterPortre;
    public TextMeshProUGUI uiKarakterIsim;
    public TextMeshProUGUI uiKarakterDetay;
    public Transform uiIliskiYuvarlaklariParent;
    public GameObject uiBosSayfaUyaris;
    public GameObject uiSayfaIcerigi;
    public List<KarakterSayfasi> karakterVeritabani = new List<KarakterSayfasi>();
    private int suankiSayfaIndex = 0;
    private List<KarakterSayfasi> acikKarakterlerListesi = new List<KarakterSayfasi>();

    [Header("Butonlar")]
    public GameObject layout2li;
    public GameObject layout3lu;

    // 2'li Düzen
    public Button buton1; public Image imgButon1; public TextMeshProUGUI buton1Text; public TextMeshProUGUI buton1Text_Sonuc;
    public Button buton2; public Image imgButon2; public TextMeshProUGUI buton2Text; public TextMeshProUGUI buton2Text_Sonuc;
    public Button butonOrta; public Image imgButonOrta; public TextMeshProUGUI butonOrtaText; public TextMeshProUGUI butonOrtaText_Sonuc;

    // 3'lü Düzen
    public Button btn3_Sol; public Image imgBtn3_Sol; public TextMeshProUGUI txtBtn3_Sol; public TextMeshProUGUI txtBtn3_Sol_Sonuc;
    public Button btn3_Orta; public Image imgBtn3_Orta; public TextMeshProUGUI txtBtn3_Orta; public TextMeshProUGUI txtBtn3_Orta_Sonuc;
    public Button btn3_Sag; public Image imgBtn3_Sag; public TextMeshProUGUI txtBtn3_Sag; public TextMeshProUGUI txtBtn3_Sag_Sonuc;

    [Header("Sistemler")]
    public GameObject[] enerjiSimgeleri;
    public SahneVerisi ilkSahne;
    public float gecisSuresi = 0.25f;
    public List<string> envanter = new List<string>();

    public int pZeka = 0; public int pCesaret = 0; public int pMerhamet = 0; public int pItaat = 0;
    private string mevcutUnvan = "Belirsiz";
    
    // Değişkenler
    private HashSet<string> ziyaretEdilenSahneler = new HashSet<string>();
    private SahneVerisi suankiSahne;
    private SahneVerisi bekleyenSonrakiSahne;
    private int mevcutEnerji = 4;
    private int bekleyenEnerjiDegisimi;
    private bool animasyonOynuyor = false;
    private bool sonucGosteriliyor = false;
    private List<Coroutine> calisanRoutineler = new List<Coroutine>();

    void Awake() 
    { 
        if (arkaplanImg != null) { arkaplanImg.canvasRenderer.SetAlpha(1.0f); arkaplanImg.color = Color.white; arkaplanImg.gameObject.SetActive(true); } 
    }

    void Start()
    {
        EnvanterPaneliniEkranaYay();
        VeritabaniSifirla();
        if (envanterPaneli != null) envanterPaneli.SetActive(false);
        EnerjiGorseliniGuncelle();
        UnvanHesapla();
        SahneYukle(ilkSahne, true);
    }

    // --- TEMEL FONKSİYONLAR ---

    public void KarakterOldur(string isim)
    {
        if (!string.IsNullOrEmpty(isim) && !olenKarakterler.Contains(isim))
            olenKarakterler.Add(isim);
    }

    void EnvanterPaneliniEkranaYay()
    {
        if (envanterPaneli == null) return;
        RectTransform rt = envanterPaneli.GetComponent<RectTransform>();
        if (rt != null)
        {
            rt.anchorMin = Vector2.zero; 
            rt.anchorMax = Vector2.one;  
            rt.offsetMin = Vector2.zero; 
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
        }
    }

    void VeritabaniSifirla()
    {
        ozelBayraklar.Clear();
        foreach (var k in karakterVeritabani) { k.kilidiAcildiMi = false; k.kazanilanBilgiler.Clear(); k.iliskiPuani = 0; }
    }

    public void OzelBayrakEkle(string bayrak)
    {
        string temizBayrak = bayrak.Trim();
        if (!ozelBayraklar.Contains(temizBayrak)) ozelBayraklar.Add(temizBayrak);
    }

    // --- ENERJİ SİSTEMİ ---
    void EnerjiDegistir(int m) 
    { 
        if (m == 0) return; 
        mevcutEnerji += m; 
        if (mevcutEnerji > 4) mevcutEnerji = 4; 
        if (mevcutEnerji < 0) mevcutEnerji = 0; 
        EnerjiGorseliniGuncelle(); 
    }

    void EnerjiGorseliniGuncelle() 
    { 
        if (enerjiSimgeleri == null) return;
        for (int i = 0; i < enerjiSimgeleri.Length; i++) 
            if (enerjiSimgeleri[i]) enerjiSimgeleri[i].SetActive(i < mevcutEnerji); 
    }

    // --- GEREKSİNİM KONTROLLERİ ---

    bool GereksinimKarsilaniyorMu(KacisGereksinimi g)
    {
        string aranan = g.hedefIsim.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR"));
        switch (g.tip)
        {
            case GereksinimTipi.Esya:
                foreach (string esya in envanter) { if (esya.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR")) == aranan) return true; }
                return false;
            case GereksinimTipi.Iliski:
                KarakterSayfasi k = karakterVeritabani.Find(x => x.isim.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR")) == aranan);
                return (k != null && k.iliskiPuani >= g.hedefDeger);
            case GereksinimTipi.Enerji: return mevcutEnerji >= g.hedefDeger;
            case GereksinimTipi.Unvan: return mevcutUnvan.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR")) == aranan;
            case GereksinimTipi.OzelBayrak:
                // DÜZELTME: 'a' yerine 'aranan' yazıldı.
                foreach (string b in ozelBayraklar) { if (b.Trim().ToLower(new System.Globalization.CultureInfo("tr-TR")) == aranan) return true; }
                return false;
            case GereksinimTipi.DostSayisi:
                int dost = 0; foreach (var kar in karakterVeritabani) { if (kar.iliskiPuani >= 60) dost++; }
                return dost >= g.hedefDeger;
            case GereksinimTipi.HerhangiBirKiz:
                KarakterSayfasi lily = karakterVeritabani.Find(x => x.isim.Contains("Lily"));
                KarakterSayfasi linda = karakterVeritabani.Find(x => x.isim.Contains("Linda"));
                if (lily != null && lily.iliskiPuani >= g.hedefDeger) return true;
                if (linda != null && linda.iliskiPuani >= g.hedefDeger) return true;
                return false;
        }
        return false;
    }

    bool KacisHalaMumkunMu(KacisRotasi rota)
    {
        foreach (var gereksinim in rota.gereksinimler)
        {
            if (gereksinim.tip == GereksinimTipi.Iliski || gereksinim.tip == GereksinimTipi.HerhangiBirKiz)
            {
                if (olenKarakterler.Contains(gereksinim.hedefIsim)) return false;
            }
            if (!GereksinimKarsilaniyorMu(gereksinim)) return false;
        }
        return true;
    }

    // --- LİSTELEME SİSTEMİ ---

    public void KacislariListele()
    {
        if (kacisListesiContainer == null) return;

        for (int i = 0; i < kacisRotalari.Count; i++)
        {
            if (i >= kacisListesiContainer.childCount) break;

            Transform mevcutKart = kacisListesiContainer.GetChild(i);
            KacisRotasi rota = kacisRotalari[i];

            if (!KacisHalaMumkunMu(rota))
            {
                mevcutKart.gameObject.SetActive(false);
                continue;
            }

            mevcutKart.gameObject.SetActive(true);

            Transform rotaResmiObj = mevcutKart.transform.Find("RotaResmi");
            if (rotaResmiObj != null)
            {
                Image img = rotaResmiObj.GetComponent<Image>();
                img.sprite = rota.rotaResmi;
                img.gameObject.SetActive(true);
                img.preserveAspect = false;
            }

            Transform soruIsaretiObj = mevcutKart.transform.Find("SoruIsareti");
            if (soruIsaretiObj != null)
            {
                soruIsaretiObj.GetComponent<Image>().preserveAspect = false;
            }

            Transform gereksinimlerParent = mevcutKart.transform.Find("Gereksinimler");
            int tamamlananSayisi = 0;

            if (gereksinimlerParent != null)
            {
                for (int j = 0; j < 3; j++) 
                {
                    if (j < gereksinimlerParent.childCount) 
                    {
                        TextMeshProUGUI txt = gereksinimlerParent.GetChild(j).GetComponent<TextMeshProUGUI>();
                        if (j < rota.gereksinimler.Count) {
                            KacisGereksinimi g = rota.gereksinimler[j];
                            txt.gameObject.SetActive(true);
                            if (GereksinimKarsilaniyorMu(g)) {
                                txt.text = $"<color=green>V</color>"; 
                                tamamlananSayisi++;
                            } else {
                                txt.text = $"<color=red>X</color>"; 
                            }
                        } else { txt.text = ""; } 
                    }
                }
            }

            if (soruIsaretiObj != null)
            {
                soruIsaretiObj.gameObject.SetActive(tamamlananSayisi < 3);
            }
        }
    }

    // --- SAHNE YÖNETİMİ ---

    public void SahneYukle(SahneVerisi gelenSahne, bool ilkAcilisMi = false) 
    { 
        if (gelenSahne == null) return; 
        
        // Sadece Alternatif Sahne kontrolü yapıyoruz.
        SahneVerisi yuklenecekSahne = ZiyaretKontrol(gelenSahne); 
        
        TumRoutineleriDurdur(); 

        if (ilkAcilisMi) 
        { 
            SahneVerileriniUygula(yuklenecekSahne); 
            BaslatCoroutine(ParsomenIslemi(true)); 
        } 
        else 
        { 
            BaslatCoroutine(SahneGecisSiralama(yuklenecekSahne)); 
        } 
    }

    SahneVerisi ZiyaretKontrol(SahneVerisi orjinalSahne) 
    { 
        // 1. Gidilen sahneyi kaydet
        if (!ziyaretEdilenSahneler.Contains(orjinalSahne.name)) 
            ziyaretEdilenSahneler.Add(orjinalSahne.name); 
        
        // 2. Alternatif var mı?
        if (orjinalSahne.alternatifVersiyon != null) 
        { 
            bool sartlarTamam = true; 
            if (orjinalSahne.gerekliDigerSahneler != null && orjinalSahne.gerekliDigerSahneler.Count > 0) 
            { 
                foreach (var gerekliSahne in orjinalSahne.gerekliDigerSahneler) 
                { 
                    if (!ziyaretEdilenSahneler.Contains(gerekliSahne.name)) 
                    { 
                        sartlarTamam = false; break; 
                    } 
                } 
            } 
            else 
            { 
                sartlarTamam = false; 
            } 
            
            if (sartlarTamam) return orjinalSahne.alternatifVersiyon; 
        } 
        return orjinalSahne; 
    }

    IEnumerator SahneGecisSiralama(SahneVerisi yeniSahne) 
    { 
        animasyonOynuyor = true; 
        if (layout3lu.activeSelf) StartCoroutine(KartlariTopluCevir(true, true)); 
        else StartCoroutine(KartlariTopluCevir(true, false)); 
        
        yield return StartCoroutine(ParsomenIslemi(false)); 
        
        bool resimDegismeli = (arkaplanImg.sprite != yeniSahne.arkaplanGorseli); 
        if (resimDegismeli) 
        { 
            float fadeHiz = 1.0f / gecisSuresi; 
            for (float t = 1; t > 0; t -= Time.deltaTime * fadeHiz) { arkaplanImg.color = new Color(1, 1, 1, t); yield return null; } 
            arkaplanImg.color = new Color(1, 1, 1, 0); 
            SahneVerileriniUygula(yeniSahne); 
            KartlarinAcisiniSifirla(90); 
            for (float t = 0; t < 1; t += Time.deltaTime * fadeHiz) { arkaplanImg.color = new Color(1, 1, 1, t); yield return null; } 
            arkaplanImg.color = new Color(1, 1, 1, 1); 
        } 
        else 
        { 
            SahneVerileriniUygula(yeniSahne); 
            KartlarinAcisiniSifirla(90); 
            yield return new WaitForSeconds(0.05f); 
        } 
        
        if (layout3lu.activeSelf) StartCoroutine(KartlariTopluCevir(false, true)); 
        else StartCoroutine(KartlariTopluCevir(false, false)); 
        
        yield return StartCoroutine(ParsomenIslemi(true)); 
        animasyonOynuyor = false; 
    }

    void SahneVerileriniUygula(SahneVerisi sahne)
    {
        suankiSahne = sahne;
        sonucGosteriliyor = false;
        
        if (arkaplanImg != null) arkaplanImg.sprite = suankiSahne.arkaplanGorseli;
        if (hikayeText) hikayeText.text = suankiSahne.hikayeMetni;
        
        AnsiklopediGuncelle(suankiSahne.tanisilanKarakter, suankiSahne.eklenecekBilgiNotu);
        
        // --- 3. SEÇENEK KONTROLÜ (DÜZELTİLDİ) ---
        // 'gerekliSahneIsmi' doluysa VE oyuncu o sahneye daha önce gittiyse 3. seçenek açılır.
        bool ucuncuAcikMi = (!string.IsNullOrEmpty(suankiSahne.gerekliSahneIsmi) && ziyaretEdilenSahneler.Contains(suankiSahne.gerekliSahneIsmi));
        
        ButonlariResetle();
        
        if (ucuncuAcikMi)
        {
            layout2li.SetActive(false);
            layout3lu.SetActive(true);
            AyarlaButon(btn3_Sol, txtBtn3_Sol, txtBtn3_Sol_Sonuc, imgBtn3_Sol, suankiSahne.secenek1Metni, suankiSahne.secenek1Resmi);
            AyarlaButon(btn3_Sag, txtBtn3_Sag, txtBtn3_Sag_Sonuc, imgBtn3_Sag, suankiSahne.secenek2Metni, suankiSahne.secenek2Resmi);
            AyarlaButon(btn3_Orta, txtBtn3_Orta, txtBtn3_Orta_Sonuc, imgBtn3_Orta, suankiSahne.secenek3Metni, suankiSahne.secenek3Resmi);
        }
        else
        {
            layout3lu.SetActive(false);
            layout2li.SetActive(true);
            if (suankiSahne.tekSecenekVar)
            {
                buton1.gameObject.SetActive(false);
                buton2.gameObject.SetActive(false);
                AyarlaButon(butonOrta, butonOrtaText, butonOrtaText_Sonuc, imgButonOrta, suankiSahne.secenek1Metni, suankiSahne.secenek1Resmi);
            }
            else
            {
                butonOrta.gameObject.SetActive(false);
                AyarlaButon(buton1, buton1Text, buton1Text_Sonuc, imgButon1, suankiSahne.secenek1Metni, suankiSahne.secenek1Resmi);
                AyarlaButon(buton2, buton2Text, buton2Text_Sonuc, imgButon2, suankiSahne.secenek2Metni, suankiSahne.secenek2Resmi);
            }
        }
        if (suankiSahne.buBirOlumSahnesiMi || mevcutEnerji <= 0) { Debug.Log("OYUN BİTTİ!"); animasyonOynuyor = true; }
    }

    // --- BUTON FONKSİYONLARI ---

    public void Mod2_SolBasildi()
    {
        if (IslemYapabilirMiyim()) return; ButonlariKilitle();
        SahneSonuIslemleri(); 
        EsyaVer(suankiSahne.s1KazanilacakEsya); 
        TraitGuncelle(suankiSahne.s1Zeka, suankiSahne.s1Cesaret, suankiSahne.s1Merhamet, suankiSahne.s1Itaat); 
        IliskiDegistir(suankiSahne.s1IliskiKarakteri, suankiSahne.s1IliskiPuani); 
        SetSonuc(suankiSahne.secenek1Sonucu, suankiSahne.s1EnerjiEtkisi); 
        BaslatCoroutine(AktifKartCevir(buton1, buton1Text, buton1Text_Sonuc, imgButon1, suankiSahne.secenek1SonucYazisi)); 
        BaslatCoroutine(PasifKartYokEt(buton2));
    }
    public void Mod2_SagBasildi()
    {
        if (IslemYapabilirMiyim()) return; ButonlariKilitle();
        SahneSonuIslemleri(); 
        EsyaVer(suankiSahne.s2KazanilacakEsya); 
        TraitGuncelle(suankiSahne.s2Zeka, suankiSahne.s2Cesaret, suankiSahne.s2Merhamet, suankiSahne.s2Itaat); 
        IliskiDegistir(suankiSahne.s2IliskiKarakteri, suankiSahne.s2IliskiPuani); 
        SetSonuc(suankiSahne.secenek2Sonucu, suankiSahne.s2EnerjiEtkisi); 
        BaslatCoroutine(AktifKartCevir(buton2, buton2Text, buton2Text_Sonuc, imgButon2, suankiSahne.secenek2SonucYazisi)); 
        BaslatCoroutine(PasifKartYokEt(buton1));
    }
    public void Mod2_OrtaBasildi()
    {
        if (IslemYapabilirMiyim()) return; ButonlariKilitle();
        SahneSonuIslemleri(); 
        EsyaVer(suankiSahne.s1KazanilacakEsya); 
        TraitGuncelle(suankiSahne.s1Zeka, suankiSahne.s1Cesaret, suankiSahne.s1Merhamet, suankiSahne.s1Itaat); 
        IliskiDegistir(suankiSahne.s1IliskiKarakteri, suankiSahne.s1IliskiPuani); 
        SetSonuc(suankiSahne.secenek1Sonucu, suankiSahne.s1EnerjiEtkisi); 
        BaslatCoroutine(AktifKartCevir(butonOrta, butonOrtaText, butonOrtaText_Sonuc, imgButonOrta, suankiSahne.secenek1SonucYazisi));
    }
    public void Mod3_SolBasildi()
    {
        if (IslemYapabilirMiyim()) return; ButonlariKilitle();
        SahneSonuIslemleri(); 
        EsyaVer(suankiSahne.s1KazanilacakEsya); 
        TraitGuncelle(suankiSahne.s1Zeka, suankiSahne.s1Cesaret, suankiSahne.s1Merhamet, suankiSahne.s1Itaat); 
        IliskiDegistir(suankiSahne.s1IliskiKarakteri, suankiSahne.s1IliskiPuani); 
        SetSonuc(suankiSahne.secenek1Sonucu, suankiSahne.s1EnerjiEtkisi); 
        BaslatCoroutine(AktifKartCevir(btn3_Sol, txtBtn3_Sol, txtBtn3_Sol_Sonuc, imgBtn3_Sol, suankiSahne.secenek1SonucYazisi)); 
        BaslatCoroutine(PasifKartYokEt(btn3_Orta)); 
        BaslatCoroutine(PasifKartYokEt(btn3_Sag));
    }
    public void Mod3_SagBasildi()
    {
        if (IslemYapabilirMiyim()) return; ButonlariKilitle();
        SahneSonuIslemleri(); 
        EsyaVer(suankiSahne.s2KazanilacakEsya); 
        TraitGuncelle(suankiSahne.s2Zeka, suankiSahne.s2Cesaret, suankiSahne.s2Merhamet, suankiSahne.s2Itaat); 
        IliskiDegistir(suankiSahne.s2IliskiKarakteri, suankiSahne.s2IliskiPuani); 
        SetSonuc(suankiSahne.secenek2Sonucu, suankiSahne.s2EnerjiEtkisi); 
        BaslatCoroutine(AktifKartCevir(btn3_Sag, txtBtn3_Sag, txtBtn3_Sag_Sonuc, imgBtn3_Sag, suankiSahne.secenek2SonucYazisi)); 
        BaslatCoroutine(PasifKartYokEt(btn3_Sol)); 
        BaslatCoroutine(PasifKartYokEt(btn3_Orta));
    }
    public void Mod3_GizliBasildi()
    {
        if (IslemYapabilirMiyim()) return; ButonlariKilitle();
        SahneSonuIslemleri(); 
        EsyaVer(suankiSahne.s3KazanilacakEsya); 
        TraitGuncelle(suankiSahne.s3Zeka, suankiSahne.s3Cesaret, suankiSahne.s3Merhamet, suankiSahne.s3Itaat); 
        IliskiDegistir(suankiSahne.s3IliskiKarakteri, suankiSahne.s3IliskiPuani); 
        SetSonuc(suankiSahne.secenek3Sonucu, suankiSahne.s3EnerjiEtkisi); 
        BaslatCoroutine(AktifKartCevir(btn3_Orta, txtBtn3_Orta, txtBtn3_Orta_Sonuc, imgBtn3_Orta, suankiSahne.secenek3SonucYazisi)); 
        BaslatCoroutine(PasifKartYokEt(btn3_Sol)); 
        BaslatCoroutine(PasifKartYokEt(btn3_Sag));
    }

    void SahneSonuIslemleri()
    {
        if (suankiSahne != null && !string.IsNullOrEmpty(suankiSahne.buSahnedeOlenKisi))
        {
            KarakterOldur(suankiSahne.buSahnedeOlenKisi);
        }
    }

    // --- YARDIMCI VE UI FONKSİYONLARI ---

    void AnsiklopediGuncelle(string karakterAdi, string not) { if (string.IsNullOrEmpty(karakterAdi)) return; KarakterSayfasi hedefKarakter = karakterVeritabani.Find(k => k.isim == karakterAdi); if (hedefKarakter != null) { if (!hedefKarakter.kilidiAcildiMi) hedefKarakter.kilidiAcildiMi = true; if (!string.IsNullOrEmpty(not) && !hedefKarakter.kazanilanBilgiler.Contains(not)) hedefKarakter.kazanilanBilgiler.Add(not); } }
    public void IliskiDegistir(string karakterAdi, int miktar) { if (string.IsNullOrEmpty(karakterAdi)) return; KarakterSayfasi k = karakterVeritabani.Find(x => x.isim == karakterAdi); if (k != null) { k.iliskiPuani += miktar; if (k.iliskiPuani > 100) k.iliskiPuani = 100; if (k.iliskiPuani < 0) k.iliskiPuani = 0; } }
    public void SayfaIleri() { if (acikKarakterlerListesi.Count == 0) return; suankiSayfaIndex++; if (suankiSayfaIndex >= acikKarakterlerListesi.Count) suankiSayfaIndex = 0; SayfayiGoster(suankiSayfaIndex); }
    public void SayfaGeri() { if (acikKarakterlerListesi.Count == 0) return; suankiSayfaIndex--; if (suankiSayfaIndex < 0) suankiSayfaIndex = acikKarakterlerListesi.Count - 1; SayfayiGoster(suankiSayfaIndex); }
    void SayfayiGoster(int index) { uiBosSayfaUyaris.SetActive(false); uiSayfaIcerigi.SetActive(true); KarakterSayfasi k = acikKarakterlerListesi[index]; uiKarakterIsim.text = k.isim; if (uiKarakterPortre != null) uiKarakterPortre.sprite = k.portre; string tamMetin = "<b>Hikaye:</b>\n" + k.temelHikaye + "\n\n"; if (k.kazanilanBilgiler.Count > 0) { tamMetin += "<b>Notlar:</b>\n"; foreach (string not in k.kazanilanBilgiler) { tamMetin += "- " + not + "\n"; } } uiKarakterDetay.text = tamMetin; if (uiIliskiYuvarlaklariParent != null) { int doluSayisi = k.iliskiPuani / 10; for (int i = 0; i < 10; i++) { if (i >= uiIliskiYuvarlaklariParent.childCount) break; GameObject dolu = uiIliskiYuvarlaklariParent.GetChild(i).GetChild(0).gameObject; dolu.SetActive(i < doluSayisi); } } }
    public void EnvanterAcKapat() { bool d = envanterPaneli.activeSelf; envanterPaneli.SetActive(!d); if (!d) SekmeAc_Esyalar(); }
    public void SekmeAc_Esyalar() { PanelAyarla(true, false, false, false); EnvanteriListele(); }
    public void SekmeAc_Kacislar() { PanelAyarla(false, true, false, false); KacislariListele(); }
    public void SekmeAc_Ansiklopedi() { PanelAyarla(false, false, true, true); acikKarakterlerListesi = karakterVeritabani.FindAll(k => k.kilidiAcildiMi); if (acikKarakterlerListesi.Count > 0) { if (suankiSayfaIndex >= acikKarakterlerListesi.Count) suankiSayfaIndex = 0; SayfayiGoster(suankiSayfaIndex); } else { uiBosSayfaUyaris.SetActive(true); uiSayfaIcerigi.SetActive(false); } }
    void PanelAyarla(bool esya, bool kacis, bool ansiklopedi, bool ansIcerik) { if (sayfaEsyalar) sayfaEsyalar.SetActive(esya); if (sayfaKacislar) sayfaKacislar.SetActive(kacis); if (sayfaAnsiklopedi) sayfaAnsiklopedi.SetActive(ansiklopedi); }
    void EnvanteriListele() { if (envanterListesiText == null) return; if (envanter.Count == 0) { envanterListesiText.text = "Çantan boş."; return; } string t = ""; foreach (string e in envanter) { t += "- " + e + "\n"; } envanterListesiText.text = t; }

    void BaslatCoroutine(IEnumerator routine) { calisanRoutineler.Add(StartCoroutine(routine)); }
    void TumRoutineleriDurdur() { foreach (var r in calisanRoutineler) if (r != null) StopCoroutine(r); calisanRoutineler.Clear(); }
    void TraitGuncelle(int zeka, int cesaret, int merhamet, int itaat) { pZeka += zeka; pCesaret += cesaret; pMerhamet += merhamet; pItaat += itaat; UnvanHesapla(); }
    void UnvanHesapla() { if (pZeka + pCesaret > 5) mevcutUnvan = "Kıvılcım"; else if (pMerhamet + pItaat > 5) mevcutUnvan = "Melek"; else if (pZeka + pItaat > 5) mevcutUnvan = "Gölge"; else if (pCesaret + pMerhamet > 5) mevcutUnvan = "Lider"; else if (pZeka > 5) mevcutUnvan = "Kurnaz"; else if (pCesaret > 5) mevcutUnvan = "Bela"; else if (pMerhamet > 5) mevcutUnvan = "Aziz"; else if (pItaat > 5) mevcutUnvan = "Survivor"; else mevcutUnvan = "Yolcu"; if (unvanText != null) unvanText.text = "Ünvan: " + mevcutUnvan; }
    void AyarlaButon(Button btn, TextMeshProUGUI baslikTxt, TextMeshProUGUI sonucTxt, Image img, string yazi, Sprite resim) { btn.gameObject.SetActive(true); if (baslikTxt != null) { baslikTxt.text = yazi; baslikTxt.gameObject.SetActive(true); } if (sonucTxt != null) sonucTxt.gameObject.SetActive(false); if (img != null) { if (resim != null) { img.sprite = resim; img.gameObject.SetActive(true); } else img.gameObject.SetActive(false); } }
    void EsyaVer(string esyaAdi) { if (string.IsNullOrWhiteSpace(esyaAdi)) return; string temizIsim = esyaAdi.Trim(); if (temizIsim.StartsWith("Bayrak:")) { OzelBayrakEkle(temizIsim.Replace("Bayrak:", "")); return; } if (!envanter.Contains(temizIsim)) { envanter.Add(temizIsim); Debug.Log("Eşya Kazanıldı: " + temizIsim); if (envanterListesiText != null && envanterPaneli.activeSelf) EnvanteriListele(); } }
    bool IslemYapabilirMiyim() { if (sonucGosteriliyor) { EnerjiDegistir(bekleyenEnerjiDegisimi); SahneYukle(bekleyenSonrakiSahne); return true; } if (animasyonOynuyor) return true; animasyonOynuyor = true; return false; }
    void SetSonuc(SahneVerisi sahne, int enerji) { bekleyenSonrakiSahne = sahne; bekleyenEnerjiDegisimi = enerji; }
    void ButonlariKilitle() { if (buton1) buton1.interactable = false; if (buton2) buton2.interactable = false; if (butonOrta) butonOrta.interactable = false; if (btn3_Sol) btn3_Sol.interactable = false; if (btn3_Orta) btn3_Orta.interactable = false; if (btn3_Sag) btn3_Sag.interactable = false; }
    void ButonlariResetle() { ResetTek(buton1, buton1Text); ResetTek(buton2, buton2Text); ResetTek(butonOrta, butonOrtaText); ResetTek(btn3_Sol, txtBtn3_Sol); ResetTek(btn3_Orta, txtBtn3_Orta); ResetTek(btn3_Sag, txtBtn3_Sag); }
    void ResetTek(Button b, TextMeshProUGUI t) { if (b) { b.interactable = true; b.gameObject.SetActive(true); b.transform.rotation = Quaternion.identity; if (t) { t.alpha = 1f; t.gameObject.SetActive(true); } } }
    void KartlarinAcisiniSifirla(float aci) { Quaternion rot = Quaternion.Euler(0, aci, 0); if (layout2li.activeSelf) { buton1.transform.rotation = rot; buton2.transform.rotation = rot; butonOrta.transform.rotation = rot; } if (layout3lu.activeSelf) { btn3_Sol.transform.rotation = rot; btn3_Orta.transform.rotation = rot; btn3_Sag.transform.rotation = rot; } }
    IEnumerator PasifKartYokEt(Button btn) { float speed = gecisSuresi; for (float t = 0; t < 1; t += Time.deltaTime / speed) { btn.transform.rotation = Quaternion.Euler(0, Mathf.Lerp(0, 90, t), 0); yield return null; } btn.transform.rotation = Quaternion.Euler(0, 90, 0); btn.gameObject.SetActive(false); }
    IEnumerator ParsomenIslemi(bool ac) { if (parsomenPanel) { Vector3 s = ac ? new Vector3(1f, 0f, 1f) : Vector3.one; Vector3 e = ac ? Vector3.one : new Vector3(1f, 0f, 1f); for (float t = 0; t < 1; t += Time.deltaTime / gecisSuresi) { parsomenPanel.transform.localScale = Vector3.Lerp(s, e, t); yield return null; } parsomenPanel.transform.localScale = e; } }

    IEnumerator KartlariTopluCevir(bool kapat, bool mod3Mu) 
    { 
        float s = kapat ? 0 : 90; float e = kapat ? 90 : 0; 
        for (float t = 0; t < 1; t += Time.deltaTime / gecisSuresi) 
        { 
            float aci = Mathf.Lerp(s, e, t); Quaternion r = Quaternion.Euler(0, aci, 0); 
            if (mod3Mu) 
            { 
                if (btn3_Sol.gameObject.activeSelf) btn3_Sol.transform.rotation = r; 
                if (btn3_Orta.gameObject.activeSelf) btn3_Orta.transform.rotation = r; 
                if (btn3_Sag.gameObject.activeSelf) btn3_Sag.transform.rotation = r; 
            } 
            else 
            { 
                if (buton1.gameObject.activeSelf) buton1.transform.rotation = r; 
                if (buton2.gameObject.activeSelf) buton2.transform.rotation = r; 
                if (butonOrta.gameObject.activeSelf) butonOrta.transform.rotation = r; 
            } 
            yield return null; 
        } 
    }

    IEnumerator AktifKartCevir(Button btn, TextMeshProUGUI baslikTxt, TextMeshProUGUI sonucTxt, Image ikon, string sonuc)
    {
        float speed = gecisSuresi;
        for (float t = 0; t < 1; t += Time.deltaTime / speed) { btn.transform.rotation = Quaternion.Euler(0, Mathf.Lerp(0, 90, t), 0); yield return null; }
        btn.transform.rotation = Quaternion.Euler(0, 90, 0);
        if (ikon != null) ikon.gameObject.SetActive(false); 
        if (baslikTxt != null) baslikTxt.gameObject.SetActive(false); 
        if (sonucTxt != null) { sonucTxt.text = sonuc; sonucTxt.gameObject.SetActive(true); }
        for (float t = 0; t < 1; t += Time.deltaTime / speed) { btn.transform.rotation = Quaternion.Euler(0, Mathf.Lerp(90, 0, t), 0); yield return null; }
        btn.transform.rotation = Quaternion.identity; animasyonOynuyor = false; sonucGosteriliyor = true; btn.interactable = true;
    }
}