import { User } from './user.model';

export type LoginResponse = {
	token: string;
	user: User;
};
