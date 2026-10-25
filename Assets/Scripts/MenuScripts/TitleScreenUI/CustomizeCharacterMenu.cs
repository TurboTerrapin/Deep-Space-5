using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UIButton = UnityEngine.UI.Button;

public class CustomizeCharacterData
{
    public string CurrentHairHexValue;
    public string CurrentEyeHexValue;
    public string CurrentSkinToneHexValue;

    public string[] CustomHairSwatchHexValues;
    public string[] CustomEyeSwatchHexValues;
    public string[] CustomSkinToneSwatchHexValues;

    public string FirstName;
    public string LastName;

    public int SelectedHairOption;
    public int SelectedClothingOption;
}

public class CustomizeCharacterMenu : MonoBehaviour
{
    //CLASS CONSTANTS
    private static string[] CLOTHING_OPTION_NAMES = { "Blue Uniform", "Purple Uniform", "Orange Uniform", "Green Uniform" };
    private static string[] FIRST_NAMES = { "Morgan", "Charlie", "Taylor", "Cameron", "Alex", "Jamie", "Robin", "Elliott", "Jordan", "Griffin" };
    private static string[] LAST_NAMES = { "Gregory", "Parsons", "Brannon", "Smith", "Johnson", "Miller", "Anderson", "Kirk", "Chambers", "Allen", "Stewart", "Murphy", "Morgan", "Gehring", "Murray", "Ford", "Simpson", "Porter", "Hansen", "Carroll", "Matthews", "Greene", "Davidson", "Horton", "Meyer", "Morton", "Flowers", "Huffman", "Moon", "McCoy", "Madden", "Raleigh", "Crawford" };
    private static string[] HAIR_OPTIONS = { "Bald", "Short", "Medium", "Long" };
    private static string[] BANNED_WORDS = { "anal", "anus", "arsehole", "asshole", "assmunch", "auto erotic", "autoerotic", "bbw", "bdsm", "beaner", "bastard", "bimbos", "bitch", "blowjob", "blow job", "bollocks", "bondage", "boner", "boob", "bukkake", "bulldyke", "bullet vibe", "bung hole", "bunghole", "busty", "buttcheeks", "butthole", "camgirl", "camslut", "camwhore", "chink", "circlejerk", "clit", "clitoris", "cock", "coon", "creampie", "cumming", "cunt", "darkie", "daterape", "deepthroat", "dendrophilia", "dildo", "doggiestyle", "doggystyle", "dolcett", "dommes", "dvda", "ecchi", "erotic", "erotism", "escort", "eunuch", "faggot", "fecal", "felch", "fellatio", "feltch", "femdom", "figging", "fingering", "fisting", "footjob", "frotting", "fuck", "fudgepacker", "futanari", "genitals", "goatse", "gokkun", "goregasm", "grope", "guro", "handjob", "hardcore", "hentai", "honkey", "hooker", "humping", "incest", "intercourse", "jailbait", "jizz", "juggs", "kinbaku", "kinkster", "kinky", "knobbing", "kyke", "lolita", "lovemaking", "masturbate", "milf", "motherfucker", "muffdiving", "nawashi", "neonazi", "nigga", "nigger", "nipple", "nipples", "nympho", "omorashi", "orgasm", "orgy", "paedophile", "panties", "panty", "pedobear", "pedophile", "pegging", "penis", "pissing", "pisspig", "playboy", "ponyplay", "poontang", "punany", "poopchute", "porn", "porno", "pubes", "pussy", "queaf", "queef", "quim", "raping", "rapist", "rectum", "rimjob", "rimming", "sadism", "scat", "schlong", "semen", "sex", "shibari", "shit", "shota", "skeet", "slut", "smut", "snatch", "sodomize", "sodomy", "splooge", "spooge", "spunk", "strapon", "strappado", "suck", "sucks", "swastika", "swinger", "throating", "titties", "titty", "topless", "tosser", "tranny", "tubgirl", "tushy", "twat", "twink", "twinkie", "upskirt", "urophilia", "vagina", "vibrator", "voyeur", "vulva", "wank", "wetback", "xxx", "yaoi", "yuri", "yiffy" };
    private static Color[] DEFAULT_HAIR_SWATCHES = new Color[13] { new Color(0.953f, 0.898f, 0.671f), new Color(0.631f, 0.525f, 0.435f), new Color(0.294f, 0.212f, 0.129f), new Color(0.231f, 0.184f, 0.184f), new Color(0.71f, 0.322f, 0.224f), new Color(0.502f, 0.502f, 0.502f), new Color(1.0f, 0.188f, 0.188f), new Color(1.0f, 0.412f, 0.706f), new Color(0.608f, 0.188f, 1.0f), new Color(0.227f, 0.373f, 0.804f), new Color(0.235f, 0.702f, 0.443f), new Color(1.0f, 1.0f, 1.0f), new Color(0.0f, 0.0f, 0.0f) };
    private static Color[] DEFAULT_EYE_SWATCHES = new Color[13] { new Color(0.639f, 0.788f, 0.976f), new Color(0.424f, 0.627f, 0.863f), new Color(0.258f, 0.541f, 0.868f), new Color(0.29f, 0.427f, 0.549f), new Color(0.174f, 0.361f, 0.585f), new Color(0.58f, 0.749f, 0.655f), new Color(0.482f, 0.627f, 0.357f), new Color(0.345f, 0.549f, 0.271f), new Color(0.247f, 0.439f, 0.302f), new Color(0.51f, 0.4f, 0.267f), new Color(0.361f, 0.267f, 0.149f), new Color(0.231f, 0.184f, 0.184f), new Color(0.11f, 0.11f, 0.11f) };
    private static Color[] DEFAULT_SKIN_TONE_SWATCHES = new Color[13] { new Color(1.0f, 0.878f, 0.741f), new Color(1.0f, 0.804f, 0.58f), new Color(0.918f, 0.753f, 0.525f), new Color(1.0f, 0.678f, 0.471f), new Color(0.82f, 0.639f, 0.486f), new Color(0.776f, 0.525f, 0.259f), new Color(0.659f, 0.459f, 0.282f), new Color(0.553f, 0.333f, 0.141f), new Color(0.482f, 0.29f, 0.184f), new Color(0.42f, 0.267f, 0.137f), new Color(0.361f, 0.227f, 0.129f), new Color(0.294f, 0.196f, 0.125f), new Color(0.231f, 0.184f, 0.184f) };
    private static float ROTATION_SPEED = 50.0f;

