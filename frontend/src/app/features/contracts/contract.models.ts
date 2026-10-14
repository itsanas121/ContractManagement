export const CONTRACT_TYPES = [
  'Sales',
  'Procurement',
  'Service',
  'Partnership',
  'Other',
] as const;

export type ContractType = (typeof CONTRACT_TYPES)[number];

export type ContractStatus =
  | 'Draft'
  | 'UnderReview'
  | 'Approved'
  | 'Rejected'
  | 'Active'
  | 'Expired'
  | 'Terminated';

export interface CreateContractRequest {
  contractNumber: string;
  title: string;
  description: string | null;
  companyId: number;
  ownerId: number;
  contractValue: number;
  startDate: string;
  endDate: string;
  contractType: ContractType;
}

export interface ContractResponse {
  id: number;
  contractNumber: string;
  title: string;
  description: string | null;
  contractValue: number;
  startDate: string;
  endDate: string;
  contractType: ContractType;
  status: ContractStatus;
  companyId: number;
  ownerId: number;
}