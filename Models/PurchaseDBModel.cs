using SQLite;
using ColumnAttribute = SQLite.ColumnAttribute;
using TableAttribute = SQLite.TableAttribute;

namespace TravelBlog.Models
{
    [Table("Purchases")]
    public class PurchaseDBModel
    {
        [Column("Id")]
        [PrimaryKey]
        [NotNull]
        [AutoIncrement]
        public int Id { get; set; }


        [Column("ProductId")]
        [NotNull]
        public string ProductId { get; set; }


        [Column("PurchaseType")]
        [NotNull]
        public string PurchaseType { get; set; }
    }
}