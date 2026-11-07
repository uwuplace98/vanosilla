using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.LandOfDeath;

[MessageType("land-of-death.start")]
public class LandOfDeathStartMessage : IMessage
{
}