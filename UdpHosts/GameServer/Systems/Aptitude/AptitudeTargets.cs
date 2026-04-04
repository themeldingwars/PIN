using System;
using System.Collections;
using System.Collections.Generic;
using Serilog;

namespace GameServer.Aptitude;

public class AptitudeTargets : IEnumerable<IAptitudeTarget>
{
    private static readonly ILogger _logger = Log.ForContext<AptitudeTargets>();

    private readonly List<IAptitudeTarget> _targets;

    public AptitudeTargets()
    {
        _targets = new();
    }

    public AptitudeTargets(AptitudeTargets initialTargets)
    {
        _targets = new List<IAptitudeTarget>(initialTargets);
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

        _logger.Debug("Pushing new target: {target}", target);

        _targets.Add(target);
    }

    public bool TryPop(out IAptitudeTarget result)
    {
        PrintTargets();

        var ok = _targets.Count != 0;

        if (ok)
        {
            result = _targets[^1];

            _logger.Debug("Popping target: {result}", result);

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

            _logger.Debug("Peeking at target: {result}", result);

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

        _logger.Debug("Removing first {number} targets", number);

        _targets.RemoveRange(0, Math.Min(number, _targets.Count));
    }

    public void PopN(int number)
    {
        PrintTargets();

        _logger.Debug("Popping last {number} targets", number);

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

        _logger.Debug("Targets ({count}): {targets}", _targets.Count, s.Trim(',', ' '));
    }
}