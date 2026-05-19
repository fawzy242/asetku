import { z } from 'zod';

export const assetSchema = z.object({
  assetName: z.string().min(1, 'Asset name is required').max(100),
  categoryId: z.string().min(1, 'Category is required'),
  subCategory: z.string().optional(),
  assetType: z.string().optional(),
  brand: z.string().max(50).optional(),
  model: z.string().max(50).optional(),
  serialNumber: z.string().max(50).optional(),
  imei: z.string().max(50).optional(),
  macAddress: z.string().max(50).optional(),
  purchaseDate: z.string().optional(),
  purchasePrice: z.string().optional(),
  invoiceNumber: z.string().max(50).optional(),
  warrantyPeriod: z.string().optional(),
  warrantyExpiryDate: z.string().optional(),
  condition: z.string().optional(),
  status: z.string().optional(),
  location: z.string().max(100).optional(),
  currentHolderId: z.string().optional(),
  responsiblePartyId: z.string().optional(),
  supplierId: z.string().optional(),
  residualValue: z.string().optional(),
  usefulLife: z.string().optional(),
  depreciationStartDate: z.string().optional(),
  notes: z.string().max(500).optional(),
});

export const categorySchema = z.object({
  categoryName: z.string().min(1, 'Category name is required').max(100),
  description: z.string().max(500).optional(),
  parentCategoryId: z.string().optional(),
});

export const supplierSchema = z.object({
  supplierName: z.string().min(1, 'Supplier name is required').max(100),
  contactPerson: z.string().max(100).optional(),
  phoneNumber: z.string().max(20).optional(),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  address: z.string().max(500).optional(),
});

export const employeeSchema = z.object({
  fullName: z.string().min(1, 'Full name is required').max(100),
  department: z.string().max(50).optional(),
  position: z.string().max(50).optional(),
  division: z.string().max(50).optional(),
  branch: z.string().max(50).optional(),
  costCenter: z.string().max(50).optional(),
  phoneNumber: z.string().max(20).optional(),
  email: z.string().email('Invalid email').optional().or(z.literal('')),
  officeLocation: z.string().max(100).optional(),
  employmentStatus: z.string().optional(),
  joinDate: z.string().optional(),
});

export const locationSchema = z.object({
  locationName: z.string().min(1, 'Location name is required').max(100),
  locationType: z.string().max(50).optional(),
  address: z.string().max(500).optional(),
  city: z.string().max(100).optional(),
  parentLocationId: z.string().optional(),
});