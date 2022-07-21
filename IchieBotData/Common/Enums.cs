namespace IchieBotData.Common;

public enum AttackType
{
    Normal = 0,
    Special = 1,
    None = 2
}

public enum Element
{
    NonElem = 0,
    Flower = 1,
    Wind = 2,
    Snow = 3,
    Moon = 4,
    Space = 5,
    Cloud = 6,
    Dream = 7
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
    Seiran = 5
}