using System.Collections;
using System.Collections.Generic;

public class IntStringMap<T1, T2>
{
    private Dictionary<T1, T2> _intToString = new Dictionary<T1, T2>();
    private Dictionary<T2, T1> _stringToInt = new Dictionary<T2, T1>();

    public IntStringMap()
    {
        this.InttoString = new Indexer<T1, T2>(_intToString);
        this.StringtoInt = new Indexer<T2, T1>(_stringToInt);
    }

    public class Indexer<T3, T4> : IEnumerable<KeyValuePair<T3, T4>>
    {
        private Dictionary<T3, T4> _dictionary;
        public Indexer(Dictionary<T3, T4> dictionary)
        {
            _dictionary = dictionary;
        }
        public T4 this[T3 index]
        {
            get { return _dictionary[index]; }
            set { _dictionary[index] = value; }
        }

        public IEnumerator<KeyValuePair<T3, T4>> GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _dictionary.GetEnumerator();
        }

        public int Count => _dictionary.Count;
    }

    public void Add(T1 t1, T2 t2)
    {
        _intToString.Add(t1, t2);
        _stringToInt.Add(t2, t1);
    }

    public Indexer<T1, T2> InttoString { get; private set; }
    public Indexer<T2, T1> StringtoInt { get; private set; }
}