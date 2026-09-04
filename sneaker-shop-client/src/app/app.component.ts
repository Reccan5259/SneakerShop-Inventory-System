import { Component, OnInit } from '@angular/core';
import { Item } from './models/item';
import { Order } from './models/order';
import { ItemService } from './services/item.service';

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent implements OnInit {
  activeSection: 'dashboard' | 'inventory' | 'orders' = 'dashboard';

  items: Item[] = [];
  filteredItems: Item[] = [];
  orders: Order[] = [];

  searchTerm = '';
  message = '';

  showItemForm = false;
  editingItemId: number | null = null;

  selectedOrderItem: Item | null = null;
  orderQuantity = 1;

  formItem: Item = this.createEmptyItem();

  constructor(private itemService: ItemService) {}

  ngOnInit(): void {
    this.loadItems();
    this.loadOrders();
  }

  get totalProducts(): number {
    return this.items.length;
  }

  get totalBrands(): number {
    return new Set(this.items.map(item => item.brand)).size;
  }

  get totalStock(): number {
    return this.items.reduce(
      (total, item) => total + Number(item.quantity),
      0
    );
  }

  get inventoryValue(): number {
    return this.items.reduce(
      (total, item) =>
        total + Number(item.unitPrice) * Number(item.quantity),
      0
    );
  }

  get lowStockItems(): Item[] {
    return this.items.filter(item => item.quantity <= 5);
  }

  get orderTotal(): number {
    if (!this.selectedOrderItem) {
      return 0;
    }

    return this.selectedOrderItem.unitPrice * this.orderQuantity;
  }

  loadItems(): void {
    this.itemService.getItems().subscribe({
      next: data => {
        this.items = data;
        this.applyFilter();
        this.message = '';
      },
      error: error => {
        console.error(error);
        this.message =
          'Cannot connect to the API. Make sure ASP.NET is running on port 5000.';
      }
    });
  }

  applyFilter(): void {
    const search = this.searchTerm.trim().toLowerCase();

    if (!search) {
      this.filteredItems = [...this.items];
      return;
    }

    this.filteredItems = this.items.filter(item =>
      item.name.toLowerCase().includes(search) ||
      item.code.toLowerCase().includes(search) ||
      item.brand.toLowerCase().includes(search)
    );
  }

  openAddForm(): void {
    this.editingItemId = null;
    this.formItem = this.createEmptyItem();
    this.showItemForm = true;
  }

  openEditForm(item: Item): void {
    this.editingItemId = item.id ?? null;
    this.formItem = { ...item };
    this.showItemForm = true;
  }

  cancelItemForm(): void {
    this.showItemForm = false;
    this.editingItemId = null;
    this.formItem = this.createEmptyItem();
  }

  saveItem(): void {
    if (
      !this.formItem.name.trim() ||
      !this.formItem.code.trim() ||
      !this.formItem.brand.trim()
    ) {
      alert('Name, code, and brand are required.');
      return;
    }

    if (
      this.formItem.unitPrice <= 0 ||
      this.formItem.quantity < 0 ||
      this.formItem.size <= 0
    ) {
      alert('Enter valid price, quantity, and shoe size.');
      return;
    }

    if (this.editingItemId !== null) {
      this.itemService
        .updateItem(this.editingItemId, this.formItem)
        .subscribe({
          next: () => {
            this.cancelItemForm();
            this.loadItems();
            alert('Sneaker updated successfully.');
          },
          error: error => {
            console.error(error);
            alert('The sneaker could not be updated.');
          }
        });

      return;
    }

    this.itemService.createItem(this.formItem).subscribe({
      next: () => {
        this.cancelItemForm();
        this.loadItems();
        alert('Sneaker added successfully.');
      },
      error: error => {
        console.error(error);
        alert(
          error.error?.message ??
          'The sneaker could not be added.'
        );
      }
    });
  }

  deleteItem(item: Item): void {
    if (item.id === undefined) {
      return;
    }

    const confirmed = confirm(
      `Delete ${item.name} from the inventory?`
    );

    if (!confirmed) {
      return;
    }

    this.itemService.deleteItem(item.id).subscribe({
      next: () => {
        this.loadItems();
        alert('Sneaker deleted successfully.');
      },
      error: error => {
        console.error(error);
        alert('The sneaker could not be deleted.');
      }
    });
  }

  openOrder(item: Item): void {
    if (item.quantity <= 0) {
      alert('This sneaker is out of stock.');
      return;
    }

    this.selectedOrderItem = { ...item };
    this.orderQuantity = 1;
  }

  cancelOrder(): void {
    this.selectedOrderItem = null;
    this.orderQuantity = 1;
  }

  placeOrder(): void {
    const item = this.selectedOrderItem;

    if (!item || item.id === undefined) {
      return;
    }

    if (
      this.orderQuantity <= 0 ||
      this.orderQuantity > item.quantity
    ) {
      alert(`Enter a quantity from 1 to ${item.quantity}.`);
      return;
    }

    const updatedItem: Item = {
      ...item,
      quantity: item.quantity - this.orderQuantity
    };

    const newOrder: Order = {
      id: Date.now(),
      itemId: item.id,
      itemName: item.name,
      code: item.code,
      quantity: this.orderQuantity,
      unitPrice: item.unitPrice,
      total: item.unitPrice * this.orderQuantity,
      orderDate: new Date().toLocaleString()
    };

    this.itemService.updateItem(item.id, updatedItem).subscribe({
      next: () => {
        this.orders.unshift(newOrder);

        localStorage.setItem(
          'sneakerOrders',
          JSON.stringify(this.orders)
        );

        this.cancelOrder();
        this.loadItems();
        this.activeSection = 'orders';

        alert('Order completed successfully.');
      },
      error: error => {
        console.error(error);
        alert('The order could not be completed.');
      }
    });
  }

  private loadOrders(): void {
    const savedOrders = localStorage.getItem('sneakerOrders');

    if (savedOrders) {
      this.orders = JSON.parse(savedOrders);
    }
  }

  private createEmptyItem(): Item {
    return {
      name: '',
      code: '',
      brand: '',
      unitPrice: 0,
      quantity: 0,
      size: 0
    };
  }
}