public static class Fibonacci
{
    private static readonly List<ulong> _fib;

    static Fibonacci()
    {
        _fib = new();
        _fib.Add(0);
        _fib.Add(1);
    }

    public static ulong Calculate(int n)
    {
        if (_fib.Count > n)
            return _fib[n];

        GenerateFib(n);

        return _fib[n];
    }

    private static void GenerateFib(int n)
    {
        for (int i = _fib.Count; i <= n; i++)
        {
            var fib = _fib[i - 2] + _fib[i - 1];
            _fib.Add(fib);
        }
    }

}