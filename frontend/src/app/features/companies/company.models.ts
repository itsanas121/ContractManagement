export interface CreateCompanyRequest {
  name: string;
  registrationNumber: string;
  email: string | null;
  phone: string | null;
  address: string | null;
}

export interface CompanyResponse {
  id: number;
  name: string;
  registrationNumber: string;
  email: string | null;
  phone: string | null;
  address: string | null;
  status: 'Active' | 'Inactive';
  createdAt: string;
}
