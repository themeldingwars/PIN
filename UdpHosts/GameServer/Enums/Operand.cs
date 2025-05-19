namespace GameServer.Enums;

// Based on FUN_00bc1130
public enum Operand : byte
{
    // Add and multiply appear twice in a switch inside client
    ASSIGN = 0,
    ADD = 1,
    MULTIPLY = 2,
    EXPONENTIATE = 3,
    SUBTRACT = 4,
    DIVIDE = 5,
    ADD_ALT = 6,
    MULTIPLY_ALT = 7,
    MINIMUM = 8,
    MAXIMUM = 9,
}