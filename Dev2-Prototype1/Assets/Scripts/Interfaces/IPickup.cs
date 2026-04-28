using NUnit.Framework.Interfaces;
using UnityEngine;

public interface IPickup
{
    public void getWeaponData(WeaponData gun); // Needs to take in a scriptable gameobject (basically contains data)
}
