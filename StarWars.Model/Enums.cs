namespace StarWars.Model
{
    public enum PaymentMethod
    {
        Card = 1,
        HICAPS = 2,
        BPAY = 3,
        Invoice = 4
    }

    public enum Status
    {
        Pending = 1,
        Packed = 2,
        Delivered = 3,
        Shipped = 4,
        Cancelled = 5
    }

    public enum DeliveryType
    {
        ClickAndCollect = 1,
        Standard = 2,
        Express = 3
    }
}
