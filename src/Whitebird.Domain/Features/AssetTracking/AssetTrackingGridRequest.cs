using Whitebird.Domain.Features.Common;

namespace Whitebird.Domain.Features.AssetTracking;

/// <summary>
/// Grid request for Asset Tracking
/// </summary>
public class AssetTrackingGridRequest : GridRequestBase
{
    /// <summary>
    /// Asset ID (required, passed in path)
    /// </summary>
    public int AssetId { get; set; }
}