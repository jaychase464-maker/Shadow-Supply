using System;

namespace ShadowSupply.Character
{
    public enum CharacterSlot
    {
        BaseBody,
        Hair,
        FacialHair,
        Underwear,
        Torso,
        Legs,
        Feet,
        Gloves,
        Headwear,
        Backpack,
        ChestAccessory,
        HipAccessory
    }

    [Flags]
    public enum CharacterBodyRegion
    {
        None = 0,
        Head = 1 << 0,
        Neck = 1 << 1,
        Torso = 1 << 2,
        Arms = 1 << 3,
        Hands = 1 << 4,
        Hips = 1 << 5,
        Legs = 1 << 6,
        Feet = 1 << 7
    }

    public enum CharacterSocket
    {
        Head,
        Back,
        Chest,
        LeftHand,
        RightHand,
        Hips,
        LeftHip,
        RightHip
    }
}
