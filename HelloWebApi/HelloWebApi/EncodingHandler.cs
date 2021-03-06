﻿using System.Net.Http;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace HelloWebApi
{
    public class EncodingHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var response = await base.SendAsync(request, cancellationToken);
            try
            {
                var schema = new EncodingSchema();
                var encoder = schema.GetEncoder(response.RequestMessage.Headers.AcceptEncoding);
                if (encoder != null)
                {
                    response.Content = new EncodedContent(response.Content, encoder);
                    response.Content.Headers.ContentEncoding.Add(schema.ContentEncoding);
                }
            }
            catch (NegotiationFailedException)
            {
                return request.CreateResponse(HttpStatusCode.NotAcceptable);
            }
            return response;
        }
    }
}