    public Transform Dummy;
    private CharacterCustomization DummyAppearance;
    private Vector3 LastMousePosition;

    public GameObject MainMenu;
    public GameObject CustomizationMenu;

    public TMP_InputField HairHexInput; 
    public UIButton[] HairSwatchButtons; // All swatches
    public UIButton[] CustomHairSwatchButtons; // Buttons #14-22 (custom swatches)
    private int NextHairSwatchIndex = 0; // Keeps track of which swatch to change next

    public TMP_InputField EyeHexInput;
    public UIButton[] EyeSwatchButtons;
    public UIButton[] CustomEyeSwatchButtons;
    private int NextEyeSwatchIndex = 0;

    public TMP_InputField SkinToneHexInput;
    public UIButton[] SkinToneSwatchButtons;
    public UIButton[] CustomSkinToneSwatchButtons;
    private int NextSkinToneSwatchIndex = 0;

    public TMP_InputField FirstNameInput;
    public TMP_InputField LastNameInput;

    public TMP_Text ClothingOptionText;
    public UIButton LeftClothingButton;
    public UIButton RightClothingButton;
    private int CurrentClothingOptionIndex = 0;

    private List<int> UnlockedClothingOptions = new List<int> {0, 1, 2, 3,};

    public TMP_Text HairOptionText;
    public UIButton LeftHairButton;
    public UIButton RightHairButton;
    private int CurrentHairOptionIndex = 0;

