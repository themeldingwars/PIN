using System.Linq;
using System.Numerics;
using Shared.Content.Zones;

namespace Shared.Content.Tests;

[TestClass]
public sealed class ContentValidatorTests
{
    [TestMethod]
    public void Validate_FindsATwiceUsedId()
    {
        var deployable = new ZoneDeployable { Id = "ne-terminal", Type = 820, Position = Vector3.Zero };

        var problems = ContentValidator.Validate(new ZoneContent(448) { Deployables = [deployable, deployable with { Type = 821 }] });

        Assert.HasCount(1, problems);
        StringAssert.Contains(problems[0].Message, "2 times");
    }

    [TestMethod]
    public void Validate_FindsAnIdOutsideTheFormat()
    {
        var problems = ContentValidator.Validate(new ZoneContent(448) { Deployables = [new ZoneDeployable { Id = "NE Terminal", Type = 820, Position = Vector3.Zero }] });

        Assert.HasCount(1, problems);
    }

    [TestMethod]
    public void Validate_FindsAMeldingWithoutAnOffsetPerControlPoint()
    {
        var melding = new ZoneMelding { Id = "hydro-core", PerimeterSet = "Hydro_Core_Melding_02", ControlPoints = [Vector3.Zero, Vector3.One], Offsets = [Vector3.Zero] };

        var problems = ContentValidator.Validate(new ZoneContent(448) { Meldings = [melding] });

        Assert.HasCount(1, problems);
    }

    [TestMethod]
    public void Changes_MatchEntriesByKey()
    {
        var kept = new ZoneDeployable { Id = "kept", Type = 1, Position = Vector3.Zero };
        var moved = new ZoneDeployable { Id = "moved", Type = 1, Position = Vector3.Zero };
        var removed = new ZoneDeployable { Id = "removed", Type = 1, Position = Vector3.Zero };
        var added = new ZoneDeployable { Id = "added", Type = 1, Position = Vector3.Zero };

        var changes = ContentChanges<ZoneDeployable>.Between([kept, moved, removed], [kept, moved with { Position = Vector3.One }, added]);

        CollectionAssert.AreEqual(new[] { added }, changes.Added.ToArray());
        CollectionAssert.AreEqual(new[] { removed }, changes.Removed.ToArray());
        Assert.HasCount(1, changes.Changed);
        Assert.AreEqual(Vector3.One, changes.Changed[0].After.Position);
    }
}
