using System.Collections.Generic;

namespace BTraderWPF.Utilities
{
  public class DictionaryPlus<TKey, TValue> : Dictionary<TKey, TValue>
  {
    public DictionaryPlus()
    {

    }

    public DictionaryPlus(Dictionary<TKey, TValue> dic)
      : base(dic)
    {

    }

    public DictionaryPlus(int n)
      : base(n)
    {

    }

    public new TValue this[TKey key]
    {
      get
      {
        try
        {
          return base[key];
        }
        catch (KeyNotFoundException keyNotFoundException)
        {
          throw new KeyNotFoundException("The given key (" + key + ") was not found!");
        }
      }
      set
      {
        base[key] = value;
      }
    }
  }
}
