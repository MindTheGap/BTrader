using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BTraderWPF.Utilities
{
  public static class Extensions
  {
    public static string YMD(this DateTime date)
    {
      return date.ToString("yyyy-MM-dd");
    }

    public static int GetNthIndex(this string s, char t, int n)
    {
      int count = 0;
      for (int i = 0; i < s.Length; i++)
      {
        if (s[i] == t)
        {
          count++;
          if (count == n)
          {
            return i;
          }
        }
      }
      return -1;
    }
  }
}
