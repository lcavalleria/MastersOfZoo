public enum Rarity
{
    Basic,
    Common,
    Uncommon,
    Rare,
    Epic,
    Heroic,
    Legendary
}
public static class RarityMethods
{
    public static Rarity GetRarity(int sum)
    {
        if (sum <= 10)
            return Rarity.Basic;
        else if (sum <= 13)
            return Rarity.Common;
        else if (sum <= 16)
            return Rarity.Uncommon;
        else if (sum <= 19)
            return Rarity.Rare;
        else if (sum <= 23)
            return Rarity.Epic;
        else if (sum <= 27)
            return Rarity.Heroic;
        else
            return Rarity.Legendary;
}
}