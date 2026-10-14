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

export const PARTY_ROLES = [
  'Customer',
  'Supplier',
  'Partner',
  'Consultant',
] as const;

export type PartyRole = (typeof PARTY_ROLES)[number];

export const DOCUMENT_TYPES = [
  'MainContract',
  'CommercialRegistration',
  'TechnicalProposal',
  'FinancialProposal',
  'Other',
] as const;

export type DocumentType = (typeof DOCUMENT_TYPES)[number];

export interface AddContractPartyRequest {
  partyId: number;
  partyRole: PartyRole;
}

export interface AddContractDocumentRequest {
  fileName: string;
  documentType: DocumentType;
  filePath: string | null;
}

export interface AddedContractParty extends AddContractPartyRequest {
  name: string;
}

export interface ContractPartyResponse {
  partyId: number;
  name: string;
  partyRole: PartyRole;
  addedDate: string;
}

export interface ContractDocumentResponse {
  id: number;
  fileName: string;
  documentType: DocumentType;
  filePath: string | null;
  uploadedById: number;
  uploadedDate: string;
}

export interface ContractDetailsResponse extends ContractResponse {
  companyName: string;
  ownerName: string;
  parties: ContractPartyResponse[];
  documents: ContractDocumentResponse[];
}