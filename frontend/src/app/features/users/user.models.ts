export type UserRole =
  | 'Administrator'
  | 'ContractManager'
  | 'Reviewer';

export type UserStatus = 'Active' | 'Inactive';

export interface UserProfile {
  jobTitle: string | null;
  department: string | null;
  phoneNumber: string | null;
}

export interface CreateUserRequest {
  firstName: string;
  lastName: string;
  email: string;
  employeeNumber: string;
  password: string;
  role: UserRole;
  companyId: number;
  profile: UserProfile | null;
}

export interface UserResponse {
  id: number;
  firstName: string;
  lastName: string;
  email: string;
  employeeNumber: string;
  role: UserRole;
  status: UserStatus;
  companyId: number;
  createdAt: string;
  profile: UserProfile | null;
}
