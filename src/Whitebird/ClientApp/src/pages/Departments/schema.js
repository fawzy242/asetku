// src/pages/Departments/schema.js
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

export const departmentSchema = z.object({
  departmentCode: optionalString(100),
  departmentName: requiredString(100),
  description: optionalString(500),
});

export default departmentSchema;