namespace ColumnStore.Core.Operators;

/// <summary>Position list backed by a sorted array. Works with any other position list.</summary>
public sealed class ArrayPositionList : IPositionList
{
    private readonly int[] _positions;

    public ArrayPositionList(int[] positions)
    {
        for (var i = 1; i < positions.Length; i++)
        {
            if (positions[i] <= positions[i - 1])
            {
                throw new ArgumentException("Positions must be strictly ascending.", nameof(positions));
            }
        }

        _positions = (int[])positions.Clone();
    }

    public int Count => _positions.Length;

    public int[] ToArray() => (int[])_positions.Clone();

    public IPositionList And(IPositionList other)
    {
        var right = other.ToArray();
        var result = new List<int>();
        int i = 0, j = 0;
        while (i < _positions.Length && j < right.Length)
        {
            if (_positions[i] == right[j])
            {
                result.Add(_positions[i]);
                i++;
                j++;
            }
            else if (_positions[i] < right[j])
            {
                i++;
            }
            else
            {
                j++;
            }
        }

        return new ArrayPositionList(result.ToArray());
    }
}
