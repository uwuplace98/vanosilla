using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.TalentArena;

[MessageType("talent-arena.start")]
public class TalentArenaStartMessage : IMessage
{
}