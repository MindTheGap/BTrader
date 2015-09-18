using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;
using BTraderWPF.DBs;

namespace BTraderWPF.Utilities
{
  public static class Utils
  {
    private static readonly char[] HexDigits = { '0', '1', '2', '3', '4', '5', '6', '7', '8', '9', 'A', 'B', 'C', 'D', 'E', 'F' };

    public static IEnumerable<string> EnumerateAllLines(string filePath)
    {
      if (!File.Exists(filePath))
      {
        throw new ArgumentException(string.Format("filePath ({0}) does not exist!", filePath));
      }

      using (var fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
      using (var reader = new StreamReader(fileStream))
      {
        while (!reader.EndOfStream)
        {
          yield return reader.ReadLine();          
        }
      }
    }

    public static DictionaryPlus<string, string> ArgsToDic(string spacedArgs)
    {
      while (spacedArgs.Contains("  "))
      {
        spacedArgs = spacedArgs.Replace("  ", " ");
      }

      string[] args = spacedArgs.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
      return ArgsToDic(args);
    }

    public static DictionaryPlus<string, string> ArgsToDic(string[] args)
    {
      var dic = new DictionaryPlus<string, string>();
      for (int i = 0; i < args.Length; i++)
      {
        if (i == 0 && args[0].ToLower().StartsWith("mapp"))
        {
          continue;
        }

        if (args[i] != "-h" && args[i] != "-help" && args[i] != "/?")
        {
          if (args[i].StartsWith("---"))
          {
            var endIndex = i + 1;
            var strToFindList = new List<string>();
            while (true)
            {
              if (endIndex >= args.Length)
              {
                throw new ArgumentException("Something is wrong with the '---' argument you provided");
              }

              if (args[endIndex] == "---")
              {
                dic[args[i]] = strToFindList.Aggregate((s1, s2) => s1 + " " + s2);

                i = endIndex;
                break;
              }

              strToFindList.Add(args[endIndex]);
              endIndex++;
            }
          }
          else if (args[i].StartsWith("--"))
          {
            dic[args[i]] = null;
          }
          else
          {
            dic[args[i]] = args[i + 1];
            i++;
          }
        }
      }
      return dic;
    }

    public static string DicToStr(DictionaryPlus<string, string> dic)
    {
      return DicToStr(dic, " ");
    }

    public static string DicToStr(DictionaryPlus<string, string> dic, string separator)
    {
      string str = "";

      foreach (var key in dic.Keys)
      {
        if (!key.StartsWith("-") && key.CompareTo(" ") != 0)
        {
          continue;
        }

        str += separator + key;
        if (key.StartsWith("---"))
        {
          str += separator + dic[key] + separator + "---";
        }
        else if (!key.StartsWith("--"))
        {
          if (dic[key].Contains("\\") && !dic[key].StartsWith("\"") && key.CompareTo(" ") != 0)
          {
            str += separator + "\"" + dic[key] + "\"";
          }
          else
          {
            str += separator + dic[key];
          }
        }
      }

      return str;
    }

    public static void ShowMessageBoxSafely(string message)
    {
      if (!Dispatcher.CurrentDispatcher.CheckAccess())
      {
        Dispatcher.CurrentDispatcher.Invoke(() => MessageBox.Show(message));
        return;
      }

      MessageBox.Show(message);
    }

    public static String Calculate32UpperMd5Hash(String str)
    {
      if (str == null || str.Trim().Length == 0) return string.Empty;
      
      var bytes = Encoding.Default.GetBytes(str);
      var md = new MD5CryptoServiceProvider();
      bytes = md.ComputeHash(bytes);
      var sb = new StringBuilder();
      foreach (byte b in bytes)
      {
        sb.Append(HexDigits[(b & 0xf0) >> 4] + "" + HexDigits[b & 0xf]);
      }
      return sb.ToString();
    }

    public static string RequestHttpGet(String urlPrex, String url, String param)
    {
      string responseContent;
      url = urlPrex + url;
      if (!string.IsNullOrEmpty(param))
      {
        url += "?" + param;
      }
      var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
      httpWebRequest.Method = "GET";
      using (var httpWebResponse = (HttpWebResponse)httpWebRequest.GetResponse())
      {
        var responseStream = httpWebResponse.GetResponseStream();
        if (responseStream == null) throw new Exception("web response stream is null!");

        using (var streamReader = new StreamReader(responseStream))
        {
          responseContent = streamReader.ReadToEnd();
        }
      }

      return responseContent;
    }

    public static string RequestHttpPost(String urlPrex, String url, Dictionary<String, String> paras)
    {
      var responseContent = string.Empty;
      url = urlPrex + url;
      var httpWebRequest = (HttpWebRequest)WebRequest.Create(url);
      httpWebRequest.Method = "POST";
      httpWebRequest.ContentType = "application/x-www-form-urlencoded";
      if (paras != null && paras.Any())
      {
        var buffer = new StringBuilder();
        var i = 0;
        foreach (var key in paras.Keys)
        {
          buffer.AppendFormat(i > 0 ? "&{0}={1}" : "{0}={1}", key, paras[key]);
          i++;
        }
        var btBodys = Encoding.UTF8.GetBytes(buffer.ToString());
        httpWebRequest.ContentLength = btBodys.Length;
        httpWebRequest.GetRequestStream().Write(btBodys, 0, btBodys.Length);
      }
      using (var httpWebResponse = (HttpWebResponse) httpWebRequest.GetResponse())
      {
        var responseStream = httpWebResponse.GetResponseStream();
        if (responseStream != null)
        {
          using (var streamReader = new StreamReader(responseStream))
          {
            responseContent = streamReader.ReadToEnd();
          }
        }
      }

      return responseContent;
    }
  }
}
