namespace Whitebird.Domain.Features.Common;

[AttributeUsage(AttributeTargets.Property, AllowMultiple = false)]
public class ColumnAttribute : Attribute
{
    public string? Name { get; set; }
    public bool Ignore { get; set; }

    public ColumnAttribute() { }

    public ColumnAttribute(string name)
    {
        Name = name;
    }
}