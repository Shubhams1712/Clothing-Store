import { API_CONFIG } from "@/config/api";
import { api } from "@/lib/api";
import type {
  AuthResponse,
  RegisterRequest,
  LoginRequest,
  ForgotPasswordRequest,
  ResetPasswordRequest,
  VerifyEmailRequest,
  ChangePasswordRequest,
  UpdateProfileRequest,
  UserProfile,
} from "@/types/auth";

export const authApi = {
  register: async (data: RegisterRequest) => {
    const response = await api.post(
      API_CONFIG.ENDPOINTS.AUTH.REGISTER,
      data
    );
    return response.data;
  },

  login: async (data: LoginRequest) => {
    const response = await api.post<{ data: AuthResponse }>(
      API_CONFIG.ENDPOINTS.AUTH.LOGIN,
      data
    );
    return response.data.data;
  },

  adminLogin: async (data: LoginRequest) => {
    const response = await api.post<{ data: AuthResponse }>(
      API_CONFIG.ENDPOINTS.AUTH.ADMIN_LOGIN,
      data
    );
    return response.data.data;
  },

  logout: async () => {
    const response = await api.post(API_CONFIG.ENDPOINTS.AUTH.LOGOUT);
    return response.data;
  },

  refresh: async (refreshToken: string) => {
    const response = await api.post<{ data: AuthResponse }>(
      API_CONFIG.ENDPOINTS.AUTH.REFRESH,
      { refreshToken }
    );
    return response.data.data;
  },

  forgotPassword: async (data: ForgotPasswordRequest) => {
    const response = await api.post(
      API_CONFIG.ENDPOINTS.AUTH.FORGOT_PASSWORD,
      data
    );
    return response.data;
  },

  resetPassword: async (data: ResetPasswordRequest) => {
    const response = await api.post(
      API_CONFIG.ENDPOINTS.AUTH.RESET_PASSWORD,
      data
    );
    return response.data;
  },

  verifyEmail: async (data: VerifyEmailRequest) => {
    const response = await api.post(
      API_CONFIG.ENDPOINTS.AUTH.VERIFY_EMAIL,
      data
    );
    return response.data;
  },

  getProfile: async () => {
    const response = await api.get<{ data: UserProfile }>(
      API_CONFIG.ENDPOINTS.USERS.ME
    );
    return response.data.data;
  },

  updateProfile: async (data: UpdateProfileRequest) => {
    const response = await api.put<{ data: UserProfile }>(
      API_CONFIG.ENDPOINTS.USERS.ME,
      data
    );
    return response.data.data;
  },

  changePassword: async (data: ChangePasswordRequest) => {
    const response = await api.put(
      API_CONFIG.ENDPOINTS.USERS.CHANGE_PASSWORD,
      data
    );
    return response.data;
  },
};
