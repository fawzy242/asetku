// src/pages/Employees/schema.js
import { z } from 'zod';

const stringField = (max = 100, required = false) => {
  let schema = z.string().max(max, `Maximum ${max} characters`);
  if (!required) schema = schema.optional().or(z.literal('')).or(z.null());
  else schema = schema.min(1, 'This field is required');
  return schema;
};

const numberField = (required = false) => {
  let schema = z.coerce.number().optional().or(z.null());
  if (required) schema = z.coerce.number().min(1, 'This field is required');
  return schema;
};

const dateField = () => {
  return z.string().optional().or(z.null());
};

const emailField = () => {
  return z
    .string()
    .email('Invalid email format')
    .optional()
    .or(z.literal(''))
    .or(z.null());
};

export const employeeSchema = z.object({
  employeeCode: stringField(50, true),
  fullName: stringField(100, true),
  address: stringField(300),
  phoneNumber: stringField(20),
  email: emailField(),
  departmentId: numberField(),
  position: numberField(),
  employmentStatus: numberField(),
  officeId: numberField(),
  joinDate: dateField(),
  resignDate: dateField(),
  // Read-only display field — resolved photo URL, never sent to backend.
  photoProfileUrl: z.string().nullable().optional(),
  // Backend requires isActive on update. Defaulted to true for safety;
  // handleEdit overrides with the fetched record's actual state.
  isActive: z.boolean().default(true),
});

export const getEmployeeDefaultValues = () => ({
  employeeCode: '',
  fullName: '',
  address: '',
  departmentId: '',
  position: '',
  employmentStatus: '',
  phoneNumber: '',
  email: '',
  officeId: '',
  joinDate: '',
  resignDate: '',
  photoProfileUrl: '',
  isActive: true,
});

export default employeeSchema;