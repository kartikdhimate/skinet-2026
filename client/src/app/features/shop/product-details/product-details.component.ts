import { Component, inject, OnInit, signal } from '@angular/core';
import { ShopService } from '../../../core/services/shop.service';
import { ActivatedRoute } from '@angular/router';
import { Product } from '../../../shared/models/products';
import { CurrencyPipe } from '@angular/common';
import { MatButton } from '@angular/material/button';
import { MatIcon } from '@angular/material/icon';
import { MatFormField, MatLabel } from '@angular/material/select';
import { MatInput } from '@angular/material/input';
import { MatDivider } from '@angular/material/divider';
import { CartService } from '../../../core/services/cart.service';
import { FormsModule } from '@angular/forms';

@Component({
  selector: 'app-product-details',
  imports: [
    CurrencyPipe,
    MatButton,
    MatIcon,
    MatFormField,
    MatInput,
    MatLabel,
    MatDivider,
    FormsModule
  ],
  templateUrl: './product-details.component.html',
  styleUrl: './product-details.component.css',
})
export class ProductDetailsComponent implements OnInit {
  private shopService = inject(ShopService);
  private activatedRoute = inject(ActivatedRoute);
  private cartService = inject(CartService);

  product = signal<Product | undefined>(undefined);
  quantityInCart = 0;
  quantity = 1;

  ngOnInit(): void {
    this.loadProduct();
  }

  loadProduct() {
    const id = this.activatedRoute.snapshot.paramMap.get('id');
    if (!id) return;

    this.shopService.getProduct(+id).subscribe({
      next: product => {
        this.product.set(product);
        this.updateQuantityInCart();
      },
      error: error => console.log(error),
    });
  }

  updateCart() {
    const product = this.product();
    if (!product) return;

    if (this.quantity > this.quantityInCart) {
      const quantityToAdd = this.quantity - this.quantityInCart;
      this.quantityInCart += quantityToAdd;
      this.cartService.addItemToCart(product, quantityToAdd);
    } else {
      const quantityToRemove = this.quantityInCart - this.quantity;
      this.quantityInCart -= quantityToRemove;
      this.cartService.removeItemFromCart(product.id, quantityToRemove);
    }
  }

  updateQuantityInCart() {
    this.quantityInCart = this.cartService.cart()?.items.find(i => i.productId === this.product()?.id)?.quantity || 0;
    this.quantity = this.quantityInCart || 1;
  }

  getButtonText() {
    return this.quantityInCart > 0 ? 'Update Cart' : 'Add to Cart';
  }
}  
