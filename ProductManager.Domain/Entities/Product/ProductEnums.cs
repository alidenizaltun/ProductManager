namespace ProductManager.Domain.Entities.Product
{
    public enum ProductKind
    {
        Physical = 1,
        Software = 2,
        Service = 3,
        Subscription = 4,
        Bundle = 5,
        DigitalAsset = 6,
        Other = 99
    }

    public enum ProductStatus
    {
        Draft = 1,
        Active = 2,
        Passive = 3,
        Archived = 4
    }

    public enum ProductAttributeDataType
    {
        Text = 1,
        LongText = 2,
        Number = 3,
        Decimal = 4,
        Boolean = 5,
        Date = 6,
        DateTime = 7,
        Json = 8,
        SingleSelect = 9,
        MultiSelect = 10
    }

    public enum PriceType
    {
        Sale = 1,
        List = 2,
        Cost = 3,
        Campaign = 4,
        Wholesale = 5
    }

    public enum InventoryPolicy
    {
        TrackAndBlockWhenNegative = 1,
        TrackAndAllowNegative = 2,
        DoNotTrack = 3
    }

    public enum InventoryTransactionType
    {
        PurchaseReceipt = 1,
        SaleIssue = 2,
        ReturnIn = 3,
        ReturnOut = 4,
        TransferIn = 5,
        TransferOut = 6,
        Reservation = 7,
        ReservationRelease = 8,
        Adjustment = 9
    }

    public enum InventoryReservationStatus
    {
        Active = 1,
        Released = 2,
        Converted = 3,
        Expired = 4,
        Cancelled = 5
    }

    public enum MediaType
    {
        Image = 1,
        Video = 2,
        Document = 3,
        Url = 4
    }

    public enum ServiceDeliveryMode
    {
        OnSite = 1,
        Remote = 2,
        Hybrid = 3
    }

    public enum SoftwareLicenseModel
    {
        Perpetual = 1,
        Subscription = 2,
        UsageBased = 3,
        SeatBased = 4
    }

    public enum BillingPeriodUnit
    {
        Day = 1,
        Week = 2,
        Month = 3,
        Year = 4
    }
}
