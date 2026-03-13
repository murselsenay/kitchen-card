using Modules.Economy.Enums;

namespace Modules.Economy.Constants
{
    public struct CurrencyKeys
    {
        public static string Gold = "gold";
        public static string Gem = "gem";

        public static string GetKeyForCurrency(ECurrencyType type)
        {
            return type switch
            {
                ECurrencyType.Gold => Gold,
                ECurrencyType.Gem => Gem,
                _ => Gold
            };
        }
    }
}
