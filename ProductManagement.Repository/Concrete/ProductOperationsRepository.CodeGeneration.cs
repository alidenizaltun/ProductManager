using Dapper;
using System.Data;

namespace ProductManagement.Repository.Concrete
{
    /// <summary>
    /// Sistem tarafından üretilen kodlar. Ekleme ekranlarında kod alanı sorulmaz;
    /// istek içinde kod boş gelirse ilgili tablonun son sıra numarası bulunup
    /// "&lt;önek&gt;-&lt;6 haneli sıra&gt;" biçiminde (ör. CAT-000001) yeni kod üretilir.
    /// </summary>
    public sealed partial class ProductOperationsRepository
    {
        private const int GeneratedCodeDigits = 6;

        /// <summary>Kod üretiminde kullanılan tablo/kolon/önek üçlüsü. Değerler yalnızca aşağıdaki sabitlerden gelir.</summary>
        private sealed record GeneratedCodeSource(string Table, string Column, string Prefix);

        private static readonly GeneratedCodeSource ProductCodeSource = new("[Product].[Products]", "ProductCode", "PRD-");
        private static readonly GeneratedCodeSource CategoryCodeSource = new("[Product].[ProductCategories]", "Code", "CAT-");
        private static readonly GeneratedCodeSource SupplierCodeSource = new("[Product].[ProductSuppliers]", "SupplierCode", "SUP-");
        private static readonly GeneratedCodeSource WarehouseCodeSource = new("[Product].[Warehouses]", "Code", "WH-");
        private static readonly GeneratedCodeSource PriceListCodeSource = new("[Product].[ProductPriceLists]", "Code", "PL-");
        private static readonly GeneratedCodeSource UnitDefinitionCodeSource = new("[Product].[UnitDefinitions]", "Code", "UNIT-");
        private static readonly GeneratedCodeSource RegionCodeSource = new("[Product].[Regions]", "Code", "REG-");
        private static readonly GeneratedCodeSource PricingTemplateCodeSource = new("[Product].[PricingTemplates]", "Code", "TPL-");
        private static readonly GeneratedCodeSource PriceRevisionCodeSource = new("[Product].[PriceRevisions]", "Code", "ZAM-");

        private static string? NormalizeRequestedCode(string? requestedCode)
            => string.IsNullOrWhiteSpace(requestedCode) ? null : requestedCode.Trim();

        /// <summary>
        /// Kodu çözer (istekte varsa onu, yoksa üretileni kullanır) ve eklemeyi aynı işlem
        /// içinde yapar. Sıra numarası okunurken alınan UPDLOCK/HOLDLOCK, eşzamanlı iki
        /// eklemenin aynı numarayı almasını engeller.
        /// </summary>
        private async Task<Guid> InsertWithGeneratedCodeAsync(
            string? requestedCode,
            GeneratedCodeSource source,
            Func<IDbConnection, IDbTransaction, string, CancellationToken, Task<Guid>> insertAsync,
            CancellationToken cancellationToken)
        {
            var explicitCode = NormalizeRequestedCode(requestedCode);

            using var connection = CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                var code = explicitCode
                    ?? await GenerateNextCodeAsync(connection, transaction, source, cancellationToken);

                var id = await insertAsync(connection, transaction, code, cancellationToken);
                transaction.Commit();
                return id;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// Verilen önekle başlayan mevcut en yüksek sıra numarasını bulup bir sonrakini üretir.
        /// Öneke uymayan ya da sayısal olmayan eski kodlar hesaplamaya girmez.
        /// </summary>
        private static async Task<string> GenerateNextCodeAsync(
            IDbConnection connection,
            IDbTransaction? transaction,
            GeneratedCodeSource source,
            CancellationToken cancellationToken)
        {
            // Tablo ve kolon adları yalnızca yukarıdaki sabit GeneratedCodeSource tanımlarından
            // gelir; istek verisi asla SQL metnine gömülmez.
            var sql = $@"
SELECT ISNULL(MAX(TRY_CONVERT(int, SUBSTRING({source.Column}, @PrefixLength + 1, 9))), 0)
FROM {source.Table} WITH (UPDLOCK, HOLDLOCK)
WHERE {source.Column} LIKE @Pattern;";

            var lastSequence = await connection.ExecuteScalarAsync<int>(
                new CommandDefinition(
                    sql,
                    new
                    {
                        PrefixLength = source.Prefix.Length,
                        Pattern = $"{source.Prefix}[0-9]%"
                    },
                    transaction,
                    cancellationToken: cancellationToken));

            return $"{source.Prefix}{(lastSequence + 1).ToString($"D{GeneratedCodeDigits}")}";
        }
    }
}
