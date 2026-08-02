namespace Shared.Collision.Tagfile.Models;

public record DictionaryTagfile(
    List<TagfileDictObject> Processed,
    List<TagfileDictObject> Raw);
