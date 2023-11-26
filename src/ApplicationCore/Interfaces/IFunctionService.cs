using Microsoft.eShopWeb.ApplicationCore.Entities.OrderAggregate;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Microsoft.eShopWeb.ApplicationCore.Interfaces;

public interface IFunctionService
{
    Task<bool> ReserveOrderItems(IEnumerable<OrderItem> orderItems);
}
