using System;
using System.Collections.Generic;
using System.Linq;
using BTraderWPF.DBs;
using BTraderWPF.Utilities;

namespace BTraderWPF.ExchangesAPI.OkCoin
{
  public class FetchOkCoin : IFetch
  {
    private const string ApiKey = @"56891031-b6f6-4afa-8d7a-c82bee184eec";
    private const string SecretKey = @"20ED5D962A53BD107F3641EFA3AEB093";
    private const string UrlPrex = "https://www.okcoin.com";
    private readonly StockRestApiOkCoin _stockRestApi = new StockRestApiOkCoin(UrlPrex, ApiKey, SecretKey);

    public List<Trade> GetTradeHistory(Symbol symbol, string startingTradeId = "0")
    {
      var result = new List<Trade>();

      var trades = _stockRestApi.GetTradeHistory(symbol.Code, startingTradeId);

      return result;
    }

    public void UpdateOrderInfo(Trade trade)
    {
      throw new NotImplementedException();
    }

    public void UpdateOrdersInfo(List<Trade> trade)
    {
      throw new NotImplementedException();
    }

    public void PlaceOrder(Trade trade)
    {
      throw new NotImplementedException();
    }

    public bool CancelOrder(Trade trade)
    {
      if (!trade.TradeId.HasValue) return false;

      var result = _stockRestApi.CancelOrder(trade.Symbol.Code, trade.TradeId.Value.ToString("D"));
      return result == "true";
    }

    public void UpdateUserAccountInfo(Account account)
    {
      throw new NotImplementedException();
    }

    #region Static Functions

    public static String BuildMysignV1(Dictionary<String, String> parametersDic, String secretKey)
    {
      var prestr = CreateLinkString(parametersDic) + "&secret_key=" + secretKey;
      return Utils.Calculate32UpperMd5Hash(prestr);
    }

    public static String CreateLinkString(Dictionary<String, String> parametersDic)
    {
      var keys = new List<string>(parametersDic.Keys);

      var paraSort = from objDic in parametersDic orderby objDic.Key ascending select objDic;
      var prestr = "";
      var i = 0;
      foreach (var kvp in paraSort)
      {
        if (i == keys.Count() - 1)
        {
          prestr = prestr + kvp.Key + "=" + kvp.Value;
        }
        else
        {
          prestr = prestr + kvp.Key + "=" + kvp.Value + "&";
        }
        i++;
        if (i == keys.Count())
        {
          break;
        }
      }

      return prestr;
    }

    #endregion Static Functions
  }
}
