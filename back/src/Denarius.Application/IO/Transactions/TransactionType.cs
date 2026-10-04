using System.Text.Json.Serialization;

namespace Denarius.Application.IO.Transactions;

/// <summary>Written <c>all</c>, <c>in</c> and <c>out</c> in JSON, as the API reads it from the query string.</summary>
[JsonConverter(typeof(JsonStringEnumConverter<TransactionType>))]
public enum TransactionType
{
    [JsonStringEnumMemberName("all")]
    All,
    [JsonStringEnumMemberName("in")]
    In,
    [JsonStringEnumMemberName("out")]
    Out
}
