using Plugin.InAppBilling;
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
        public ItemType PurchaseType { get; set; }
    }

    [Table("PurchaseDBModelDetail")]
    public class PurchaseDBModelDetail
    {
        [Column("Id")]
        [PrimaryKey]
        [NotNull]
        [AutoIncrement]
        public int Id { get; set; }

        [Column("PurchaseModelId")]
        [NotNull]
        public int PurchaseModelId { get; set; }

        [Column("AutoRenewing")]
        [NotNull]
        public bool AutoRenewing { get; set; }

        [Column("IsAcknowledged")]
        [NotNull]
        public bool? IsAcknowledged { get; set; }

        [Column("TransactionDateUtc")]
        [NotNull]
        public DateTime TransactionDateUtc { get; set; }
    }
}