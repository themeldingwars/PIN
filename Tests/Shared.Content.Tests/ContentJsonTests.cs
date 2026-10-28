using System.Numerics;
using Shared.Content.Zones;
using Shared.Content.Zones.Encounters;

namespace Shared.Content.Tests;

[TestClass]
public sealed class ContentJsonTests
{
    private static readonly ZoneDeployable Terminal = new()
    {
        Id = "deployable-2",
        Type = 820,
        Position = new Vector3(177.09631f, 245.79558f, 472.0671f),
        Orientation = new Quaternion(0, 0, 0.9955702f, -0.09402133f),
        Source = "2016gameplay.71776119061217430",
    };

    [TestMethod]
    public void Write_PutsVectorsOnOneLineAndSortsByNumber()
    {
        var text = ContentJson.Write(ContentKinds.Deployables, [Terminal with { Id = "deployable-10", Source = null, Edited = true, Notes = "Moved off the cliff" }, Terminal]);

        Assert.AreEqual(
            """
            {
              "$schema": "../../schemas/deployables.schema.json",
              "deployables": [
                {
                  "id": "deployable-2",
                  "type": 820,
                  "position": [177.09631, 245.79558, 472.0671],
                  "orientation": [0, 0, 0.9955702, -0.09402133],
                  "source": "2016gameplay.71776119061217430"
                },
                {
                  "id": "deployable-10",
                  "type": 820,
                  "position": [177.09631, 245.79558, 472.0671],
                  "orientation": [0, 0, 0.9955702, -0.09402133],
                  "edited": true,
                  "notes": "Moved off the cliff"
                }
              ]
            }

            """.ReplaceLineEndings("\n"),
            text);
    }

    [TestMethod]
    public void Read_GivesBackWhatWasWritten()
    {
        var outpost = new ZoneOutpost
        {
            Id = 17,
            NameTextId = 151333,
            Position = new Vector3(172.17278f, 254.14604f, 479.88083f),
            Radius = 485,
            SpawnPoints = [new OutpostSpawnPoint { Position = new Vector3(176.65f, 250.13f, 491.94f), AimDirection = new Vector3(-0.6929f, 0.7208f, -0.0162f) }],
        };

        var text = ContentJson.Write(ContentKinds.Outposts, [outpost]);
        var read = ContentJson.Read(ContentKinds.Outposts, text, "outposts.json");

        Assert.AreEqual(text, ContentJson.Write(ContentKinds.Outposts, read));
    }

    [TestMethod]
    public void Read_GivesBackEncountersOfEveryType()
    {
        var pose = new Pose { Position = new Vector3(-667.259f, -329.08145f, 442.6372f) };
        ZoneEncounter[] encounters =
        [
            new ZoneLgvRace { Id = "copacabana-trans-hub", LeaderboardId = 1, Start = pose, Finish = pose, Terminal = pose },
            new ZoneMeldingRepulsor
            {
                Id = "hydro-core-melding-02",
                PerimeterSet = "Hydro_Core_Melding_02",
                Repulsor = new PlacedObject { Type = 1290, Position = Vector3.Zero },
                Terminal = new PlacedObject { Type = 1291, Position = Vector3.One },
                MeldingPosition = new MeldingPosition { ControlPointIndex = 3, Position = Vector3.One },
            },
        ];

        var text = ContentJson.Write(ContentKinds.Encounters, encounters);
        var read = ContentJson.Read(ContentKinds.Encounters, text, "encounters.json");

        StringAssert.Contains(text, "\"type\": \"melding-repulsor\"");
        Assert.IsInstanceOfType<ZoneMeldingRepulsor>(read[1]);
        Assert.AreEqual(text, ContentJson.Write(ContentKinds.Encounters, read));
    }

    [TestMethod]
    public void Read_FindsTheEncounterTypeAfterOtherFields()
    {
        const string text = """{ "encounters": [ { "id": "a", "start": { "position": [0, 0, 0] }, "finish": { "position": [0, 0, 0] }, "terminal": { "position": [0, 0, 0] }, "type": "lgv-race" } ] }""";

        var read = ContentJson.Read(ContentKinds.Encounters, text, "encounters.json");

        Assert.IsInstanceOfType<ZoneLgvRace>(read[0]);
    }

    [TestMethod]
    [DataRow("""{ "encounters": [ { "type": "brontodon-hunt", "id": "a" } ] }""")]
    [DataRow("""{ "encounters": [ { "id": "a" } ] }""")]
    public void Read_RejectsAnUnknownOrMissingEncounterType(string text)
    {
        Assert.ThrowsExactly<ContentException>(() => ContentJson.Read(ContentKinds.Encounters, text, "encounters.json"));
    }

    [TestMethod]
    public void Read_RejectsAnUnknownField()
    {
        const string text = """{ "deployables": [ { "id": "a", "type": 1, "position": [0, 0, 0], "postion": [1, 1, 1] } ] }""";

        var e = Assert.ThrowsExactly<ContentException>(() => ContentJson.Read(ContentKinds.Deployables, text, "deployables.json"));
        StringAssert.Contains(e.Message, "postion");
    }

    [TestMethod]
    public void Read_RejectsAListUnderAnotherName()
    {
        const string text = """{ "deployable": [] }""";

        Assert.ThrowsExactly<ContentException>(() => ContentJson.Read(ContentKinds.Deployables, text, "deployables.json"));
    }

    [TestMethod]
    public void Read_RejectsAVectorWithTwoValues()
    {
        const string text = """{ "deployables": [ { "id": "a", "type": 1, "position": [0, 0] } ] }""";

        Assert.ThrowsExactly<ContentException>(() => ContentJson.Read(ContentKinds.Deployables, text, "deployables.json"));
    }

    [TestMethod]
    public void Read_RejectsAMissingRequiredField()
    {
        const string text = """{ "deployables": [ { "id": "a", "position": [0, 0, 0] } ] }""";

        Assert.ThrowsExactly<ContentException>(() => ContentJson.Read(ContentKinds.Deployables, text, "deployables.json"));
    }
}
