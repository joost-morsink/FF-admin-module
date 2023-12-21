using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using Stripe;

namespace External.Stripe.ApiClient;

public class StripeService : IStripeService
{
    private readonly StripeApiOptions _options;
    private readonly RequestOptions _requestOptions;

    public StripeService(IOptions<StripeApiOptions> options)
    {
        _options = options.Value;
        _requestOptions = new RequestOptions {ApiKey = _options.ApiKey};
    }

    private Task<PaymentIntent?> GetPaymentIntent(string paymentIntentId)
    {
        var service = new PaymentIntentService();
        return service.GetAsync(paymentIntentId, null, _requestOptions);
    }
    private Task<BalanceTransaction?> GetBalanceTransaction(string balanceTransactionId)
    {
        var service = new BalanceTransactionService();
        return service.GetAsync(balanceTransactionId, null, _requestOptions);
    }
    private Task<Charge?> GetCharge(string chargeId)
    {
        var service = new ChargeService();
        return service.GetAsync(chargeId, null, _requestOptions);
    }
    public async Task<StripeInformation> GetConvertedAmount(string paymentIntentId)
    {
        var intent = await GetPaymentIntent(paymentIntentId);
        if (intent is null || intent.LatestChargeId is null)
            return new (intent, null,null,null);
        var charge = await GetCharge(intent.LatestChargeId);
        if (charge is null || !charge.Paid || charge.BalanceTransactionId is null)
            return new(intent, charge, null, null);
        var transaction = await GetBalanceTransaction(charge.BalanceTransactionId);
        return new (intent, charge, transaction, transaction?.Amount / 100m); // If costs should be paid by customer use transaction?.Net/100m
    }
}
