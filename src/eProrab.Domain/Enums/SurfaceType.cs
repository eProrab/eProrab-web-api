namespace eProrab.Domain.Enums;

/// <summary>
/// Surface / building component this finish material is intended for.
/// Lets the renovation calculator automatically assign catalog items
/// to the correct room surface without guessing from keywords.
/// </summary>
public enum SurfaceType
{
    /// <summary>Not specified / generic material.</summary>
    None = 0,

    /// <summary>Wall covering: paint, wallpaper, tile, stucco …</summary>
    Wall = 1,

    /// <summary>Floor covering: laminate, parquet, ceramogranite, screed …</summary>
    Floor = 2,

    /// <summary>Ceiling: drywall, suspended ceiling, paint …</summary>
    Ceiling = 3,

    /// <summary>Door: interior door, frame …</summary>
    Door = 4,

    /// <summary>Window: PVC/aluminium window, sill …</summary>
    Window = 5,

    /// <summary>Electrical: cable, panel, sockets, luminaires …</summary>
    Electric = 6,

    /// <summary>Plumbing: pipes, boiler, sanitary ware …</summary>
    Plumbing = 7,
}
