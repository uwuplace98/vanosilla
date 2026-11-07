using PhoenixLib.DAL;
using PhoenixLib.DAL.EFCore.PGSQL;
using Plugin.Database.DB;
using Plugin.Database.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plugin.Database.Api.Shop
{
    [Table("shopitems", Schema = DatabaseSchemas.API)]

    public class DbShopItemEntity : BaseAuditableEntity, ILongEntity
    {
        public int CategoryId { get; set; }
        public string Description { get; set; }
        public string Name { get; set; }
        public int IconId { get; set; }
        public long Price { get; set; }
        public int Vnum { get; set; }

        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public long Id { get; set; }
    }
}
