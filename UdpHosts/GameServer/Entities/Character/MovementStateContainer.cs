using System;
using System.Linq;

namespace GameServer.Entities.Character;

/// <summary>
///     Allows eased access to the several flags
/// </summary>
internal class MovementStateContainer
{
    public CharMovementState MovementState;

    /// <summary>
    ///     The set flags which are not specified in the enum
    /// </summary>
    /// <returns></returns>
    public CharMovementState InvalidFlags => MovementState ^ ValidFlags;

    /// <summary>
    ///     The set flags which are specified in the enum
    /// </summary>
    public CharMovementState ValidFlags => MovementState & Enum.GetValues<CharMovementState>()
                                                               .Aggregate((accumulate, currentValue) => accumulate | currentValue);

    /// <summary>
    ///     Corresponds to the <see cref="CharMovementState.Crouch" /> flag
    /// </summary>
    public bool Crouch
    {
        get => GetValue(CharMovementState.Crouch);
        set => SetValue(CharMovementState.Crouch, value);
    }

    /// <summary>
    ///     Corresponds to the <see cref="CharMovementState.MovementKeys" /> flag
    /// </summary>
    public bool MovementKeys
    {
        get => GetValue(CharMovementState.MovementKeys);
        set => SetValue(CharMovementState.MovementKeys, value);
    }

    /// <summary>
    ///     Corresponds to the <see cref="CharMovementState.Sprint" /> flag
    /// </summary>
    public bool Sprint
    {
        get => GetValue(CharMovementState.Sprint);
        set => SetValue(CharMovementState.Sprint, value);
    }

    /// <summary>
    ///     Corresponds to the <see cref="CharMovementState.TryingToMove" /> flag
    /// </summary>
    public bool TryingToMove
    {
        get => GetValue(CharMovementState.TryingToMove);
        set => SetValue(CharMovementState.TryingToMove, value);
    }

    /// <summary>
    ///     Corresponds to the <see cref="CharMovementState.IsMoving" /> flag
    /// </summary>
    public bool IsMoving
    {
        get => GetValue(CharMovementState.IsMoving);
        set => SetValue(CharMovementState.IsMoving, value);
    }

    /// <summary>
    ///     Corresponds to the <see cref="CharMovementState.Unk" /> flag
    /// </summary>
    public bool Unk
    {
        get => GetValue(CharMovementState.Unk);
        set => SetValue(CharMovementState.Unk, value);
    }

    /// <summary>
    ///     Print the currently set valid and invalid flags
    /// </summary>
    /// <returns></returns>
    public override string ToString()
    {
        return $"MovementState value: {MovementState,5} | {GetBinaryString(MovementState)}\n" +
               $"Valid flags:         {ValidFlags,5:D} | {GetBinaryString(ValidFlags)}\n" +
               $"                     [{ValidFlags:F}]\n" +
               $"Invalid flags:       {InvalidFlags,5} | {GetBinaryString(InvalidFlags)}\n" +
               "Flag overview:\n" +
               $"  {string.Join("\n  ", Enum.GetValues<CharMovementState>().Where(state => state != CharMovementState.None).Select(state => $"{Enum.GetName(typeof(CharMovementState), state) + ":",-13} {GetValue(state)}"))}";
    }
    
    /// <summary>
    ///     Get a binary-number string representation of the passed value
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private static string GetBinaryString(CharMovementState state)
    {
        return Convert.ToString((short)state, 2).PadLeft(16, '0');
    }
    
    /// <summary>
    ///     Get a flag
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    private bool GetValue(CharMovementState state)
    {
        return (MovementState & state) == state;
    }

    /// <summary>
    ///     Set or unset the flag, depending on the value
    /// </summary>
    /// <param name="state"></param>
    /// <param name="value">if true the specified flag will be set, else it will be unset</param>
    private void SetValue(CharMovementState state, bool value)
    {
        if (value)
        {
            MovementState |= state;
        }
        else
        {
            MovementState ^= state;
        }
    }
}