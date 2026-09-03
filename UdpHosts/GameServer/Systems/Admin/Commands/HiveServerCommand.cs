using System.Globalization;
using System.Text;
using GameServer.Systems.World;

namespace GameServer.Systems.Admin.Commands;

[ServerCommand("Manage aranha hive sites (burrowed population woken by thumping)", "hive add [radius] [burst] | hive list | hive clear", "hive")]
public class HiveServerCommand : ServerCommand
{
    public override void Execute(string[] parameters, ServerCommandContext context)
    {
        var character = context.SourcePlayer?.CharacterEntity;
        if (character == null)
        {
            SourceFeedback("Requires a player character", context);
            return;
        }

        var shard = character.Shard;
        var sub = parameters.Length > 0 ? parameters[0].ToLowerInvariant() : "list";

        switch (sub)
        {
            case "add":
            {
                var radius = 120f;
                var burst = 50;
                if (parameters.Length > 1 && float.TryParse(parameters[1], NumberStyles.Float, CultureInfo.InvariantCulture, out var r))
                {
                    radius = r;
                }

                if (parameters.Length > 2 && int.TryParse(parameters[2], out var b))
                {
                    burst = b;
                }

                var position = character.Position;
                var name = $"hive-{shard.Hives.Sites.Count + 1}";
                shard.Hives.Add(new HiveSite(name, position.X, position.Y, position.Z, radius, burst));
                SourceFeedback($"Hive '{name}' planted at ({position.X:0},{position.Y:0},{position.Z:0}) radius {radius:0}m burst {burst}. Thump nearby to wake it.", context);
                break;
            }

            case "clear":
            {
                var removed = shard.Hives.Clear();
                SourceFeedback($"Removed {removed} hive sites.", context);
                break;
            }

            default:
            {
                var sites = shard.Hives.Sites;
                if (sites.Count == 0)
                {
                    SourceFeedback("No hive sites. Use: hive add [radius] [burst]", context);
                    return;
                }

                var sb = new StringBuilder($"{sites.Count} hive sites:\n");
                foreach (var site in sites)
                {
                    var dx = character.Position.X - site.X;
                    var dy = character.Position.Y - site.Y;
                    var distance = System.MathF.Sqrt((dx * dx) + (dy * dy));
                    sb.AppendLine($"  {site.Name}: ({site.X:0},{site.Y:0}) r={site.Radius:0} burst={site.Burst} — {distance:0}m from you");
                }

                SourceFeedback(sb.ToString(), context);
                break;
            }
        }
    }
}
