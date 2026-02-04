/**
 * Point of Sale Types - EP8
 * Type definitions for point of sale management
 */

export interface PointOfSale {
  id: string;
  name: string;
  code: string;
  address?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
  createdAt: string;
  updatedAt: string;
}

export interface CreatePointOfSaleRequest {
  name: string;
  code: string;
  address?: string;
  phone?: string;
  email?: string;
}

export interface UpdatePointOfSaleRequest {
  name: string;
  address?: string;
  phone?: string;
  email?: string;
  isActive: boolean;
}

export interface PointOfSaleStatusRequest {
  isActive: boolean;
}

export interface UserPointOfSale {
  id: string;
  userId: string;
  pointOfSaleId: string;
  pointOfSale?: PointOfSale;
  user?: {
    id: string;
    username: string;
    firstName: string;
    lastName: string;
    email?: string;
    role: string;
  };
  isActive: boolean;
  assignedAt: string;
  unassignedAt?: string;
}
