import { User } from "../../core/types/user";

export type AuthResponse = {
  user: User;
  accessToken: string;
  refreshToken: string;
};
