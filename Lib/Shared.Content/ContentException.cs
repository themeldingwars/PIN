using System;
using System.Globalization;

namespace Shared.Content;

public sealed class ContentException(string path, string message, Exception? inner = null)
    : Exception(string.Create(CultureInfo.InvariantCulture, $"{path}: {message}"), inner)
{
    public string Path { get; } = path;
}
