namespace Whitebird.App.Features.Reports.Draw;

/// <summary>
/// Interface for drawing reports (Excel/PDF)
/// </summary>
public interface IDrawReport<T>
{
    Task<byte[]> GenerateAsync(IEnumerable<T> data, string format);
}