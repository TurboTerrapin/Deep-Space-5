using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CharacterCustomization : MonoBehaviour
{
    public GameObject leftEyeObject = null; 
    public GameObject rightEyeObject = null;
    public GameObject playerObject = null;

    public List<GameObject> hairModels = new List<GameObject>();
    public GameObject hairObject = null;

    public GameObject shirtObject = null;
    public List<Mesh> shirtModels = new List<Mesh>();
    public List<Material> shirtMaterials = new List<Material>();

    public GameObject pantsObject = null;
    public List<Mesh> pantsModels = new List<Mesh>();
    public List<Material> pantsMaterials = new List<Material>();

    private void Start()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening || GetComponent<NetworkObject>() == null || GetComponent<NetworkObject>().IsOwner)
        {
            LoadLocalCharacterAppearance(CustomizeCharacterMenu.GetCharacterData());
        }
    }

    public void ApplyHairColor(Color newColor)
    {
        if (newColor.a > 0.0f)
        {
            Material mat = hairObject.GetComponent<MeshRenderer>().material;
            mat.SetColor("_BaseColor", newColor);
        }
    }

    public void ApplyEyeColor(Color newColor)
    {
        if (newColor.a > 0.0f)
        {
            Material matLeft = leftEyeObject.GetComponent<MeshRenderer>().material;
            Material matRight = rightEyeObject.GetComponent<MeshRenderer>().material;
            matLeft.SetColor("_BaseColor", newColor);
            matRight.SetColor("_BaseColor", newColor);
        }
    }

    public void ApplySkinTone(Color newColor)
    {
        if (newColor.a > 0.0f)
        {
            Material mat = playerObject.GetComponent<SkinnedMeshRenderer>().material;
            mat.SetColor("_BaseColor", newColor);
        }
    }

    public void ApplyHairType(int newHair)
    {
        if (newHair == 0)
        {
            hairObject.GetComponent<MeshFilter>().mesh = null;
            return;
        }

        hairObject.GetComponent<MeshFilter>().mesh = hairModels[newHair - 1].GetComponent<MeshFilter>().sharedMesh;
    }

    public void ApplyClothingType(int newClothing)
    {
        shirtObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = shirtModels[newClothing];
        shirtObject.GetComponent<SkinnedMeshRenderer>().material = shirtMaterials[newClothing];
        pantsObject.GetComponent<SkinnedMeshRenderer>().sharedMesh = pantsModels[newClothing];
        pantsObject.GetComponent<SkinnedMeshRenderer>().material = pantsMaterials[newClothing];
    }

    // ------ LOAD CUSTOMIZATION DATA FROM FILE ------
    public void LoadLocalCharacterAppearance(CustomizeCharacterData data)
    {
        // Load current hair, eyes, skin tone
        ColorUtility.TryParseHtmlString("#" + data.CurrentHairHexValue, out Color HairColor);
        ColorUtility.TryParseHtmlString("#" + data.CurrentEyeHexValue, out Color EyeColor);
        ColorUtility.TryParseHtmlString("#" + data.CurrentSkinToneHexValue, out Color SkinTone);
        ApplyHairColor(HairColor);
        ApplyEyeColor(EyeColor);
        ApplySkinTone(SkinTone);

        // Load hair and clothing options
        ApplyHairType(data.SelectedHairOption);
        ApplyClothingType(data.SelectedClothingOption);
    }
}