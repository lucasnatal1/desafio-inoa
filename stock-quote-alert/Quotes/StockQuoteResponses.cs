namespace Quotes;

class StockQuoteResponse
{
    public StockQuoteSuccessResponse? Success { get; set; }
    public StockQuoteErrorResponse? Error { get; set; }
}
//reference: https://brapi.dev/docs/acoes/cotacao
class StockQuoteSuccessResponse
{
    public required StockQuoteSeries[] results { get; set; }
    public required string requestedAt { get; set; } //ISO 8601
    public required int took { get; set; }
}

class StockQuoteSeries
{
    public required string requestedSymbol { get; set; }
    public required string symbol { get; set; }
    public required bool changed { get; set; }
    public required StockQuoteSnapshot data { get; set; }
}

class StockQuoteSnapshot
{
    public required string shortName { get; set; }
    public required string longName { get; set; }
    public required string currency { get; set; }
    public required double regularMarketPrice { get; set; }
    public required double regularMarketDayHigh { get; set; }
    public required double regularMarketDayLow { get; set; }
    public required string regularMarketDayRange { get; set; }
    public required double regularMarketChange { get; set; }
    public required double regularMarketChangePercent { get; set; }
    public required string regularMarketTime { get; set; } //ISO 8601
    public required long marketCap { get; set; }
    public required long regularMarketVolume { get; set; }
    public required double regularMarketPreviousClose { get; set; }
    public required double regularMarketOpen { get; set; }
    public required string fiftyTwoWeekRange { get; set; }
    public required double fiftyTwoWeekLow { get; set; }
    public required double fiftyTwoWeekHigh { get; set; }
    public required string logourl { get; set; }
}

class StockQuoteErrorResponse
{
    public required bool error { get; set; }
    public required string message { get; set; }
    public string? code { get; set; }
}

//reference: https://brapi.dev/docs/tickers
class TickerListResponse {
    public required TickerListItem[] results { get; set; }
    public required TickerIndexItem[] indexes { get; set; }
    public required TickerFacets facets { get; set; }
    public required TickerPagination pagination { get; set; }
    public required string requestedAt { get; set; } //ISO 8601
    public required int took { get; set; }
}

class TickerListItem {
  public required string symbol { get; set; }
  public required string name { get; set; }
  public required string longName { get; set; }
  public required string assetType { get; set; }
  public required string subType { get; set; }
  public required string exchange { get; set; }
  public required string currency { get; set; }
  public required string sector { get; set; }
  public required string subsector { get; set; }
  public required bool isActive { get; set; }
  public required string logoUrl { get; set; }
  public required TickerQuoteSummary quote { get; set; }
}

class TickerIndexItem {
  public required string symbol { get; set; }
  public required string name { get; set; }
  public required string exchange { get; set; }
  public required string assetType { get; set; }
}

class TickerFacets {
  public required string[] sectors { get; set; }
  public required string[] subsectors { get; set; }
  public required string[] assetTypes { get; set; }
  public required string[] subTypes { get; set; }
}

class TickerPagination {
  public required int page { get; set; }
  public required int limit { get; set; }
  public required int totalItems { get; set; }
  public required int totalPages { get; set; }
  public required bool hasNextPage { get; set; }
}

class TickerQuoteSummary {
  public required double lastPrice { get; set; }
  public required double changePercent { get; set; }
  public required long volume { get; set; }
  public required long? marketCap { get; set; }
}
