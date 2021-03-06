﻿using Newtonsoft.Json.Linq;
using System.Net.Http;
using ShopifySharp.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ShopifySharp.Infrastructure;
using Newtonsoft.Json;
using System.Net;

namespace ShopifySharp
{
    /// <summary>
    /// A service for using or manipulating Shopify's Graph API.
    /// </summary>
    public class GraphService : ShopifyService
    {
        /// <summary>
        /// Creates a new instance of <see cref="GraphService" />.
        /// </summary>
        /// <param name="myShopifyUrl">The shop's *.myshopify.com URL.</param>
        /// <param name="shopAccessToken">An API access token for the shop.</param>
        public GraphService(string myShopifyUrl, string shopAccessToken) : base(myShopifyUrl, shopAccessToken) { }        

        /// <summary>
        /// Executes a Graph API Call.
        /// </summary>
        /// <param name="body">The query you would like to execute. Please see documentation for formatting.</param>
        /// <returns>A JToken containing the data from the request.</returns>
        public virtual async Task<JToken> PostAsync(string body)
        {
            var req = PrepareRequest("graphql.json");

            var content = new StringContent(body, Encoding.UTF8, "application/graphql");

            return await SendAsync(req, content);
        }

        /// <summary>
        /// Executes a Graph API Call.
        /// </summary>
        /// <param name="body">The query you would like to execute, as a JToken. Please see documentation for formatting.</param>
        /// <returns>A JToken containing the data from the request.</returns>
        public virtual async Task<JToken> PostAsync(JToken body)
        {
            var req = PrepareRequest("graphql.json");

            var content = new StringContent(JsonConvert.SerializeObject(body), Encoding.UTF8, "application/json");

            return await SendAsync(req, content);
        }

        /// <summary>
        /// Content agnostic way to send the request, regardless of Json or GraphQL.
        /// </summary>
        /// <param name="req">The RequestUri.</param>
        /// <param name="content">The HttpContent, be it GraphQL or Json.</param>
        /// <returns>A JToken containing the data from the request.</returns>
        private async Task<JToken> SendAsync(RequestUri req, HttpContent content)
        {
            JToken response = await ExecuteRequestAsync(req, HttpMethod.Post, content);

            await CheckForErrorsAsync(response);

            return response["data"];
        }

        /// <summary>
        /// Since Graph API Errors come back with error code 200, checking for them in a way similar to the REST API doesn't work well without potentially throwing unnecessary errors. This loses the requestId, but otherwise is capable of passing along the message.
        /// </summary>
        /// <param name="response">The JToken response from ExecuteRequestAsync.</param>
        /// <returns>Task.</returns>
        private async Task CheckForErrorsAsync(JToken response)
        {
            if (response["errors"] != null)
            {
                var errorList = new List<string>();
                foreach (var error in response["errors"])
                {
                    errorList.Add(error["message"].ToString());
                }

                var message = response["errors"].FirstOrDefault()["message"].ToString();

                var errors = new Dictionary<string, IEnumerable<string>>()
                {
                    {"Error", errorList}
                };

                throw new ShopifyException(HttpStatusCode.OK, errors, message, JsonConvert.SerializeObject(response), "");
            }
        }
    }
}
