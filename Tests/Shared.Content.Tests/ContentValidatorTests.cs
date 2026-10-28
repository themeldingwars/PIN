using System.Linq;
using System.Numerics;
using Shared.Content.Spawns;
using Shared.Content.Zones;

namespace Shared.Content.Tests;

[TestClass]
public sealed class ContentValidatorTests
{
    [TestMethod]
    public void Validate_FindsATwiceUsedId()
    {
        var deployable = new ZoneDeployable { Id = "ne-terminal", Type = 820, Position = Vector3.Zero };

        var problems = ContentValidator.Validate(new ZoneContent(448) { Deployables = [deployable, deployable with { Type = 821 }] }, []);

        Assert.HasCount(1, problems);
        StringAssert.Contains(problems[0].Message, "2 times");
    }

    [TestMethod]
    public void Validate_FindsAnIdOutsideTheFormat()
    {
        var problems = ContentValidator.Validate(new ZoneContent(448) { Deployables = [new ZoneDeployable { Id = "NE Terminal", Type = 820, Position = Vector3.Zero }] }, []);

        Assert.HasCount(1, problems);
    }

    [TestMethod]
    public void Validate_FindsAMeldingWithoutAnOffsetPerControlPoint()
    {
        var melding = new ZoneMelding { Id = "hydro-core", PerimeterSet = "Hydro_Core_Melding_02", ControlPoints = [Vector3.Zero, Vector3.One], Offsets = [Vector3.Zero] };

        var problems = ContentValidator.Validate(new ZoneContent(448) { Meldings = [melding] }, []);

        Assert.HasCount(1, problems);
    }

    [TestMethod]
    public void Validate_FindsARegionWithATableThatDoesntExist()
    {
        var region = new SpawnRegion { Id = "ne-brontodon-01", Table = "brontodon-heard", Shape = new CircleShape { Center = Vector3.Zero } };

        var problems = ContentValidator.Validate(new ZoneContent(448) { Regions = [region] }, ["brontodon-herd"]);

        Assert.HasCount(1, problems);
        StringAssert.Contains(problems[0].ToString(), "zone 448 regions ne-brontodon-01: spawn table brontodon-heard");
    }

    [TestMethod]
    public void Validate_FindsAPolygonWithoutAnInside()
    {
        var line = new PolygonShape { Points = [Vector3.Zero, new Vector3(10, 0, 0), new Vector3(20, 0, 0)] };
        var region = new SpawnRegion { Id = "ne-brontodon-01", Table = "brontodon-herd", Shape = line };

        var problems = ContentValidator.Validate(new ZoneContent(448) { Regions = [region, region with { Id = "ne-brontodon-02", Shape = new PolygonShape { Points = [Vector3.Zero, Vector3.One] } }] }, ["brontodon-herd"]);

        Assert.HasCount(2, problems);
        StringAssert.Contains(problems[0].Message, "on a line");
        StringAssert.Contains(problems[1].Message, "at least 3 points");
    }

    [TestMethod]
    public void Validate_FindsAnActivationRadiusOfZero()
    {
        var region = new SpawnRegion { Id = "ne-brontodon-01", Table = "brontodon-herd", Shape = new CircleShape { Center = Vector3.Zero }, ActivationRadius = 0 };
        var settings = new ZoneSettings { Population = new PopulationSettings { ActivationRadius = 0 } };

        var problems = ContentValidator.Validate(new ZoneContent(448) { Settings = settings, Regions = [region] }, ["brontodon-herd"]);

        Assert.HasCount(2, problems);
        StringAssert.Contains(problems[0].ToString(), "zone 448 zone: population.activation_radius");
    }

    [TestMethod]
    public void Validate_FindsATableUsedInTwoFiles()
    {
        var table = new SpawnTable { Id = "brontodon-herd", RespawnSeconds = new NumberRange(60, 60), Groups = [new SpawnGroup { Members = [new SpawnMember { Monster = 582 }] }] };

        var problems = ContentValidator.Validate([new SpawnTableFile("brontodons", [table]), new SpawnTableFile("sertao", [table])]);

        Assert.HasCount(1, problems);
        StringAssert.Contains(problems[0].ToString(), "brontodons.json, sertao.json");
    }

    [TestMethod]
    public void Validate_FindsATableThatCanSpawnNobody()
    {
        var table = new SpawnTable
        {
            Id = "brontodon-herd",
            RespawnSeconds = new NumberRange(600, 300),
            Groups = [new SpawnGroup { Members = [new SpawnMember { Monster = 633, Count = new NumberRange(0, 2) }] }, new SpawnGroup { Weight = 0, Members = [new SpawnMember { Monster = 582 }] }],
        };

        var problems = ContentValidator.Validate([new SpawnTableFile("brontodons", [table])]);

        Assert.HasCount(3, problems);
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
