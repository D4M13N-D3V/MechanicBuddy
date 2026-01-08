export interface IUser {
    id: string;
    userName: string;
    email: string;
    firstName: string;
    lastName: string;
    phone?: string;
    isDefaultAdmin: boolean;
    mustChangePassword: boolean;
}
