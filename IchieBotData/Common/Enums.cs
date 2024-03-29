﻿namespace IchieBotData.Common;

public enum AttackType
{
    Normal = 1,
    Special = 2,
    None = 0
}

public enum Element
{
    Flower = 1,
    Wind = 2,
    Snow = 3,
    Moon = 4,
    Space = 5,
    Cloud = 6,
    Dream = 7,
    Sun = 8,
    Star = 9,
    NoElem = 0
}

public enum EffectType
{
    Timed = 0,
    Stack = 1,
    Other = 2
}

public enum EffectQuality
{
    Plus = 0,
    Minus = 1,
    Neutral = 2
}

public enum Condition
{
    None = 0,
    Permanent = 1,
    Start = 2,
    Entry = 3,
    Exit = 4,
    CutIn = 5
}

public enum Pool
{
    Permanent = 0,
    Seasonal = 1,
    Kirafest = 2,
    Limited = 3,
    Event = 4,
    Premium = 5,
    Special = 6,
    Unknown = 7
}

public enum School
{
    Seisho = 1,
    Rinmeikan = 2,
    Frontier = 3,
    Siegfeld = 4,
    Seiran = 5,
    None = -1
}

public enum Cost
{
    Cost6 = 6,
    Cost9 = 9,
    Cost12 = 12,
    Cost13 = 13,
    Cost14 = 14,
    Cost15 = 15,
    Cost20 = 20,
    Cost23 = 23,
    Cost100 = 100
}

public enum Row
{
    Front = 0,
    Middle = 1,
    Back = 2
}

public enum Rarity
{
    Star1 = 1,
    Star2 = 2,
    Star3 = 3,
    Star4 = 4,
    Star5 = 5
}