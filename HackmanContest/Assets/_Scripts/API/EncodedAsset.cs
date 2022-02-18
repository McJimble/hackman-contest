using UnityEngine;

/// <summary>
/// Simply holds a string reference that we can store as a permanent asset.
/// String is intended to be encoded, so decode it using whatever methods
/// are necessary for your implementation.
/// </summary>
public class EncodedAsset : ScriptableObject
{
    [SerializeField] private string encryptedData;

    public string EncryptedData { get => encryptedData; set => encryptedData = value; }
}
