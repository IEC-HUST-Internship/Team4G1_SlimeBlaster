using UnityEngine;

public class StoreCurrencyReference : MonoBehaviour
{
    [Header("Currency Pools")]
    public ObjectPool redBitsCurrencyPool;
    public ObjectPool blueBitsCurrencyPool;
    public ObjectPool purpleBitsCurrencyPool;

    public ObjectPool GetCurrencyPool(EnumCurrency currencyType)
    {
        switch (currencyType)
        {
            case EnumCurrency.redBits:
                return redBitsCurrencyPool;
            case EnumCurrency.blueBits:
                return blueBitsCurrencyPool;
            case EnumCurrency.purpleBits:
                return purpleBitsCurrencyPool;
            default:
                return null;
        }
    }
}