    private void Start()
    {
        //UnlockSecretClothingOptions(true, true);
        //UnlockSecretClothingOptions(true, false);
        //UnlockSecretClothingOptions(false, true);
        //DeleteCharacterSaveData();

        // Input checks
        HairHexInput.characterLimit = 6;
        HairHexInput.onValidateInput = CheckHexValue;

        EyeHexInput.characterLimit = 6;
        EyeHexInput.onValidateInput = CheckHexValue;

        SkinToneHexInput.characterLimit = 6;
        SkinToneHexInput.onValidateInput = CheckHexValue;

        FirstNameInput.characterLimit = 10;
        FirstNameInput.onValidateInput = CheckName;

        LastNameInput.characterLimit = 10;
        LastNameInput.onValidateInput = CheckName;

        // Listen for when player hits enter, call OnSubmit and pass the hex value to it.
        HairHexInput.onSubmit.AddListener(OnHairHexSubmitted);
        EyeHexInput.onSubmit.AddListener(OnEyeHexSubmitted);
        SkinToneHexInput.onSubmit.AddListener(OnSkinToneHexSubmitted);

        // Listen for when player clicks off of player name inputs
        FirstNameInput.onDeselect.AddListener(HandleFirstNameSubmitted);
        FirstNameInput.onEndEdit.AddListener(HandleFirstNameSubmitted);
        LastNameInput.onDeselect.AddListener(HandleLastNameSubmitted);
        LastNameInput.onEndEdit.AddListener(HandleLastNameSubmitted);

        // UI to click through different clothing and hair options
        LeftClothingButton.onClick.AddListener(PreviousClothingOption);
        RightClothingButton.onClick.AddListener(NextClothingOption);

        LeftHairButton.onClick.AddListener(PreviousHairOption);
        RightHairButton.onClick.AddListener(NextHairOption);

        foreach (UIButton button in HairSwatchButtons)
        {
            UIButton capturedButton = button;
            capturedButton.onClick.AddListener(() =>
            {
                Color buttonColor = capturedButton.image.color;
                DummyAppearance.ApplyHairColor(buttonColor);
            });
        }

        foreach (UIButton button in EyeSwatchButtons)
        {
            UIButton capturedButton = button;
            capturedButton.onClick.AddListener(() =>
            {
                Color buttonColor = capturedButton.image.color;
                DummyAppearance.ApplyEyeColor(buttonColor);
            });
        }

        foreach (UIButton button in SkinToneSwatchButtons)
        {
            UIButton capturedButton = button;
            capturedButton.onClick.AddListener(() =>
            {
                Color buttonColor = capturedButton.image.color;
                DummyAppearance.ApplySkinTone(buttonColor);
            });
        }

        DummyAppearance = Dummy.GetComponent<CharacterCustomization>();
        LoadCharacterData();
        OnHairUpdate();
        OnClothingUpdate();
    }

