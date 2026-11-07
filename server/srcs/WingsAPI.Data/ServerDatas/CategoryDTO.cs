using PhoenixLib.DAL;
using ProtoBuf;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WingsAPI.Data.Api.Category
{
    [ProtoContract]
    public class CategoryDTO : ILongDto
    {
        [ProtoMember(2)]
        public string Name { get; set; }

        [ProtoMember(1)]
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
