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
  return z.string().email('Invalid email format').optional().or(z.literal('')).or(z.null());
};

export const employeeSchema = z.object({
  // Required fields
  employeeCode: stringField(50, true),
  fullName: stringField(100, true),
  
  // Optional fields
  address: stringField(300),
  phoneNumber: stringField(20),
  email: emailField(),
  
  // ID fields (FK)
  departmentId: numberField(),
  position: numberField(),
  employmentStatus: numberField(),
  officeId: numberField(),
  
  // Date fields
  joinDate: dateField(),
  resignDate: dateField(),
});

export type EmployeeFormData = z.infer<typeof employeeSchema>;

export default employeeSchema;