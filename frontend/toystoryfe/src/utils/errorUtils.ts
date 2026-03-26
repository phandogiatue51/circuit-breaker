import axios from 'axios';

export const getApiErrorMessage = (err: unknown, fallback: string): string => {
  if (!axios.isAxiosError(err)) {
    return fallback;
  }

  const status = err.response?.status;
  const data = err.response?.data as any;
  const validationErrors = data?.errors;
  const validationMessage = Array.isArray(validationErrors)
    ? validationErrors.flatMap((item: any) => Object.values(item)).filter(Boolean).join(', ')
    : '';

  const rawMessage = typeof data?.message === 'string' ? data.message : '';
  const title = typeof data?.title === 'string' ? data.title : '';
  const detail = typeof data?.detail === 'string' ? data.detail : '';

  const message = [detail, validationMessage, title]
    .find((item) => item && item.trim().length > 0 && item !== 'Success')
    || (rawMessage && rawMessage !== 'Success' ? rawMessage : '');

  if (message) {
    return message;
  }

  if (status) {
    return `${status} ${fallback}`;
  }

  return fallback;
};
