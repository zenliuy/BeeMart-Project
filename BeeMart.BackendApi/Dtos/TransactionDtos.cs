namespace BeeMart.BackendApi.Dtos;
    public class CreateTransactionRequest
    {
    public int IdPaymentMethod { get; set; }
    public List<TranscantionItemRequest>? Items { get; set; }
    }

    public class TranscantionItemRequest
    {
        public string? IdProduct { get; set; }
        public int Qty { get; set; }
    }

