import { AuthResponse } from '@auth/interfaces/auth-response.interface';

export const mockAuthResponse: AuthResponse = {
  user: {
    id: 'mock-user-1',
    email: 'admin@tesloshop.dev',
    fullName: 'Teslo Shop Admin',
    isActive: true,
    roles: ['admin'],
  },
  token: 'mock-auth-token',
};
