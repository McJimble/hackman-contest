using UnityEngine;

public interface IDamageReceiver
{
    /// <summary>
    /// Handles what should happen when this object receives damage.
    /// </summary>
    /// <param name="damage">Amount of damage to send</param>
    /// <param name="sender">Object that the caller claims send the damage. Ideally is the caller itself.</param>
    /// <returns>Whether damage was successfully received due to custom attributes of the implementer</returns>
    public bool ReceiveDamage(float damage, object sender = null);
}
