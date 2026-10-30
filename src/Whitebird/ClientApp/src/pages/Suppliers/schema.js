// src/pages/Suppliers/schema.js
import { z } from 'zod';

const optionalString = (max) =>
  z
    .string()
    .max(max, `Maximum ${max} characters`)
    .optional()
    .or(z.literal(''))
    .or(z.null());

const requiredString = (max) =>
  z
    .string()
    .min(1, 'This field is required')
    .max(max, `Maximum ${max} characters`);

const optionalEmail = () =>
  z
    .string()
    .max(100, 'Maximum 100 characters')
    .email('Invalid email format')
    .optional()
    .or(z.literal(''))
    .or(z.null());

export const supplierSchema = z.object({
  supplierName: requiredString(100),
  contactPerson: optionalString(100),
  phoneNumber: optionalString(20),
  email: optionalEmail(),
  address: optionalString(500),
});

export default supplierSchema;