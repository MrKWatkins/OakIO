namespace MrKWatkins.OakIO.Resources;

/// <summary>
/// The traversal order of a rectangular resource's stored elements.
/// </summary>
public enum ResourceDataOrder
{
    /// <summary>
    /// Left to right within each row, then top to bottom.
    /// </summary>
    RowMajor,
    /// <summary>
    /// Top to bottom within each column, then left to right.
    /// </summary>
    ColumnMajor
}