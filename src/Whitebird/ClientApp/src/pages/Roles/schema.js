// src/pages/Roles/schema.js
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

export const roleSchema = z.object({
  roleCode: optionalString(50),
  roleName: requiredString(100),
  description: optionalString(500),
});

export default roleSchema;