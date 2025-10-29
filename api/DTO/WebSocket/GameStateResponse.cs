using System.Text.Json.Serialization;
using api.Entity;
using api.Enumerable;
using TypeGen.Core.TypeAnnotations;

namespace api.DTO.Websocket;

[ExportTsInterface]
public class GameStateResponse
{
    public Game? Game { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<Player>? Players { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<GameLog>? GameLogs { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<BoardSpace>? BoardSpaces { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public IEnumerable<Trade>? Trades { get; set; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public AudioFiles? AudioFile { get; set; }

}