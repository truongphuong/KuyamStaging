using Kuyam.Database;
namespace Kuyam.Domain.Payments
{
    /// <summary>
    /// Represents a PostProcessPaymentRequest
    /// </summary>
    public partial class PostProcessPaymentRequest
    {        
        public Order Order { get; set; }
    }
}
