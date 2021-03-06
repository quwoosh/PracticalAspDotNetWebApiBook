﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.IO.Compression;
using System.Net.Http.Headers;

namespace HelloWebApi
{
    public class EncodingSchema
    {
        private const string IDENTITY = "identity";
        private IDictionary<string, Func<Stream, Stream>> supported = new Dictionary<string, Func<Stream, Stream>>(StringComparer.OrdinalIgnoreCase);
        public string ContentEncoding { get; private set; }
        public EncodingSchema()
        {
            supported.Add("gzip", GetGZipStream);
            supported.Add("deflate", GetDeflateStream);
        }
        public Stream GetGZipStream(Stream stream)
        {
            return new GZipStream(stream, CompressionMode.Compress, true);
        }
        public Stream GetDeflateStream(Stream stream)
        {
            return new DeflateStream(stream, CompressionMode.Compress, true);
        }

        private Func<Stream,Stream> GetStreamForSchema(string schema)
        {
            if(supported.ContainsKey(schema))
            {
                ContentEncoding = schema.ToLowerInvariant();
                return supported[schema];
            }
            throw new InvalidOperationException(String.Format("Unsupported encoding schema {0}", schema));
        }
        public Func<Stream, Stream> GetEncoder(HttpHeaderValueCollection<StringWithQualityHeaderValue> list)
        {
            if (list != null && list.Count() > 0)
            {
                var headerValue = list.OrderByDescending(e => e.Quality ?? 1.0D)
                        .Where(e => !e.Quality.HasValue ||
                                    e.Quality.Value > 0.0D)
                        .FirstOrDefault(e => supported.Keys
                                .Contains(e.Value, StringComparer.OrdinalIgnoreCase));

                if (headerValue != null)
                    return GetStreamForSchema(headerValue.Value);

                if (list.Any(e => e.Value == "*" &&
                        (!e.Quality.HasValue || e.Quality.Value > 0.0D)))
                {
                    var encoding = supported.Keys.Where(se =>
                                        !list.Any(e =>
                                                    e.Value.Equals(se, StringComparison.OrdinalIgnoreCase) &&
                                                        e.Quality.HasValue &&
                                                            e.Quality.Value == 0.0D))
                                                                .FirstOrDefault();
                    if (encoding != null)
                        return GetStreamForSchema(encoding);
                }

                if (list.Any(e => e.Value.Equals(IDENTITY, StringComparison.OrdinalIgnoreCase) &&
                        e.Quality.HasValue && e.Quality.Value == 0.0D))
                {

                    throw new NegotiationFailedException();
                }

                // Case 4: Client is not willing to accept one of the encodings 
                // we support and is not willing to accept identity
                if (list.Any(e => e.Value == "*" &&
                        (e.Quality.HasValue || e.Quality.Value == 0.0D)))
                {
                    if (!list.Any(e => e.Value.Equals(IDENTITY, StringComparison.OrdinalIgnoreCase)))
                        throw new NegotiationFailedException();
                }

            }
            return null;
        }
    }
    public class NegotiationFailedException : ApplicationException { }
}