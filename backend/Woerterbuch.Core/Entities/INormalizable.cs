namespace Woerterbuch.Core.Entities;

/// <summary>Entity with normalized search columns (filled in before saving).</summary>
public interface INormalizable
{
    void Normalize();
}
