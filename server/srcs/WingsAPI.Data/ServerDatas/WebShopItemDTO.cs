using PhoenixLib.DAL;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingsAPI.Data.Api.Shop
{
    [ProtoContract]
    public class WebShopItemDTO : ILongDto
    {
        [ProtoMember(1)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }

        [ProtoMember(2)]
        public int CategoryId { get; set; }

        [ProtoMember(3)]
        public string Description { get; set; }

        [ProtoMember(4)]
        public string Name { get; set; }

        [ProtoMember(5)]
        public int IconId { get; set; }

        [ProtoMember(6)]
        public long Price { get; set; }

        [ProtoMember(7)]
        public int Vnum { get; set; }
    }
}
