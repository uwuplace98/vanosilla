using PhoenixLib.ServiceBus;
using PhoenixLib.ServiceBus.Routing;

namespace WingsAPI.Communication.Icebreaker
{
    [MessageType("icebreaker.start")]
    public class IcebreakerStartMessage : IMessage
    {
    }
}
