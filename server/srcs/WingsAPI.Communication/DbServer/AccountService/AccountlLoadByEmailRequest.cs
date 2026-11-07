using ProtoBuf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingsAPI.Communication.DbServer.AccountService
{
    [ProtoContract]
    public class AccountlLoadByEmailRequest
    {
        [ProtoMember(1)]
        public string Email { get; init; }
    }
}
