export interface Party {
  id: number;
  partyType: string;
  name: string;
  email: string;
  phone: string;
  nationalId: string | null;
  registrationNumber: string | null;
  contactPerson: string | null;
}

export interface CreateIndividualPartyRequest {
  name: string;
  nationalId: string;
  email: string;
  phone: string;
}

export interface CreateOrganizationPartyRequest {
  organizationName: string;
  registrationNumber: string;
  contactPerson: string;
  email: string;
  phone: string;
}