    private void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            LastMousePosition = Input.mousePosition;
        }

        if (Input.GetMouseButton(0))
        {
            Vector3 MouseMovement = Input.mousePosition - LastMousePosition;

            Dummy.Rotate(0.0f, -MouseMovement.x * ROTATION_SPEED * Time.deltaTime, 0.0f);

            LastMousePosition = Input.mousePosition;
        }
    }

    // ------ HANDLE HEX VALUE INPUTS ------

    // Helper method used to ensure that hex value strings either start with a # or are empty
    private string EnforcePoundSymbolStart(string HexValue)
    {
        if (!string.IsNullOrEmpty(HexValue))
        {
            if (!HexValue.StartsWith("#"))
            {
                HexValue = "#" + HexValue;
            }
            return HexValue;
        }
        return "";
    }

    private Color OnHexSubmitted(string HexValue, UIButton CustomSwatchButton, UIButton[] SwatchButtons, TMP_InputField InputField)
    {
        HexValue = EnforcePoundSymbolStart(HexValue);
        InputField.text = "";

        // If the player enters a valid hex #, convert it to a Unity color
        if (ColorUtility.TryParseHtmlString(HexValue, out Color newColor))
        {
            newColor.a = 1.0f;
            foreach (UIButton SwatchButton in SwatchButtons)
            {
                if (SwatchButton.image.color == newColor)
                {
                    // Color already a swatch option
                    return newColor;
                }
            }
            CustomSwatchButton.image.color = newColor;
            CustomSwatchButton.interactable = true;
            return newColor;
        }
        return new Color(0.0f, 0.0f, 0.0f, 0.0f);
    }

    private void OnHairHexSubmitted(string HairHexValue)
    {
        DummyAppearance.ApplyHairColor(OnHexSubmitted(HairHexValue, CustomHairSwatchButtons[NextHairSwatchIndex], HairSwatchButtons, HairHexInput));
        NextHairSwatchIndex = FindEmptySwatch(CustomHairSwatchButtons);
    }

    private void OnEyeHexSubmitted(string EyeHexValue)
    {
        DummyAppearance.ApplyEyeColor(OnHexSubmitted(EyeHexValue, CustomEyeSwatchButtons[NextEyeSwatchIndex], EyeSwatchButtons, EyeHexInput));
        NextEyeSwatchIndex = FindEmptySwatch(CustomEyeSwatchButtons);
    }

    private void OnSkinToneHexSubmitted(string SkinToneHexValue)
    {
        DummyAppearance.ApplySkinTone(OnHexSubmitted(SkinToneHexValue, CustomSkinToneSwatchButtons[NextSkinToneSwatchIndex], SkinToneSwatchButtons, SkinToneHexInput));
        NextSkinToneSwatchIndex = FindEmptySwatch(CustomSkinToneSwatchButtons);
    }

    // ------ CHARACTER FIRST AND LAST NAME ------

    private static bool IsBanned(string Name)
    {
        Name = Name.ToLower();
        foreach (string bannedWord in BANNED_WORDS)
        {
            if (Name.Contains(bannedWord))
            {
                return true;
            }
        }
        return false;
    }

    private void HandleFirstNameSubmitted(string Name)
    {
        if (string.IsNullOrEmpty(Name) || IsBanned(Name))
        {
            // Revert to current first name if invalid
            FirstNameInput.text = GetFirstName(); 
        }
    }

    private void HandleLastNameSubmitted(string Name)
    {
        if (string.IsNullOrEmpty(Name) || IsBanned(Name))
        {
            // Revert to current last name if invalid
            LastNameInput.text = GetLastName(); 
        }
    }

    public static CustomizeCharacterData GetCharacterData()
    {
        if (PlayerPrefs.HasKey("CustomizeCharacterData"))
        {
            // Get the JSON string we stored in PlayerPrefs
            string JSON = PlayerPrefs.GetString("CustomizeCharacterData");
            // Convert the string back to a CustomizeCharacterData object
            return JsonUtility.FromJson<CustomizeCharacterData>(JSON);
        }
        return GenerateDefaultCustomization();
    }

    public static string GetFirstName()
    {
        return GetCharacterData().FirstName;
    }

    public static string GetLastName()
    {
        return GetCharacterData().LastName;
    }

    // ------ CHANGE HAIR/CLOTHING OPTIONS ------

    private void PreviousClothingOption()
    {
        int CurrentPosition = UnlockedClothingOptions.IndexOf(CurrentClothingOptionIndex);
        CurrentPosition--;
        if (CurrentPosition < 0)
        {
            // Wrap around
            CurrentPosition = UnlockedClothingOptions.Count - 1;
        }
        CurrentClothingOptionIndex = UnlockedClothingOptions[CurrentPosition];
        OnClothingUpdate();
    }

    private void NextClothingOption()
    {
        int CurrentPosition = UnlockedClothingOptions.IndexOf(CurrentClothingOptionIndex);
        CurrentPosition++;
        if (CurrentPosition >= UnlockedClothingOptions.Count)
        {
            // Wrap around
            CurrentPosition = 0;
        }
        CurrentClothingOptionIndex = UnlockedClothingOptions[CurrentPosition];
        OnClothingUpdate();
    }

    private void OnClothingUpdate()
    {
        DummyAppearance.ApplyClothingType(CurrentHairOptionIndex);
        ClothingOptionText.text = CLOTHING_OPTION_NAMES[CurrentClothingOptionIndex];
    }

    private void UnlockSecretClothingOptions(bool HasBeatenGame, bool HasBeatenAllScenarios)
    {
        UnlockedClothingOptions = new List<int> { 0,1,2,3}; 

        if (HasBeatenGame)
        {
            UnlockedClothingOptions.Add(4);
        }
        if (HasBeatenAllScenarios)
        {
            UnlockedClothingOptions.Add(5);
        }
    }

    private void PreviousHairOption()
    {
        CurrentHairOptionIndex--;
        if (CurrentHairOptionIndex < 0)
        {
            // Wrap around
            CurrentHairOptionIndex = HAIR_OPTIONS.Length - 1;
        }
        OnHairUpdate();
    }

    private void NextHairOption()
    {
        CurrentHairOptionIndex++;
        if (CurrentHairOptionIndex >= HAIR_OPTIONS.Length)
        {
            // Wrap around
            CurrentHairOptionIndex = 0;
        }
        OnHairUpdate();
    }

    private void OnHairUpdate()
    {
        DummyAppearance.ApplyHairType(CurrentHairOptionIndex);
        HairOptionText.text = HAIR_OPTIONS[CurrentHairOptionIndex];
    }

    // ------ TEXT INPUT LIMITATIONS/CHECKS ------

    private char CheckName(string text, int charIndex, char addedChar)
    {
        if (char.IsLetter(addedChar))
        {
            // Allow
            return addedChar;
        }
        else
        {
            // '\0' tells Unity to reject that char
            return '\0';
        }
    }

    private char CheckHexValue(string text, int charIndex, char addedChar)
    {
        // Allow number inputs
        if (char.IsDigit(addedChar))
        {
            // Allow
            return addedChar;
        }

        // Convert char to uppercase
        char upperChar = char.ToUpper(addedChar);

        if (upperChar >= 'A' && upperChar <= 'F')
        {
            // Allow
            return addedChar;
        }

        // Disallow
        return '\0';
    }

    private static CustomizeCharacterData GenerateDefaultCustomization()
    {
        // New data object, start from scratch
        CustomizeCharacterData Data = new CustomizeCharacterData();

        Data.FirstName = FIRST_NAMES[Random.Range(0, FIRST_NAMES.Length)];
        Data.LastName = LAST_NAMES[Random.Range(0, LAST_NAMES.Length)];
        Data.CurrentHairHexValue = ColorUtility.ToHtmlStringRGBA(DEFAULT_HAIR_SWATCHES[Random.Range(0, DEFAULT_HAIR_SWATCHES.Length)]);
        Data.CurrentEyeHexValue = ColorUtility.ToHtmlStringRGBA(DEFAULT_EYE_SWATCHES[Random.Range(0, DEFAULT_EYE_SWATCHES.Length)]);
        Data.CurrentSkinToneHexValue = ColorUtility.ToHtmlStringRGBA(DEFAULT_SKIN_TONE_SWATCHES[Random.Range(0, DEFAULT_SKIN_TONE_SWATCHES.Length)]);
        Data.SelectedHairOption = Random.Range(0, HAIR_OPTIONS.Length);
        Data.SelectedClothingOption = 0;
        Data.CustomHairSwatchHexValues = new string[9];
        Data.CustomEyeSwatchHexValues = new string[9];
        Data.CustomSkinToneSwatchHexValues = new string[9];

        // Save new data
        WriteToPlayerPrefs(Data);

        return Data;
    }

    public void SaveCharacterData()
    {
        // New data object, start from scratch
        CustomizeCharacterData Data = new CustomizeCharacterData();

        // Save current player data (hair, eyes, skin tone)
        Data.CurrentHairHexValue = ColorUtility.ToHtmlStringRGBA(DummyAppearance.hairObject.GetComponent<MeshRenderer>().material.GetColor("_BaseColor"));
        Data.CurrentEyeHexValue = ColorUtility.ToHtmlStringRGBA(DummyAppearance.leftEyeObject.GetComponent<MeshRenderer>().material.GetColor("_BaseColor"));
        Data.CurrentSkinToneHexValue = ColorUtility.ToHtmlStringRGBA(DummyAppearance.playerObject.GetComponent<SkinnedMeshRenderer>().material.GetColor("_BaseColor"));

        // Save custom swatches
        Data.CustomHairSwatchHexValues = new string[CustomHairSwatchButtons.Length];
        for (int i = 0; i < CustomHairSwatchButtons.Length; i++)
        {
            Data.CustomHairSwatchHexValues[i] = ColorUtility.ToHtmlStringRGBA(CustomHairSwatchButtons[i].image.color);
        }
        Data.CustomEyeSwatchHexValues = new string[CustomEyeSwatchButtons.Length];
        for (int i = 0; i < CustomEyeSwatchButtons.Length; i++)
        {
            Data.CustomEyeSwatchHexValues[i] = ColorUtility.ToHtmlStringRGBA(CustomEyeSwatchButtons[i].image.color);
        }
        Data.CustomSkinToneSwatchHexValues = new string[CustomSkinToneSwatchButtons.Length];
        for (int i = 0; i < CustomHairSwatchButtons.Length; i++)
        {
            Data.CustomSkinToneSwatchHexValues[i] = ColorUtility.ToHtmlStringRGBA(CustomSkinToneSwatchButtons[i].image.color);
        }

        // Save player name
        Data.FirstName = FirstNameInput.text;
        Data.LastName = LastNameInput.text;

        // Save selected hair and clothing options
        Data.SelectedHairOption = CurrentHairOptionIndex;
        Data.SelectedClothingOption = CurrentClothingOptionIndex;

        WriteToPlayerPrefs(Data);
    }

    private static void WriteToPlayerPrefs(CustomizeCharacterData Data)
    {
        // Convert data into a single JSON string
        string JSON = JsonUtility.ToJson(Data);

        // Store the string in PlayerPrefs using CustomizeCharacterData as a key
        PlayerPrefs.SetString("CustomizeCharacterData", JSON);
        PlayerPrefs.Save();
    }

    private int FindEmptySwatch(UIButton[] CustomSwatchButtons)
    {
        for (int i = 0; i < CustomSwatchButtons.Length; i++)
        {
            if (CustomSwatchButtons[i].image.color.a <= 0.01f)
            {
                return i;
            }
        }
        return CustomHairSwatchButtons.Length - 1;
    }

    private void LoadSwatches(string[] SwatchValues, UIButton[] SwatchButtons)
    {
        for (int i = 0; i < SwatchValues.Length; i++)
        {
            if (ColorUtility.TryParseHtmlString("#" + SwatchValues[i], out Color c))
            {
                SwatchButtons[i].image.color = c;
                SwatchButtons[i].interactable = true;
            }
        }
    }

    public void LoadCharacterData()
    {
        // Get data (will create new data if none available)
        CustomizeCharacterData Data = GetCharacterData();

        // Update dummy appearance
        DummyAppearance.LoadLocalCharacterAppearance(Data);

        // Load swatches
        LoadSwatches(Data.CustomHairSwatchHexValues, CustomHairSwatchButtons);
        NextHairSwatchIndex = FindEmptySwatch(CustomHairSwatchButtons);
        LoadSwatches(Data.CustomEyeSwatchHexValues, CustomEyeSwatchButtons);
        NextEyeSwatchIndex = FindEmptySwatch(CustomEyeSwatchButtons);
        LoadSwatches(Data.CustomSkinToneSwatchHexValues, CustomSkinToneSwatchButtons);
        NextSkinToneSwatchIndex = FindEmptySwatch(CustomSkinToneSwatchButtons);

        // Load player name
        FirstNameInput.text = Data.FirstName;
        LastNameInput.text = Data.LastName;

        // Load selected options
        CurrentHairOptionIndex = Data.SelectedHairOption;
        CurrentClothingOptionIndex = Data.SelectedClothingOption;
    }

    public void HandleXButtonClick()
    {
        // Closes settings menu
        SaveCharacterData();
        CustomizationMenu.SetActive(false);
        MainMenu.SetActive(true);
    }

    // TEST METHOD TO ERASE CHARACTER SAVE DATA
    private void DeleteCharacterSaveData()
    {
        PlayerPrefs.DeleteKey("CustomizeCharacterData");
        PlayerPrefs.Save();

        Debug.Log("Character Save Data Deleted");
    }
}