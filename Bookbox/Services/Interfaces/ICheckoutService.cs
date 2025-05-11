using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Bookbox.DTOs;
using Bookbox.Constants;

namespace Bookbox.Services.Interfaces
{
    public interface ICheckoutService
    {
        /// <summary>
        /// Prepares checkout data from the user's cart
        /// </summary>
        /// <param name="userId">The ID of the user checking out</param>
        /// <returns>A populated CheckoutDTO with all details needed for checkout</returns>
        Task<CheckoutDTO> PrepareCheckoutFromCartAsync(Guid userId);
        
        /// <summary>
        /// Prepares checkout data from selected items in the user's cart
        /// </summary>
        /// <param name="userId">The ID of the user checking out</param>
        /// <param name="selectedItems">List of selected cart item IDs to include in checkout</param>
        /// <returns>A populated CheckoutDTO with details for selected items</returns>
        Task<CheckoutDTO> PrepareCheckoutFromCartAsync(Guid userId, List<Guid> selectedItems);
        
        /// <summary>
        /// Checks if a user is eligible for a loyalty discount (10% after every 10 successful orders)
        /// </summary>
        /// <param name="userId">The ID of the user to check</param>
        /// <returns>True if the user is eligible for a loyalty discount</returns>
        Task<bool> CheckLoyaltyDiscountEligibilityAsync(Guid userId);
        
        /// <summary>
        /// Generates a unique claim code for the checkout confirmation
        /// </summary>
        /// <returns>A unique claim code string</returns>
        string GenerateClaimCode();
        
        /// <summary>
        /// Confirms the checkout and prepares the confirmation data
        /// </summary>
        /// <param name="checkoutData">The checkout data to confirm</param>
        /// <param name="paymentMethod">The selected payment method</param>
        /// <param name="notes">Any additional notes</param>
        /// <returns>The finalized checkout data with claim code</returns>
        Task<CheckoutDTO> ConfirmCheckoutAsync(CheckoutDTO checkoutData, PaymentMethod paymentMethod, string notes);
    }
}