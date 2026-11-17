namespace ColumnStore.Core.Queries;

/// <summary>
/// One of the four configurations being compared (row store, plain columns, compressed eager, compressed direct).
/// All four must return identical answers. Money is in cents, distance in hundredths of a mile, dates are day numbers.
/// </summary>
public interface IQueryConfiguration
{
    string Name { get; }

    /// <summary>Bytes read from disk since the last ResetCounters.</summary>
    long BytesRead { get; }

    void ResetCounters();

    /// <summary>Q1: trip count and total fare per payment type.</summary>
    IReadOnlyDictionary<int, (long TripCount, long FareCents)> Q1();

    /// <summary>Q2: trips for one vendor between two days, inclusive.</summary>
    long Q2(int vendorId, int firstDay, int lastDay);

    /// <summary>Q3: total tip and trip count for one payment type with more than minPassengers passengers.</summary>
    (long TipCents, long TripCount) Q3(int paymentType, int minPassengers);

    /// <summary>Q4: total distance per pickup day.</summary>
    IReadOnlyDictionary<int, long> Q4();
}
