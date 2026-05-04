import { useForm } from 'react-hook-form';
import { zodResolver } from '@hookform/resolvers/zod';

export const useFormValidation = (schema, defaultValues = {}) => {
  const form = useForm({
    resolver: zodResolver(schema),
    defaultValues,
    mode: 'onBlur',
  });

  return {
    ...form,
    isSubmitting: form.formState.isSubmitting,
    errors: form.formState.errors,
  };
};