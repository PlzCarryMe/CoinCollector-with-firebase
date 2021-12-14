using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    // Fungsi [Range (min, max)] ialah menjaga value agar tetap berada di antara min dan max-nya 
    [Range(0f, 1f)]
    public float AutoCollectPercentage = 0.1f;
    public ResourceConfig[] ResourcesConfigs;
    public Sprite[] ResourcesSprites;

    public Transform ResourcesParent;
    public ResourceController ResourcePrefab;
    public Taptext TapTextPrefab;

    public Transform CoinIcon;
    public Text GoldInfo;
    public Text AutoCollectInfo;

    private List<ResourceController> _activeResources = new List<ResourceController>();
    private List<Taptext> _tapTextPool = new List<Taptext>();
    private float _collectSecond;

    private static int counter = 1;

    //public double TotalGold { get; private set; }

    public float SaveDelay = 5f;
    private float _saveDelayCounter;

    private static GameManager _instance = null;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();

            }
            return _instance;

        }

    }

    // Start is called before the first frame update
    void Start()
    {
        AddAllResources();
        GoldInfo.text = $"Gold: { UserDataManager.Progress.Gold.ToString("0") }";
    }

    // Update is called once per frame
    void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        _saveDelayCounter -= deltaTime;

        // Fungsi untuk selalu mengeksekusi CollectPerSecond setiap detik 
        _collectSecond += deltaTime;
        if (_collectSecond >= 1f)
        {
            CollectPerSecond();
            _collectSecond = 0f;

        }
        CheckResourceCost();

        CoinIcon.transform.localScale = Vector3.LerpUnclamped(CoinIcon.transform.localScale, Vector3.one * 2f, 0.15f);
        CoinIcon.transform.Rotate(0f, 0f, Time.deltaTime * -100f);

    }

    private void CheckResourceCost()
    {
        foreach (ResourceController resource in _activeResources)
        {
            bool isBuyable = false;

            if (resource.IsUnlocked)
            {
                isBuyable = UserDataManager.Progress.Gold >= resource.GetUpgradeCost();
            }
            else
            {
                isBuyable = UserDataManager.Progress.Gold >= resource.GetUnlockCost();
            }

            resource.ResourceImage.sprite = ResourcesSprites[isBuyable ? 1 : 0];
        }

    }

    private void AddAllResources()
    {
        bool showResources = true;
        int index = 0;

        foreach (ResourceConfig config in ResourcesConfigs)
        {
            GameObject obj = Instantiate(ResourcePrefab.gameObject, ResourcesParent, false);
            ResourceController resource = obj.GetComponent<ResourceController>();

            resource.SetConfig(index, config);
            obj.gameObject.SetActive(showResources);

            if (showResources && !resource.IsUnlocked)
            {
                showResources = false;
            }

            _activeResources.Add(resource);
            index++;
        }

    }

    public void ShowNextResource()
    {
        foreach (ResourceController resource in _activeResources)
        {
            if (!resource.gameObject.activeSelf)
            {
                resource.gameObject.SetActive(true);
                break;
            }
        }

    }

    private void CollectPerSecond()
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }
        }

        output *= AutoCollectPercentage;
        // Fungsi ToString("F1") ialah membulatkan angka menjadi desimal yang memiliki 1 angka di belakang koma 
        AutoCollectInfo.text = $"Auto Collect: { output.ToString("F1") } / second";

        AddGold(output);
    }

    public void AddGold(double value)
    {
        UserDataManager.Progress.Gold += value;
        GoldInfo.text = $"Gold: { UserDataManager.Progress.Gold.ToString("0") }";

        UserDataManager.Save(_saveDelayCounter < 0f);

        if (_saveDelayCounter < 0f)
        {
            _saveDelayCounter = SaveDelay;
        }

        goldachievement(UserDataManager.Progress.Gold);
    }

    public void goldachievement(double totalgold)
    {
        var b = counter.ToString();
        if (totalgold > 1000000000 && counter == 1)
        {
            AchievementController.Instance.UnlockAchievement2(AchievementType.TotalGold, b, totalgold);
            counter++;
        }

        if (totalgold > 1000000000000 && counter == 2)
        {
            AchievementController.Instance.UnlockAchievement2(AchievementType.TotalGold, b, totalgold);
            counter++;
        }
    }

    public void CollectByTap(Vector3 tapPosition, Transform parent)
    {
        double output = 0;
        foreach (ResourceController resource in _activeResources)
        {
            if (resource.IsUnlocked)
            {
                output += resource.GetOutput();
            }

        }

        Taptext tapText = GetOrCreateTapText();
        tapText.transform.SetParent(parent, false);
        tapText.transform.position = tapPosition;

        tapText.Text.text = $"+{ output.ToString("0") }";
        tapText.gameObject.SetActive(true);
        CoinIcon.transform.localScale = Vector3.one * 1.75f;

        AddGold(output);
    }

    private Taptext GetOrCreateTapText()
    {
        Taptext tapText = _tapTextPool.Find(t => !t.gameObject.activeSelf);
        if (tapText == null)
        {
            tapText = Instantiate(TapTextPrefab).GetComponent<Taptext>();
            _tapTextPool.Add(tapText);
        }
        return tapText;

    }

}

// Fungsi System.Serializable adalah agar object bisa di-serialize dan
// value dapat di-set dari inspector
[System.Serializable]

public struct ResourceConfig
{
    public string Name;
    public double UnlockCost;
    public double UpgradeCost;
    public double Output;

}
//public struct ResourceConfig2
//{
//    public double gold;

//}