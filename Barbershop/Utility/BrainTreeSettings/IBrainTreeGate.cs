using Braintree;

namespace Barbershop.Utility.BrainTreeSettings
{
    public interface IBrainTreeGate
    {
        IBraintreeGateway CreateGateway();
        IBraintreeGateway GetGateway();
    }
}
