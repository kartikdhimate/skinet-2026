using Core.Entities;
using Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Stripe;
using Product = Core.Entities.Product;

namespace Infrastructure.Services;

public class PaymentService(IConfiguration config, ICartService cartService,
    IUnitOfWork unitOfWork) : IPaymentService
{
    public async Task<ShoppingCart?> CreateOrUpdatePaymentIntent(string cartId)
    {
        StripeConfiguration.ApiKey = config["StripeSettings:SecretKey"];

        var cart = await cartService.GetCartAsync(cartId);

        if (cart == null)
            return null;

        var shippingPrice = 0m;

        if(cart.DeliveryMethodId.HasValue)
        {
            var deliveryMethod = await unitOfWork.Repository<DeliveryMethod>().GetByIdAsync(cart.DeliveryMethodId.Value);
            if(deliveryMethod == null)
                return null;
            
            shippingPrice = deliveryMethod.Price;
        }

        foreach(var item in cart.Items)
        {
            var productItem = await unitOfWork.Repository<Product>().GetByIdAsync(item.ProductId);
            if(productItem == null)
                return null;
            
            if(item.Price != productItem.Price)
            {
                item.Price = productItem.Price;
            }
        }

        var service = new PaymentIntentService();
        PaymentIntent? intent = null;

        if (string.IsNullOrEmpty(cart.PaymentIntentId))
        {
            var options = new PaymentIntentCreateOptions
            {
                // Multiply by 100 because Stripe expects the amount in smallest currency unit (e.g., cents for USD)
                // We are using decimal for price in our application, so we need to convert it to long (integer) for Stripe
                Amount = (long)cart.Items.Sum(x=>x.Quantity* (x.Price*100)) + (long)shippingPrice *100,
                Currency = "usd",
                PaymentMethodTypes = ["card"]
            };

            intent = await service.CreateAsync(options);
            cart.PaymentIntentId = intent.Id;
            cart.ClientSecret = intent.ClientSecret;
        }
        else
        {
            var options = new PaymentIntentUpdateOptions
            {
                Amount = (long)cart.Items.Sum(x => x.Quantity * (x.Price * 100)) + (long)shippingPrice * 100
            };
            intent = await service.UpdateAsync(cart.PaymentIntentId, options);
        }

        await cartService.SetCartAsync(cart);

        return cart;
    }
}
