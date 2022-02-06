using UnityEngine;

public class EncodedAsset : ScriptableObject
{
    [SerializeField] private string encryptedData;

    public string EncryptedData { get => encryptedData; set => encryptedData = value; }
}
