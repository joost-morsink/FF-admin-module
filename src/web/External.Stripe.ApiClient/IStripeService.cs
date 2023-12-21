using System.Threading.Tasks;
using Stripe;

namespace External.Stripe.ApiClient;

public interface IStripeService
{
    Task<StripeInformation> GetConvertedAmount(string paymentIntentId);
}
public record StripeInformation(PaymentIntent? PaymentIntent, Charge? Charge, BalanceTransaction? BalanceTransaction, decimal? ConvertedAmount);
