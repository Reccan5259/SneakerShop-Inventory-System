export interface Order {
  id: number;
  itemId: number;
  itemName: string;
  code: string;
  quantity: number;
  unitPrice: number;
  total: number;
  orderDate: string;
}