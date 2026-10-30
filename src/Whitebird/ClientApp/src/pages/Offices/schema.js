// src/pages/Offices/schema.js
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

const optionalPositiveInt = () =>
  z.preprocess(
    (val) => (val === '' || val === undefined ? null : val),
    z.coerce
      .number()
      .int('Must be an integer')
      .min(1, 'Must be a valid ID')
      .nullable()
  );

export const officeSchema = z.object({
  officeCode: optionalString(50),
  officeName: requiredString(100),
  officeType: optionalPositiveInt(),
  city: optionalString(100),
  address: optionalString(500),
  phone: optionalString(50),
  parentOfficeId: optionalPositiveInt(),
});

export default officeSchema;