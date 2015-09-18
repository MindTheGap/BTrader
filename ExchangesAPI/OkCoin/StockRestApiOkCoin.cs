using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTraderWPF.Utilities;

namespace BTraderWPF.ExchangesAPI.OkCoin
{
  public class StockRestApiOkCoin
  {
    public string UrlPrex { get; set; }
    public string ApiKey { get; set; }
    public string SecretKey { get; set; }

    #region URL Api Consts

    private const String TickerUrl = "/api/v1/ticker.do";
    private const String DepthUrl = "/api/v1/depth.do";
    private const String TradesUrl = "/api/v1/trades.do";
    private const String KlineUrl = "/api/v1/kline.do";
    private const String UserinfoUrl = "/api/v1/userinfo.do";
    private const String TradeUrl = "/api/v1/trade.do";
    private const String BatchTradeUrl = "/api/v1/batch_trade.do";
    private const String CancelOrderUrl = "/api/v1/cancel_order.do";
    private const String OrderInfoUrl = "/api/v1/order_info.do";
    private const String OrdersInfoUrl = "/api/v1/orders_info.do";
    private const String OrderHistoryUrl = "/api/v1/order_history.do";
    private const String WithdrawUrl = "/api/v1/withdraw.do";
    private const String CancelWithdrawRul = "/api/v1/cancel_withdraw.do";
    private const String OrderFeeUrl = "/api/v1/order_fee.do";
    private const String LendDepthUrl = "/api/v1/lend_depth.do";
    private const String BorrowMoneyUrl = "/api/v1/borrow_money.do";
    private const String CancelBorrowUrl = "/api/v1/cancel_borrow.do";
    private const String BorrowOrderInfo = "/api/v1/borrow_order_info.do";
    private const String RepaymentUrl = "/api/v1/repayment.do";
    private const String UnrepaymentsInfoUrl = "/api/v1/unrepayments_info.do";
    private const String AccountRecordsUrl = "/api/v1/account_records.do";
    private const String TradeHistoryUrl = "/api/v1/trade_history.do";

    #endregion URL Api Consts

    public StockRestApiOkCoin(String urlPrex)
    {
      UrlPrex = urlPrex;
    }

    public StockRestApiOkCoin(string urlPrex, string apiKey, string secretKey)
    {
      UrlPrex = urlPrex;
      ApiKey = apiKey;
      SecretKey = secretKey;
    }

    public string GetTradeHistory(String symbol, String since)
    {
      var paras = new Dictionary<String, String> { { "api_key", ApiKey } };
      if (!string.IsNullOrEmpty(symbol)) paras.Add("symbol", symbol);
      if (!string.IsNullOrEmpty(since)) paras.Add("since", since);

      var sign = FetchOkCoin.BuildMysignV1(paras, SecretKey);
      paras.Add("sign", sign);

      var result = Utils.RequestHttpPost(UrlPrex, TradeHistoryUrl, paras);

      return result;
    }

    public string CancelOrder(String symbol, String orderId)
    {
      var paras = new Dictionary<String, String> {{"api_key", ApiKey}};
      if (!string.IsNullOrEmpty(symbol)) paras.Add("symbol", symbol);
      if (!string.IsNullOrEmpty(orderId)) paras.Add("order_id", orderId);

      var sign = FetchOkCoin.BuildMysignV1(paras, SecretKey);
      paras.Add("sign", sign);

      var result = Utils.RequestHttpPost(UrlPrex, CancelOrderUrl, paras);

      return result;
    }
  }
}
