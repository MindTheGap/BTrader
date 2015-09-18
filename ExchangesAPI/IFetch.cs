using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BTraderWPF.DBs;

namespace BTraderWPF.ExchangesAPI
{
  interface IFetch
  {
    List<Trade> GetTradeHistory(Symbol symbol, string startingTradeId);

    void UpdateOrderInfo(Trade trade);

    void UpdateOrdersInfo(List<Trade> trade);

    void PlaceOrder(Trade trade);
    bool CancelOrder(Trade trade);

    void UpdateUserAccountInfo(Account account);
  }
}
