using TypeGen.Core.TypeAnnotations;

namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventRulesUpdate
{
    public int? StartingMoney { get; set; }
    public bool? FullSetDoublePropertyRent { get; set; }
}