using MultiServerManager.Core;

namespace MultiServerManager.Utils.Extensions;

public static class TileProviderTypeExt
{
    public static string GetArg(this TileProviderType type) => type switch
    {
        TileProviderType.Stock => "",
        TileProviderType.HeapTile => "-heaptile",
        TileProviderType.Constileation => "-constileation",
        _ => ""
    };
}