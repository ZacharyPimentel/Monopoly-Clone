using api.DTO.Entity;
using TypeGen.Core.TypeAnnotations;
namespace api.DTO.Websocket;
[ExportTsInterface]
public class SocketEventTradeCreate
{
    public required PlayerTradeOffer PlayerOne { get; set; }
    public required PlayerTradeOffer PlayerTwo { get; set; }
}