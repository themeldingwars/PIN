using System;
using System.Collections;
using System.Collections.Generic;

namespace GameServer.Aptitude;

public class AptitudeTargets : IEnumerable<IAptitudeTarget>
{
    private readonly List<IAptitudeTarget> _targets;

    public AptitudeTargets()
    {
        _targets = new();
    }

    public AptitudeTargets(AptitudeTargets initialTargets)
    {
        _targets = new List<IAptitudeTarget>(initialTargets.ToArray());
    }

    public AptitudeTargets(params IAptitudeTarget[] initialTargets)
    {
        _targets = new(initialTargets.Length);

        foreach (var target in initialTargets)
        {
            if (target != null)
            {
                _targets.Add(target);
            }
        }

        PrintTargets();
    }

    public int Count => _targets.Count;

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }

    public IEnumerator<IAptitudeTarget> GetEnumerator()
    {
        return _targets.GetEnumerator();
    }

    public void Push(IAptitudeTarget target)
    {
        if (target == null)
        {
            return;
        }

        PrintTargets();

        Console.WriteLine($"Pushing new target: {target}");

        _targets.Add(target);
    }

    public bool TryPop(out IAptitudeTarget result)
    {
        PrintTargets();

        var ok = _targets.Count != 0;

        if (ok)
        {
            result = _targets[^1];

            Console.WriteLine($"Popping target: {result}");

            _targets.RemoveAt(_targets.Count - 1);

            return true;
        }

        result = null;

        return false;
    }

    public bool TryPeek(out IAptitudeTarget result)
    {
        var ok = _targets.Count != 0;

        if (ok)
        {
            result = _targets[^1];

            Console.WriteLine($"Peeking at target: {result}");

            return true;
        }

        result = null;

        return false;
    }

    public IAptitudeTarget Peek()
    {
        return _targets[^1];
    }

    public void RemoveBottomN(int number)
    {
        PrintTargets();

        Console.WriteLine($"Removing first {number} targets");

        _targets.RemoveRange(0, Math.Min(number, _targets.Count));
    }

    public void PopN(int number)
    {
        PrintTargets();

        Console.WriteLine($"Popping last {number} targets");

        _targets.RemoveRange(_targets.Count - Math.Min(number, _targets.Count), Math.Min(number, _targets.Count));
    }

    public void Clear()
    {
        _targets.Clear();
    }

    public IAptitudeTarget[] ToArray()
    {
        return _targets.ToArray();
    }

    public void PrintTargets()
    {
        var s = string.Empty;

        foreach (var e in _targets)
        {
            s += e + ", ";
        }

        Console.WriteLine($"Targets ({_targets.Count}): {s.Trim(',', ' ')}");
    }
}