using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace StarWars.Model
{
    public interface IPagedResponse
    {
        int Page { get; }
        int PageSize { get; }
        int Total { get; }
    }

    public interface IPagedResponse<T> : IPagedResponse
    {
        [JsonPropertyOrder(100)]
        IEnumerable<T> Items { get; }
    }

    public class PagedResponse<T> : IPagedResponse<T>
    {
        public int Page { get; }
        public int PageSize { get; }
        public int Total { get; }
        public IEnumerable<T> Items { get; }

        public PagedResponse(int page, int pageSize, int total, IEnumerable<T> items = null)
        {
            Page = page;
            PageSize = pageSize;
            Total = total;
            Items = items ?? Array.Empty<T>();
        }
    }
}
