import { CanActivateFn, Router } from '@angular/router';
import { CartService } from '../services/cart.service';
import { inject } from '@angular/core/primitives/di';
import { SnackbarService } from '../services/snackbar.service';

export const emptyCartGuard: CanActivateFn = (route, state) => {
  const cartService = inject(CartService);
  const snackbarService = inject(SnackbarService);
  const router = inject(Router);

  if(!cartService.cart() || cartService.cart()?.items.length === 0){
    snackbarService.error('Your cart is empty.');
    router.navigateByUrl('/cart');
    return false;
  }

  return true;
};
