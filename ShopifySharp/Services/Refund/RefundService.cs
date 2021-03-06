using System;
using Newtonsoft.Json.Linq;
using System.Net.Http;
using ShopifySharp.Filters;
using System.Collections.Generic;
using System.Threading.Tasks;
using ShopifySharp.Infrastructure;

namespace ShopifySharp
{
    /// <summary>
    /// A service for creating Shopify Refunds.
    /// </summary>
    public class RefundService : ShopifyService
    {
        /// <summary>
        /// Creates a new instance of <see cref="RefundService" />.
        /// </summary>
        /// <param name="myShopifyUrl">The shop's *.myshopify.com URL.</param>
        /// <param name="shopAccessToken">An API access token for the shop.</param>
        public RefundService(string myShopifyUrl, string shopAccessToken) : base(myShopifyUrl, shopAccessToken) { }

        /// <summary>
        /// Retrieves a list of refunds for an order.
        /// </summary>
        /// <param name="orderId">The id of the order to list orders for.</param>
        /// <returns></returns>
        [Obsolete("This ListAsync method targets a version of Shopify's API which will be deprecated and cease to function in April of 2020. ShopifySharp version 5.0 will be published soon with support for the newer list API. Make sure you update before April of 2020.")]
        public virtual async Task<IEnumerable<Refund>> ListForOrderAsync(long orderId, OrderFilter options = null)
        {
            var req = PrepareRequest($"orders/{orderId}/refunds.json");
            req.QueryParams.Add("customer_id", orderId);

            if (options != null)
            {
                req.QueryParams.AddRange(options.ToParameters());
            }

            return await ExecuteRequestAsync<List<Refund>>(req, HttpMethod.Get, rootElement: "refunds");
        }

        /// <summary>
        /// Retrieves a specific refund.
        /// </summary>
        /// <param name="orderId"></param>
        /// <param name="refundId"></param>
        /// <returns></returns>
        public virtual async Task<Refund> GetAsync(long orderId, long refundId, string fields = null)
        {
            var req = PrepareRequest($"orders/{orderId}/refunds/{refundId}.json");

            if (string.IsNullOrEmpty(fields) == false)
            {
                req.QueryParams.Add("fields", fields);
            }

            return await ExecuteRequestAsync<Refund>(req, HttpMethod.Get, rootElement: "refund");
        }

        /// <summary>
        /// Calculates <see cref="Refund"/> transactions based on line items and shipping.
        /// When you want to create a refund, you should first use the calculate endpoint to generate accurate refund transactions.
        /// Specify the line items that are being refunded, their quantity and restock instructions, and whether you intend to refund shipping costs.
        /// If the restock instructions can't be met—for example, because you try to return more items than have been fulfilled—then the endpoint returns modified restock instructions.
        /// You can then use the response in the body of the request to create the actual refund.
        /// The response includes a transactions object with "kind": "suggested_refund", which must to be changed to "kind" : "refund" for the refund to be accepted.
        /// </summary>
        public virtual async Task<Refund> CalculateAsync(long orderId, Refund options = null)
        {
            var req = PrepareRequest($"orders/{orderId}/refunds/calculate.json");
            var content = new JsonContent(new { refund = options ?? new Refund() });
            var result = await ExecuteRequestAsync<Refund>(req, HttpMethod.Post, content, "refund");

            return result;
        }

        /// <summary>
        /// Creates a <see cref="Refund"/>. Use the calculate endpoint to produce the transactions to submit.
        /// </summary>
        public virtual async Task<Refund> RefundAsync(long orderId, Refund options = null)
        {
            var req = PrepareRequest($"orders/{orderId}/refunds.json");
            var content = new JsonContent(new { refund = options ?? new Refund() });
            var result = await ExecuteRequestAsync<Refund>(req, HttpMethod.Post, content, "refund");

            return result;
        }
    }
}
