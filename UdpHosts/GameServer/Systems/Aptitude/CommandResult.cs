namespace GameServer.Systems.Aptitude;

public struct CommandResult
{
    private const uint _successBit = 1 << 0;
    private const uint _yieldBit   = 1 << 1;
    private const uint _haltBit    = 1 << 2;
    private uint _value;

    public bool Success
    {
        readonly get => (_value & _successBit) != 0;
        set
        {
            if (value)
            {
                _value |= _successBit;
            }
            else
            {
                _value &= ~_successBit;
            }
        }
    }

    public bool Yield
    {
        readonly get => (_value & _yieldBit) != 0;
        set
        {
            if (value)
            {
                _value |= _yieldBit;
            }
            else
            {
                _value &= ~_yieldBit;
            }
        }
    }

    public bool Halt
    {
        readonly get => (_value & _haltBit) != 0;
        set
        {
            if (value)
            {
                _value |= _haltBit;
            }
            else
            {
                _value &= ~_haltBit;
            }
        }
    }

    public StatusCode StatusCode
    {
        readonly get => (StatusCode)(_value >> 16);
        set
        {
            _value &= 0x0000FFFF;
            _value |= (uint)(ushort)value << 16;
        }
    }

    public readonly uint RawValue => _value;

    public readonly string Outcome
    {
        get
        {
            if (Halt)
            {
                return "HALT";
            }

            if (Yield)
            {
                return "WAIT";
            }

            if (Success)
            {
                return "PASS";
            }

            return "FAIL";
        }
    }

    public void SetPass(StatusCode? statusCode = null)
    {
        Success = true;

        if (statusCode.HasValue)
        {
            StatusCode = statusCode.Value;
        }
    }

    public void SetFail(StatusCode? statusCode = null)
    {
        Success = false;

        if (statusCode.HasValue)
        {
            StatusCode = statusCode.Value;
        }
    }

    public void SetYield(StatusCode? statusCode = null)
    {
        Yield = true;

        if (statusCode.HasValue)
        {
            StatusCode = statusCode.Value;
        }
    }

    public void SetHalt(StatusCode? statusCode = null)
    {
        Halt = true;

        if (statusCode.HasValue)
        {
            StatusCode = statusCode.Value;
        }
    }

    public override string ToString()
    {
        return
            $"{Outcome} " +
            $"status={StatusCode} " +
            $"(0x{(ushort)StatusCode:X4}) " +
            $"raw=0x{_value:X8} " +
            $"[S={(Success ? 1 : 0)} Y={(Yield ? 1 : 0)} H={(Halt ? 1 : 0)}]";
    }
}